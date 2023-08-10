using Disco.Web.Controllers;
using Disco.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Disco.UnitTest;

public class UnitTestBotController
{
    [Fact]
    public async Task TestIsAuthorized()
    {
        var key = "Key 1234";
        var log = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<BotController>>();
        var userService = new Moq.Mock<IUserService>();
        var botService = new Moq.Mock<IBotService>();
        var httpRequest = new Moq.Mock<IHttpRequestService>();
        botService.Setup(c => c.GetAuthorizationKey()).Returns(key);
        httpRequest.Setup(c => c.GetRequestHeader(BotController.BotAuthorizationHeaderName)).Returns(key);
        var controller = new BotController(log.Object, userService.Object, botService.Object, httpRequest.Object);
        var ok = controller.IsAuthorized();
        Assert.True(ok);
    }
    
    [Theory]
    [InlineData(null)] // not present
    [InlineData("Hello")] // wrong
    [InlineData("key 1234")] // close but wrong case
    [InlineData("Key 123")] // missing last char
    [InlineData("Key 12345")] // extra char
    [InlineData("ey 1234")] // missing first char
    public async Task TestIsAuthorizedFail(string? value)
    {
        var realKey = "Key 1234";
        var log = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<BotController>>();
        var userService = new Moq.Mock<IUserService>();
        var botService = new Moq.Mock<IBotService>();
        var httpRequest = new Moq.Mock<IHttpRequestService>();
        botService.Setup(c => c.GetAuthorizationKey()).Returns(value);
        httpRequest.Setup(c => c.GetRequestHeader(BotController.BotAuthorizationHeaderName)).Returns(realKey);
        var controller = new BotController(log.Object, userService.Object, botService.Object, httpRequest.Object);
        var ok = controller.IsAuthorized();
        Assert.False(ok);
    }
}