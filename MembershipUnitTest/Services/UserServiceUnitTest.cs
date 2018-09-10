using System.Threading.Tasks;
using Membership.Models;
using Membership.Repositories;
using Membership.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MembershipUnitTest.Services
{
    public class UserServiceUnitTest
    {
        private const string Username = "mluoau@gamil.com";
        private readonly UserInfoEntity _userInfo = new UserInfoEntity()
        {
            Username = Username
        };
        [Fact]
        public async void GetUserByUsernameAsync_ShouldWork()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var mockIdSettings = new Mock<IOptions<IdentitySettings>>();
            mockRepo.Setup(m => m.GetUserByUsernameAsync(Username)).Returns(Task.FromResult(_userInfo));

            // Act
            var service = new UserService(mockRepo.Object, mockIdSettings.Object);
            var result = await service.GetUserByUsernameAsync(Username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_userInfo.Username, result.Username);
        }
    }
}
