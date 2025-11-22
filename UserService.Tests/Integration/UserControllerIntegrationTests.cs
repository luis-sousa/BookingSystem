using FluentAssertions;
using Humanizer;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http.Json;
using UserService;
using UserService.Data;
using UserService.DTOs;
using UserService.Middleware;
using UserService.Models;
using Xunit;

namespace UserService.Tests.Integration
{
    public class UserControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UserControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        // =============================
        // CREATE CLIENT + PROVIDER
        // =============================
        private (HttpClient client, IServiceProvider provider) CreateClient(string dbName)
        {
            IServiceProvider provider = null!;

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove DbContext existente
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));
                    if (descriptor != null)
                        services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<UserDbContext>)));

                    // Adiciona InMemory database
                    services.AddDbContext<UserDbContext>(options =>
                        options.UseInMemoryDatabase(dbName));

                    // Adiciona AutoMapper
                    services.AddAutoMapper(typeof(UserService.Mapping.UserProfile).Assembly);

                    // Build provider local
                    provider = services.BuildServiceProvider();
                });
            }).CreateClient();

            return (client, provider);
        }

        // =============================
        // CREATE USER + LOGIN
        // =============================
        private async Task<string> CreateUserAndLoginAsync(HttpClient client, IServiceProvider provider, string role, string username = "testuser", string email = "test@test.com", string password = "Test123!")
        {
            var dto = new CreateUserDto
            {
                Username = username,
                Email = email,
                Password = password
            };

            // Criar user via API
            var createResponse = await client.PostAsJsonAsync("/api/v1/User", dto);
            createResponse.EnsureSuccessStatusCode();

            // Obter DbContext do mesmo provider do client
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

            var user = db.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null)
                throw new InvalidOperationException($"user com email {dto.Email} não foi criado.");

            // Atribuir role
            user.Role = role;
            db.SaveChanges();

            // Login via API
            var loginDto = new LoginDto { Email = email, Password = password };
            var loginResponse = await client.PostAsJsonAsync("/api/v1/User/login", loginDto);
            loginResponse.EnsureSuccessStatusCode();

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthDto>();
            return auth!.Token;
        }

        private async Task<(int userId, string token, string password)> CreateUserAndLoginForTest(HttpClient client, IServiceProvider provider, string role)
        {
            var password = "Test123!";
            var dto = new CreateUserDto
            {
                Username = "testuser",
                Email = "test@test.com",
                Password = password
            };

            // Criar user
            var createResponse = await client.PostAsJsonAsync("/api/v1/User", dto);
            createResponse.EnsureSuccessStatusCode();

            // Atribuir role diretamente no DB
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var user = db.Users.First(u => u.Email == dto.Email);
            user.Role = role;
            db.SaveChanges();

            // Login
            var loginDto = new LoginDto { Email = dto.Email, Password = dto.Password };
            var loginResponse = await client.PostAsJsonAsync("/api/v1/User/login", loginDto);
            loginResponse.EnsureSuccessStatusCode();

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthDto>();

            return (user.IdUser, auth!.Token, password);
        }

        // =============================
        // TESTES
        // =============================

        [Fact]
        public async Task CreateUserAndLogin_ShouldReturnToken()
        {
            var (client, provider) = CreateClient("TestDb1");
            var token = await CreateUserAndLoginAsync(client, provider, "User");
            token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_InvalidPassword_ShouldReturnNotFound()
        {
            var (client, provider) = CreateClient("TestDb2");

            var dto = new CreateUserDto
            {
                Username = "invalidpass",
                Email = "invalidpass@test.com",
                Password = "Test123!"
            };
            var createResponse = await client.PostAsJsonAsync("/api/v1/User", dto);
            createResponse.EnsureSuccessStatusCode();

            var loginDto = new LoginDto { Email = dto.Email, Password = "WrongPass" };
            var loginResponse = await client.PostAsJsonAsync("/api/v1/User/login", loginDto);

            loginResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        //[Fact]
        //public async Task UpdatePassword_Valid_ShouldReturnNoContent()
        //{
        //    // 1️⃣ Criar cliente com InMemory DB
        //    var (client, provider) = CreateClient("TestDb3");

        //    // 2️⃣ Criar user e login, obtendo userId, token e password original
        //    var (userId, token, password) = await CreateUserAndLoginForTest(client, provider, "User");

        //    // 3️⃣ Configurar header de autenticação
        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //    // 4️⃣ DTO para atualização de password
        //    var updateDto = new UpdatePasswordDto
        //    {
        //        CurrentPassword = password,      // password correta
        //        NewPassword = "NewPass123!"      // password válida para passar validação
        //    };

        //    // 5️⃣ Chamada da API
        //    var patchResponse = await client.PatchAsJsonAsync($"/api/v1/User/{userId}/password", updateDto);

        //    // 6️⃣ Verificação de sucesso
        //    patchResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        //    // 7️⃣ Opcional: verificar no banco se a password foi realmente atualizada
        //    using var scope = provider.CreateScope();
        //    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        //    var updatedUser = db.Users.First(u => u.IdUser == userId);

        //    //updatedUser.PasswordHash.Should().NotBe(password); // a password foi alterada
        //}

        [Fact]
        public async Task UpdatePassword_InvalidCurrent_ShouldReturnBadRequest()
        {
            var (client, provider) = CreateClient("TestDb4");
            var token = await CreateUserAndLoginAsync(client, provider, "User");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updateDto = new UpdatePasswordDto
            {
                CurrentPassword = "WrongPass!",
                NewPassword = "NewPass123!"
            };

            var userId = 1;
            var patchResponse = await client.PatchAsJsonAsync($"/api/v1/User/{userId}/password", updateDto);
            patchResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteUser_AsAdmin_ShouldReturnNoContent()
        {
            var (client, provider) = CreateClient("TestDb5");
            var token = await CreateUserAndLoginAsync(client, provider, "Admin");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var userId = 1;
            var deleteResponse = await client.DeleteAsync($"/api/v1/User/{userId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteUser_NonExistent_ShouldReturnBadRequest()
        {
            var (client, provider) = CreateClient("TestDb6");
            var token = await CreateUserAndLoginAsync(client, provider, "Admin");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var nonExistentUserId = 999;
            var deleteResponse = await client.DeleteAsync($"/api/v1/User/{nonExistentUserId}");

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var error = await deleteResponse.Content.ReadFromJsonAsync<BusinessException>();
            error!.Code.Should().Be(4001);
            error.Message.Should().Be("Utilizador não encontrado");
        }

        [Fact]
        public async Task DeleteUser_AsNonAdmin_ShouldReturnForbidden()
        {
            var (client, provider) = CreateClient("TestDb7");
            var token = await CreateUserAndLoginAsync(client, provider, "User");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var deleteResponse = await client.DeleteAsync($"/api/v1/User/1");

            // utulizador autenticado mas sem permissão -> 403 Forbidden
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
