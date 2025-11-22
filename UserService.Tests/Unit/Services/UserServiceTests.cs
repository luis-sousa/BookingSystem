using AutoMapper;
using Moq;
using UserService.DTOs;
using UserService.Middleware;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;
using UserService.Validators;

namespace UserService.Tests.Unit.Services
{
    public class UserAuthServiceTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly Mock<IJwtService> _jwt = new();
        private readonly IMapper _mapper;

        private readonly CreateUserValidator _createValidator = new();
        private readonly UpdateUserValidator _updateValidator = new();
        private readonly UpdatePasswordValidator _passwordValidator = new();
        private readonly LoginValidator _loginValidator = new();

        public UserAuthServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UserService.Mapping.UserProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private UserAuthService CreateService() =>
            new UserAuthService(
                _repo.Object,
                _jwt.Object,
                _mapper,
                _createValidator,
                _updateValidator,
                _passwordValidator,
                _loginValidator
            );

        // ----------------------------------------------------------
        // CREATE
        // ----------------------------------------------------------

        [Fact]
        public async Task CreateAsync_ThrowsBusinessException_WhenEmailExists()
        {
            // ARRANGE
            var service = CreateService();
            var dto = new CreateUserDto
            {
                Username = "test",
                Email = "test@test.com",
                Password = "123456"
            };

            _repo.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(true);

            // ACT + ASSERT
            var ex = await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(dto));
            Assert.Equal(4002, ex.Code);
        }

        [Fact]
        public async Task CreateAsync_ThrowsBusinessException_WhenValidationFails()
        {
            // ARRANGE
            var service = CreateService();
            var dto = new CreateUserDto
            {
                Username = "",
                Email = "invalid",
                Password = "123"
            };

            // ACT + ASSERT
            await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(dto));
        }

        // ----------------------------------------------------------
        // LOGIN
        // ----------------------------------------------------------

        [Fact]
        public async Task LoginAsync_ThrowsBusinessException_WhenUserNotFound()
        {
            // ARRANGE
            var service = CreateService();

            _repo.Setup(r => r.GetByEmailAsync("a@b.com"))
                 .ReturnsAsync((User?)null);

            // ACT + ASSERT
            await Assert.ThrowsAsync<BusinessException>(() => service.LoginAsync("a@b.com", "pass"));
        }

        [Fact]
        public async Task LoginAsync_ReturnsToken_WhenValid()
        {
            // ARRANGE
            var service = CreateService();

            var user = new User
            {
                IdUser = 1,
                Email = "a@b.com",
                Username = "test",
                Role = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            };

            _repo.Setup(r => r.GetByEmailAsync(user.Email))
                 .ReturnsAsync(user);

            _jwt.Setup(j => j.GenerateToken(It.IsAny<UserDto>(), "User"))
                .Returns("fake-token");

            // ACT
            var result = await service.LoginAsync("a@b.com", "123456");

            // ASSERT
            Assert.Equal("fake-token", result.Token);
            Assert.Equal("a@b.com", result.User.Email);
        }

        // ----------------------------------------------------------
        // UPDATE PASSWORD
        // ----------------------------------------------------------

        [Fact]
        public async Task UpdatePasswordAsync_Throws_WhenWrongCurrentPassword()
        {
            // ARRANGE
            var service = CreateService();

            var user = new User
            {
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpw")
            };

            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var dto = new UpdatePasswordDto
            {
                CurrentPassword = "wrongpw",
                NewPassword = "123456"
            };

            // ACT + ASSERT
            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                service.UpdatePasswordAsync(1, dto));

            Assert.Equal(4004, ex.Code);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ReturnsTrue_WhenValid()
        {
            // ARRANGE
            var service = CreateService();

            var user = new User
            {
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpws")
            };

            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var dto = new UpdatePasswordDto
            {
                CurrentPassword = "oldpws",
                NewPassword = "newpassword123"
            };

            // ACT
            var result = await service.UpdatePasswordAsync(1, dto);

            // ASSERT
            Assert.True(result);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ----------------------------------------------------------
        // GET USER BY ID
        // ----------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_ThrowsBusinessException_WhenNotFound()
        {
            // ARRANGE
            var service = CreateService();

            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

            // ACT + ASSERT
            await Assert.ThrowsAsync<BusinessException>(() => service.GetByIdAsync(1));
        }
    }
}
