using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.Controllers;
using UserService.DTOs;
using UserService.Middleware;
using UserService.Services;
using Xunit;

namespace UserService.Tests.Unit.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _serviceMock = new();
        private readonly Mock<IJwtService> _jwtMock = new();

        private UserController CreateController() =>
            new(_serviceMock.Object, _jwtMock.Object);

        [Fact]
        public async Task GetAll_ReturnsOk_WithUsers()
        {
            var users = new List<UserDto> { new() { IdUser = 1, Email = "a@b.com" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

            var controller = CreateController();

            var actionResult = await controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedUsers = Assert.IsType<List<UserDto>>(okResult.Value);

            Assert.Single(returnedUsers);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenUserExists()
        {
            var userDto = new UserDto { IdUser = 1, Email = "a@b.com" };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(userDto);

            var controller = CreateController();

            var actionResult = await controller.GetById(1);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);

            Assert.Equal(userDto, returnedUser);
        }

        [Fact]
        public async Task GetById_ThrowsBusinessException_WhenUserNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1))
                        .ThrowsAsync(new BusinessException(404, 4001, "Utilizador não encontrado"));

            var controller = CreateController();

            await Assert.ThrowsAsync<BusinessException>(() => controller.GetById(1));
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithUser()
        {
            var dto = new CreateUserDto { Email = "a@b.com", Username = "User" };
            var userDto = new UserDto { IdUser = 1, Email = dto.Email, Username = dto.Username };

            _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(userDto);

            var controller = CreateController();

            var actionResult = await controller.Create(dto);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedUser = Assert.IsType<UserDto>(createdResult.Value);

            Assert.Equal(userDto, returnedUser);
            Assert.Equal(nameof(controller.GetById), createdResult.ActionName);
        }

        [Fact]
        public async Task Create_ThrowsBusinessException_WhenEmailExists()
        {
            var dto = new CreateUserDto { Email = "a@b.com" };
            _serviceMock.Setup(s => s.CreateAsync(dto))
                        .ThrowsAsync(new BusinessException(409, 4002, "Email já registado"));

            var controller = CreateController();

            await Assert.ThrowsAsync<BusinessException>(() => controller.Create(dto));
        }

        [Fact]
        public async Task Login_ReturnsOk_WithAuthResult_WhenValid()
        {
            // Arrange
            var dto = new LoginDto { Email = "a@b.com", Password = "123" };
            var userDto = new UserDto { IdUser = 1, Email = dto.Email, Username = "a" };
            var authResult = new AuthDto
            {
                User = userDto,
                Token = "fake-jwt-token"
            };

            _serviceMock.Setup(s => s.LoginAsync(dto.Email, dto.Password))
                        .ReturnsAsync(authResult);

            var controller = CreateController();

            // Act
            var actionResult = await controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedAuthResult = Assert.IsType<AuthDto>(okResult.Value);

            Assert.Equal(authResult.Token, returnedAuthResult.Token);
            Assert.Equal(authResult.User.Email, returnedAuthResult.User.Email);
            Assert.Equal(authResult.User.Username, returnedAuthResult.User.Username);
        }

        [Fact]
        public async Task Login_ThrowsBusinessException_WhenInvalid()
        {
            var dto = new LoginDto { Email = "a@b.com", Password = "wrong" };
            _serviceMock.Setup(s => s.LoginAsync(dto.Email, dto.Password))
                        .ThrowsAsync(new BusinessException(401, 4011, "Login inválido"));

            var controller = CreateController();

            await Assert.ThrowsAsync<BusinessException>(() => controller.Login(dto));
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var controller = CreateController();

            var result = await controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ThrowsBusinessException_WhenNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1))
                        .ThrowsAsync(new BusinessException(404, 4001, "Utilizador não encontrado"));

            var controller = CreateController();

            await Assert.ThrowsAsync<BusinessException>(() => controller.Delete(1));
        }
    }
}
