using Disco.Web.Services;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Logging;

namespace Disco.UnitTest;

public class UnitTestUserService
{
    [Fact]
    public async Task TestPasswordHash()
    {
        var log = new Moq.Mock<Microsoft.Extensions.Logging.ILogger>();
        var cacheHelper = new Moq.Mock<ICacheHelperService>();
        var user = new UserService(log.Object, cacheHelper.Object);
        var hash = await user.HashPassword("password");
        Assert.True(await user.IsPasswordValid(hash, "password"));
    }
}