using UserService.DTOs;
using UserService.Validators;

namespace UserService.Tests.Unit.Validators
{
    public class UpdatePasswordValidatorTests
    {
        private readonly UpdatePasswordValidator _validator = new();

        [Fact]
        public void Should_Fail_When_NewPassword_Is_Short()
        {
            // ARRANGE
            var dto = new UpdatePasswordDto
            {
                CurrentPassword = "123456",
                NewPassword = "123"
            };

            // ACT
            var result = _validator.Validate(dto);

            // ASSERT
            Assert.False(result.IsValid);
        }
    }
}
