using System.Text.RegularExpressions;
using Disco.Web.Exceptions.Discord;
using Disco.Web.Exceptions.User;
using Microsoft.EntityFrameworkCore;
namespace Disco.Web.Data;

public class DiscoContext : DbContext
{
    public DbSet<Account> accounts { get; set; } 
    public DbSet<AccountPassword> accountPasswords { get; set; }
    public DbSet<AccountTag> accountTags { get; set; }
    public DbSet<AccountDescription> accountDescriptions { get; set; }
    public DbSet<AccountDiscord> accountDiscords { get; set; }
    public DbSet<AccountMatrix> accountMatrix { get; set; }
    public DbSet<AccountDiscordCode> accountDiscordCodes { get; set; }
    public DbSet<AccountSession> accountSessions { get; set; }
    public DbSet<AccountRelationship> accountRelationships { get; set; }
    public DbSet<AccountAvatar> accountAvatars { get; set; }
    public DbSet<AccountReport> accountReports { get; set; }
    public DbSet<AccountBan> accountBans { get; set; }
    public DbSet<AccountDiscordBan> accountDiscordBans { get; set; }
    public DbSet<UserUploadedImage> images { get; set; }
    public DbSet<TopTag> topTags { get; set; }

    private string dbPath { get; set; }
    
    public DiscoContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        dbPath = System.IO.Path.Join(path, "disco.db");
    }
    
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AccountDescription>()
            .HasIndex(a => a.accountId)
            .IsUnique();
        
        b.Entity<AccountPassword>()
            .HasIndex(a => a.accountId)
            .IsUnique();

        b.Entity<AccountTag>()
            .HasKey(a => a.accountTagId);

        b.Entity<AccountSession>()
            .HasKey(a => a.accountSessionId);

        b.Entity<AccountTag>()
            .HasIndex("accountId", "tag")
            .IsUnique();
        b.Entity<AccountTag>()
            .HasIndex(a => a.accountId);
        b.Entity<AccountRelationship>().HasIndex(a => a.accountId);

        b.Entity<AccountAvatar>()
            .HasIndex(a => a.accountId)
            .IsUnique();

        b.Entity<AccountBan>()
            .HasIndex(a => a.bannedAccountId)
            .IsUnique();
        
        b.Entity<AccountDiscordBan>()
            .HasIndex(a => a.discordId)
            .IsUnique();

        b.Entity<UserUploadedImage>()
            .HasIndex(a => a.sha256Hash)
            .IsUnique();

        b.Entity<TopTag>()
            .HasIndex(a => a.tag)
            .IsUnique();
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={dbPath}");
}

public enum SocialMedia
{
    Discord = 1,
    Matrix,
}

public class Account
{
    public long accountId { get; set; }
    public string username { get; set; }
    public string displayName { get; set; }
    public DateTime createdAt { get; set; }
    public string? gender { get; set; }
    public int? age { get; set; }
    public string? pronouns { get; set; }
}

public enum AvatarSource
{
    Discord = 1,
    Matrix,
    UserUploadedImage,
}

