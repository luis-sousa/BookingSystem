using UserService.DTOs;
using UserService.Validators;


namespace UserService.Tests.Unit.Validators
{
    public class CreateUserValidatorTests
    {
        private readonly CreateUserValidator _validator = new();

        [Fact]
        public void Should_Fail_When_Username_Is_Empty()
        {
            // ARRANGE
            var dto = new CreateUserDto
            {
                Username = "",
                Email = "test@test.com",
                Password = "123456"
            };

            // ACT
            var result = _validator.Validate(dto);

            // ASSERT
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Valid()
        {
            // ARRANGE
            var dto = new CreateUserDto
            {
                Username = "luis",
                Email = "test@test.com",
                Password = "123456"
            };

            // ACT
            var result = _validator.Validate(dto);

            // ASSERT
            Assert.True(result.IsValid);
        }
    }
}
