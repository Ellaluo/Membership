using System.Threading.Tasks;
using Membership.Controllers;
using Membership.Models;
using Membership.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MembershipUnitTest.Controllers
{
    public class UserControllerUnitTest : IClassFixture<ClaimsPrincipalFixture>
    {
        private static ClaimsPrincipalFixture _cpFixture;

        public UserControllerUnitTest(ClaimsPrincipalFixture fixture)
        {
            _cpFixture = fixture;
        }
        private static UserController MockControllerUser(IMock<IUserService> mockService, string Role = "administrator")
        {
            var user = _cpFixture.User;
            var controller = new UserController(mockService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = user }
                }
            };
            return controller;
        }

        private const string TokenString = "";
        private readonly UserInfoDto _userInfoDto = new UserInfoDto()
        {
            Username = "mluoau@gmail.com",
            Password = "carsales",
            Client = "A"
        };

        private readonly UserInfoDto _invalidUserInfoDto = new UserInfoDto()
        {
            Username = "mluoau@gmail.com",
            Password = ""
        };

        private readonly UserInfoDto _invalidClientUserInfoDto = new UserInfoDto()
        {
            Username = "mluoau@gmail.com",
            Password = "carsales",
            Client = "D"
        };

        private readonly UserInfoDto _nullExistingUserInfoDto = null;

        [Fact]
        public async void Authenticate_ShouldReturnOk()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_userInfoDto));
            mockService.Setup(s => s.ValidateUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            mockService.Setup(s => s.GenerateToken(It.IsAny<string>(), It.IsAny<string>())).Returns(TokenString);

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.Authenticate(_userInfoDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(TokenString, okResult.Value);
        }

        [Fact]
        public async void Authenticate_ShouldReturnBadRequestWhenInvalidUserPassword()
        {
            // Arrange
            var mockService = new Mock<IUserService>();

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.Authenticate(_invalidUserInfoDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid username or password", badRequestResult.Value);
        }

        [Fact]
        public async void Authenticate_ShouldReturnBadRequestWhenInvalidClient()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_userInfoDto));
            mockService.Setup(s => s.ValidateUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.Authenticate(_invalidClientUserInfoDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Invalid client", badRequestResult.Value);
        }

        [Fact]
        public async void Authenticate_ShouldReturnUnauthorizedWhenUserNotExsits()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_nullExistingUserInfoDto));

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.Authenticate(_userInfoDto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async void Authenticate_ShouldReturnUnauthorizedWhenInvalidUsernamePassword()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.ValidateUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.Authenticate(_userInfoDto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async void CreateUser_ShouldReturnOk()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_nullExistingUserInfoDto));
            mockService.Setup(s => s.CreateUserAsync(It.IsAny<UserInfoDto>())).Returns(Task.FromResult(_userInfoDto));

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.CreateUser(_userInfoDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(_userInfoDto, okResult.Value);
        }

        [Fact]
        public async void CreateUser_ShouldReturnConflict()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_userInfoDto));

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.CreateUser(_userInfoDto);

            // Assert
            Assert.IsType<ConflictResult>(result);
            var conflictResult = result as ConflictResult;
            Assert.NotNull(conflictResult);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async void CreateUser_ShouldReturnBadRequest()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_nullExistingUserInfoDto));
            mockService.Setup(s => s.CreateUserAsync(It.IsAny<UserInfoDto>())).Returns(Task.FromResult(_nullExistingUserInfoDto));

            // Act
            var userController = new UserController(mockService.Object);
            var result = await userController.CreateUser(_userInfoDto);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            var badRequestResult = result as BadRequestResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async void UpdaterUserPassword_ShouldReturnAccepted()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_userInfoDto));
            mockService.Setup(s => s.UpdateUserPasswordAsync(It.IsAny<UserInfoDto>())).Returns(Task.FromResult(1));

            // Act
            var userController = MockControllerUser(mockService);
            var result = await userController.UpdateUserPassword(_userInfoDto);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            var acceptedResult = result as AcceptedResult;
            Assert.NotNull(acceptedResult);
            Assert.Equal(202, acceptedResult.StatusCode);
        }

        [Fact]
        public async void Activate_ShouldReturnOK()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_userInfoDto));
            mockService.Setup(s => s.ActivateAsync(It.IsAny<UserInfoDto>(), It.IsAny<string>())).Returns(Task.FromResult(1));

            // Act
            var userController = MockControllerUser(mockService);
            var result = await userController.Activate(_userInfoDto);

            // Assert
            Assert.IsType<OkResult>(result);
            var okResult = result as OkResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async void Activate_ShouldReturnBadRequest()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_userInfoDto));
            mockService.Setup(s => s.ActivateAsync(It.IsAny<UserInfoDto>(), It.IsAny<string>())).Returns(Task.FromResult(0));

            // Act
            var userController = MockControllerUser(mockService);
            var result = await userController.Activate(_userInfoDto);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            var badRequestResultResult = result as BadRequestResult;
            Assert.NotNull(badRequestResultResult);
            Assert.Equal(400, badRequestResultResult.StatusCode);
        }

        [Fact]
        public async void Activate_ShouldReturnNotFound()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_nullExistingUserInfoDto));
            mockService.Setup(s => s.ActivateAsync(It.IsAny<UserInfoDto>(), It.IsAny<string>())).Returns(Task.FromResult(0));

            // Act
            var userController = MockControllerUser(mockService);
            var result = await userController.Activate(_userInfoDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async void Deactivate_ShouldReturnOK()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_userInfoDto));
            mockService.Setup(s => s.DeActivateAsync(It.IsAny<UserInfoDto>(), It.IsAny<string>())).Returns(Task.FromResult(1));

            // Act
            var userController = MockControllerUser(mockService);
            var result = await userController.DeActivate(_userInfoDto);

            // Assert
            Assert.IsType<OkResult>(result);
            var okResult = result as OkResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async void Deactivate_ShouldReturnBadRequest()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_userInfoDto));
            mockService.Setup(s => s.DeActivateAsync(It.IsAny<UserInfoDto>(), It.IsAny<string>())).Returns(Task.FromResult(0));

            // Act
            var userController = MockControllerUser(mockService);
            var result = await userController.DeActivate(_userInfoDto);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            var badRequestResultResult = result as BadRequestResult;
            Assert.NotNull(badRequestResultResult);
            Assert.Equal(400, badRequestResultResult.StatusCode);
        }

        [Fact]
        public async void Deactivate_ShouldReturnNotFound()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>())).Returns(Task.FromResult(_nullExistingUserInfoDto));
            mockService.Setup(s => s.DeActivateAsync(It.IsAny<UserInfoDto>(), It.IsAny<string>())).Returns(Task.FromResult(0));

            // Act
            var userController = MockControllerUser(mockService);
            var result = await userController.DeActivate(_userInfoDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            var notFoundResult = result as NotFoundResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}
