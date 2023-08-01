using Disco.Web.Services;
using Isopoh.Cryptography.Argon2;

namespace Disco.UnitTest;

public class UnitTestUserService
{
    [Fact]
    public async Task TestPasswordHash()
    {
        var user = new UserService();
        var hash =await user.HashPassword("password");
        Assert.True(await user.IsValid(hash, "password"));
    }
}