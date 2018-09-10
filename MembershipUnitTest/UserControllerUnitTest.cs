using System.Threading.Tasks;
using Membership.Controllers;
using Membership.Models;
using Membership.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MembershipUnitTest
{
    public class UserControllerUnitTest
    {
        private const string TokenString = "";

        private readonly UserInfoDto _userInfoDto = new UserInfoDto()
        {
            Username = "mluoau@gmail.com",
            Password = "carsales"
        };

        [Fact]
        public async void Authenticate_ShouldReturnToken()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.CheckUserExsits(It.IsAny<string>())).Returns(Task.FromResult(true));
            mockService.Setup(s => s.ValidateUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            mockService.Setup(s => s.GenerateToken(It.IsAny<string>())).Returns(TokenString);

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.Authenticate(_userInfoDto);
            var okResult = result as OkObjectResult;
            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(TokenString, okResult.Value);
        }
    }
}
