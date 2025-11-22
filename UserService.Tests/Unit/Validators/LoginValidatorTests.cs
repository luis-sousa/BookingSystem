using UserService.DTOs;
using UserService.Validators;

namespace UserService.Tests.Unit.Validators
{
    public class LoginValidatorTests
    {
        private readonly LoginValidator _validator = new();

        [Fact]
        public void Should_Fail_When_Email_Invalid()
        {
            // ARRANGE
            var dto = new LoginDto
            {
                Email = "invalid",
                Password = "123456"
            };

            // ACT
            var result = _validator.Validate(dto);

            // ASSERT
            Assert.False(result.IsValid);
        }
    }
}
