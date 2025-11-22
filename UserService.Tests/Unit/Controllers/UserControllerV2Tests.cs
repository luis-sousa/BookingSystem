using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.Controllers;
using UserService.DTOs;
using UserService.Middleware;
using UserService.Services;
using Xunit;

namespace UserService.Tests.Unit.Controllers
{
    public class UserController_UpdatePassword_Tests
    {
        private readonly Mock<IUserService> _serviceMock = new();

        private UserController2 CreateController() =>
            new(_serviceMock.Object);

        // -------------------------------------------------------------
        // 🟩 1 — SUCESSO → Deve retornar 204 NoContent
        // -------------------------------------------------------------
        [Fact]
        public async Task UpdatePassword_Returns204_WhenSuccessful()
        {
            // ARRANGE
            var dto = new UpdatePasswordDto
            {
                CurrentPassword = "123456",
                NewPassword = "NovaSenha!"
            };

            _serviceMock.Setup(s => s.UpdatePasswordAsync(1, dto))
                        .ReturnsAsync(true);

            var controller = CreateController();

            // ACT
            var result = await controller.UpdatePassword(1, dto);

            // ASSERT
            Assert.IsType<NoContentResult>(result);
        }

        // -------------------------------------------------------------
        // 🟥 2 — ERRO: Password atual incorreta → BusinessException(4004)
        // -------------------------------------------------------------
        [Fact]
        public async Task UpdatePassword_ThrowsBusinessException_WhenCurrentPasswordInvalid()
        {
            // ARRANGE
            var dto = new UpdatePasswordDto
            {
                CurrentPassword = "errada",
                NewPassword = "NovaSenha!"
            };

            _serviceMock.Setup(s => s.UpdatePasswordAsync(1, dto))
                        .ThrowsAsync(new BusinessException(400, 4004, "Password atual incorreta"));

            var controller = CreateController();

            // ACT + ASSERT
            await Assert.ThrowsAsync<BusinessException>(
                () => controller.UpdatePassword(1, dto)
            );
        }

        // -------------------------------------------------------------
        // 🟥 3 — ERRO: User não encontrado → BusinessException(4001)
        // -------------------------------------------------------------
        [Fact]
        public async Task UpdatePassword_ThrowsBusinessException_WhenUserNotFound()
        {
            // ARRANGE
            var dto = new UpdatePasswordDto
            {
                CurrentPassword = "123456",
                NewPassword = "NovaSenha!"
            };

            _serviceMock.Setup(s => s.UpdatePasswordAsync(1, dto))
                        .ThrowsAsync(new BusinessException(404, 4001, "Utilizador não encontrado"));

            var controller = CreateController();

            // ACT + ASSERT
            await Assert.ThrowsAsync<BusinessException>(
                () => controller.UpdatePassword(1, dto)
            );
        }
    }
}
