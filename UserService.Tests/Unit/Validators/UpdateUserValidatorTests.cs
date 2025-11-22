using UserService.DTOs;
using UserService.Validators;

namespace UserService.Tests.Unit.Validators
{
    public class UpdateUserValidatorTests
    {
        private readonly UpdateUserValidator _validator = new();

        [Fact]
        public void Should_Fail_When_Email_Invalid()
        {
            // ARRANGE
            var dto = new UpdateUserDto
            {
                Username = "Luis",
                Email = "INVALID_EMAIL"
            };

            // ACT
            var result = _validator.Validate(dto);

            // ASSERT
            Assert.False(result.IsValid);
        }
    }
}
