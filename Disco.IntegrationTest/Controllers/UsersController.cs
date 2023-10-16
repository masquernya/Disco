using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disco.Web.Controllers;
using Disco.Web.Data;
using Disco.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;
// Assert.NotNull is being ignored by this rule :(
#pragma warning disable CS8602

namespace Disco.IntegrationTest;

public class FakeDisposable : IDisposable
{
    public void Dispose()
    {
        
    }
}

public class LoggerTest<T> : ILogger<T>
{
    private readonly ITestOutputHelper _testOutputHelper;
    public LoggerTest(ITestOutputHelper helper)
    {
        this._testOutputHelper = helper;
    }


    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _testOutputHelper.WriteLine("[" + logLevel + "] " + formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        // don't care
        return new FakeDisposable();
    }
}

public class UserServiceTest : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private string dbPath { get; }
    public UserServiceTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        // Setup DB
        this.dbPath = Path.GetTempFileName();
        DiscoContext.dbPath = dbPath;
        var db = new DiscoContext();
        db.Database.Migrate();

        UserService.Configure(Path.GetTempPath() + "/images/");
        this.service = new UserService(new LoggerTest<UserService>(_testOutputHelper), new CacheHelperService(new MemoryCacheService(), new LoggerTest<CacheHelperService>(_testOutputHelper)));
    }

    public void Dispose()
    {
        _testOutputHelper.WriteLine("Disposing {0}", DiscoContext.dbPath);
        File.Delete(DiscoContext.dbPath);
        Directory.Delete(Path.GetTempPath() + "/images/", true);
    }
    
    private IUserService service { get; set; }

    [Fact]
    public async Task CreateUser()
    {
        // Create the account.
        var createAccountResult = await service.CreateAccount("Username123", "Password123");
        Assert.NotNull(createAccountResult);
        Assert.Equal(1, createAccountResult.accountId);
        Assert.Equal("Username123", createAccountResult.username);
        // Confirm password was inserted properly.
        var passwordOk = await service.IsPasswordValid(createAccountResult.accountId
            , "Password123");
        Assert.True(passwordOk);
        // Confirm invalid password doesn't work.
        var passwordNotOk = await service.IsPasswordValid(createAccountResult.accountId
            , "Password1234");
        Assert.False(passwordNotOk);
        
        // Confirm GetAccountByName works.
        var account = await service.GetAccountByName("Username123");
        Assert.NotNull(account);
        Assert.Equal(1, account.accountId);
        
        var doesntExist = await service.GetAccountByName("Username123DoesntExist");
        Assert.Null(doesntExist);
        
        // Try to make a session.
        var session = await service.CreateSession(createAccountResult.accountId);
        Assert.NotNull(session);
        Assert.Equal(1, session.accountId);
        Assert.NotNull(session.accountSessionId);
        Assert.NotEmpty(session.accountSessionId);
        // actual len should be about 128 * 1.33
        Assert.True(session.accountSessionId.Length is > 128 and < (128 * 2));
        
        
        // Confirm session exists
        var sessionExists = await service.GetSessionAndUpdate(session.accountSessionId);
        Assert.NotNull(sessionExists);
        Assert.Equal(1, sessionExists.accountId);
        
        // Confirm GetAccountById is returning correct info.
        var accountById = await service.GetAccountById(1);
        Assert.NotNull(accountById);
        Assert.Equal(1, accountById.accountId);
        Assert.Equal("Username123", accountById.username);
        
        // Basic stuff
        await TestDescription(account.accountId);
        await TestDisplayName(account.accountId);
        await TestGender(account.accountId);
        await TestPronouns(account.accountId);
        await TestAge(account.accountId);
        
        // Tags
        var myTags = new List<string>() { "Cars", "Computers", "Video Games", "Minecraft", "Gaming" };
        foreach (var tag in myTags)
        {
            await service.AddTag(account.accountId, tag);
        }
        
        // Confirm tags were added
        var tags = (await service.GetTags(account.accountId)).ToList();
        Assert.NotNull(tags);
        Assert.Equal(myTags.Count, tags.Count);
        foreach (var tag in myTags)
        {
            Assert.Contains(tags, a => a.displayTag == tag);
        }
        
        // Delete "Gaming"
        await service.DeleteTag(account.accountId, tags.First(a => a.displayTag == "Gaming").accountTagId);
        
        // Make sure it was removed.
        tags = (await service.GetTags(account.accountId)).ToList();
        Assert.NotNull(tags);
        Assert.Equal(myTags.Count - 1, tags.Count);
        foreach (var tag in myTags)
        {
            if (tag == "Gaming")
            {
                Assert.DoesNotContain(tags, a => a.displayTag == tag);
            }
            else
            {
                Assert.Contains(tags, a => a.displayTag == tag);
            }
        }
        
        // Test verify matrix
        await service.SetMatrixAccount(account.accountId, "@discofriendsbot:matrix.org");
        var matrix = await service.GetMatrixForAccount(account.accountId);
        Assert.NotNull(matrix);
        Assert.Equal("discofriendsbot", matrix.name);
        Assert.Equal("matrix.org", matrix.domain);

        var pendingImages = await service.GetImagesAwaitingReview();
        foreach (var image in pendingImages)
            await service.SetImageStatus(image.image.userUploadedImageId, ImageStatus.Approved);
        
        // get pfp
        var pfp = await service.GetAvatarForAccount(account.accountId);
        Assert.NotNull(pfp);
        Assert.NotNull(pfp.url);
        Assert.Equal(AvatarSource.UserUploadedImage, pfp.source);
        
    }

    public async Task TestDescription(long accountId)
    {
        var myDesc = "Description integration test "+ Guid.NewGuid().ToString();
        await service.SetDescription(accountId, myDesc);
        // Make sure it matches
        var desc = await service.GetDescription(accountId);
        Assert.NotNull(desc);
        Assert.Equal(myDesc, desc.description);
    }

    public async Task TestDisplayName(long accountId)
    {
        var dn = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
        await service.SetDisplayName(accountId, dn);
        // Make sure it matches
        var newDn = await service.GetAccountById(accountId);
        Assert.NotNull(newDn);
        Assert.Equal(dn, newDn.displayName);
    }

    public async Task TestGender(long accountId)
    {
        var gender = "Male";
        await service.SetGender(accountId, gender);
        // Make sure it matches
        var newGender = await service.GetAccountById(accountId);
        Assert.NotNull(newGender);
        Assert.Equal(gender, newGender.gender);
    }

    public async Task TestPronouns(long accountId)
    {
        var pronouns = "He/Him";
        await service.SetPronouns(accountId, pronouns);
        var newPronouns = await service.GetAccountById(accountId);
        Assert.Equal(pronouns, newPronouns.pronouns);
    }

    public async Task TestAge(long accountId)
    {
        var age = 50;
        await service.SetAge(accountId, age);
        var newAge = await service.GetAccountById(accountId);
        Assert.Equal(age, newAge.age);
    }

    [Fact]
    public async Task TestGetMatrixProfilePictureUrl()
    {
        // var resultOne = await (service as UserService).GetMatrixProfilePictureUrl("MyUsername")
    }
}