public class AccountAvatar
{
    public long accountAvatarId { get; set; }
    public long accountId { get; set; }
    public string? url { get; set; }
    public long? userUploadedImageId { get; set; }
    public AvatarSource source { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class AccountPassword
{
    public long accountPasswordId { get; set; }
    public long accountId { get; set; }
    public string hash { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class AccountTag
{
    public long accountTagId { get; set; }
    public long accountId { get; set; }
    public string tag { get; set; }
    public string displayTag { get; set; }
    public DateTime createdAt { get; set; }

    public static string NormalizeTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new InvalidTagException();
        if (tag.Length > 64) 
            tag = tag.Substring(0, 64);
        
        tag = tag.ToLowerInvariant();
        var rgx = new Regex("[^a-z0-9]");
        tag = rgx.Replace(tag, "");
        return tag;
    }
}

public class AccountDescription
{
    public long accountDescriptionId { get; set; }
    public long accountId { get; set; }
    public string? description { get; set; }
    public DateTime updatedAt { get; set; }
}

public class AccountDiscord
{
    public long accountDiscordId { get; set; }
    public long accountId { get; set; }
    public long discordId { get; set; }
    public string name { get; set; }
    public int tag { get; set; }
    public string? avatarUrl { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }

    public static Tuple<string,int> Parse(string str)
    {
        if (!str.Contains("#"))
            throw new MissingTagException();
        var result = str.Split("#");
        if (result.Length != 2)
            throw new BadLengthAfterTagSplit();
        var id = int.Parse(result[1]);
        if (id < 0 || id > 9999)
            throw new IdOutOfRangeException();
        return new(result[0], id);
    }

    public string DisplayString
    {
        get
        {
            var str = name + "#";
            if (tag > 1000)
                str += tag;
            else if (tag > 100)
                str += "0" + tag;
            else if (tag > 10)
                str += "00" + tag;
            else
                str += "000" + tag;

            return str;
        }
    }
}

public class AccountMatrix
{
    public long accountMatrixId { get; set; }
    public long accountId { get; set; }
    public string name { get; set; }
    public string domain { get; set; }
    public string? avatarUrl { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }

    public string GetDisplayString()
    {
        return "@" + name + ":" + domain;
    }
}

public class AccountDiscordCode
{
    public long accountDiscordCodeId { get; set; }
    public long accountId { get; set; }
    public string code { get; set; }
    public string? redirectUrl { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class AccountSession
{
    public string accountSessionId { get; set; }
    public long accountId { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public enum RelationStatus
{
    Unknown = 0,
    /// <summary>
    /// User intentionally ignored this account.
    /// </summary>
    Ignored,
    /// <summary>
    /// User added this user to their favorites or whatever
    /// </summary>
    Accepted,
}

public class AccountRelationship
{
    public long accountRelationshipId { get; set; }
    public long accountId { get; set; }
    public long otherAccountId { get; set; }
    public RelationStatus relationship { get; set; }
}

public enum ReportState
{
    Created = 1,
    Resolved,
    Rejected,
}

public enum ReportField
{
    Username = 1,
    DisplayName,
    Avatar,
    Description,
    Tags,
    Pronouns,
    Gender,
    Other,
}

public enum ReportReason
{
    Slur = 1,
    Harassment,
    Url,
    Spam,
    Illegal,
    NotSafeForWorkUnder18,
    NotSafeForWorkOutsideCorrectTags,
    Scam,
}

public class AccountReport
{
    public long accountReportId { get; set; }
    public long accountId { get; set; }
    public long reportedAccountId { get; set; }
    public ReportState state { get; set; }
    public ReportField field { get; set; }
    public ReportReason reason { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class AccountBan
{
    public long accountBanId { get; set; }
    public long bannedAccountId { get; set; }
    public string reason { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class AccountDiscordBan
{
    public long accountDiscordBanId { get; set; }
    public long discordId { get; set; }
    public long? bannedAccountId { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public enum ImageStatus
{
    AwaitingApproval = 1,
    Approved,
    Rejected,
}

public enum ImageFormat
{
    FormatWebP = 1,
    FormatPng,
}

public class UserUploadedImage
{
    public static string baseUrl { get; set; }
    public long userUploadedImageId { get; set; }
    public long accountId { get; set; }
    public string sha256Hash { get; set; }
    public string originalSha256Hash { get; set; }
    public ImageStatus status { get; set; }
    public ImageFormat format { get; set; }
    public int sizeX { get; set; }
    public int sizeY { get; set; }
    public long fileSize { get; set; }

    public string extension
    {
        get
        {
            if (format is not ImageFormat.FormatWebP)
                throw new NotImplementedException();
            return "webp";
        }
    }
    public string url
    {
        get
        {
            return baseUrl + "/user-content/images/" + sha256Hash + "." + extension;
        }
    }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    
}

public class TopTag
{
    public long topTagId { get; set; }
    public string tag { get; set; }
    public string displayTag { get; set; }
}