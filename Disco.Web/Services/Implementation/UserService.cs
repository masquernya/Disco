using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Disco.Web.Data;
using Disco.Web.Exceptions.Matrix;
using Disco.Web.Exceptions.User;
using Disco.Web.Models.User;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Webp;

namespace Disco.Web.Services;

public class UserService : IUserService
{
    private ILogger logger { get; set; }
    public UserService(ILogger logger)
    {
        this.logger = logger;
    }
    
    private static string uploadImagePath { get; set; }

    public static void Configure(string pathToImages)
    {
        uploadImagePath = pathToImages;
        // init
        var folderExists = Directory.Exists(uploadImagePath);
        if (!folderExists)
            Directory.CreateDirectory(uploadImagePath);
    }

    public async Task<string> HashPassword(string password)
    {
        var hash = Argon2.Hash(password);
        return hash;
    }

    public async Task<bool> IsPasswordValid(string hash, string provided)
    {
        return Argon2.Verify(hash, provided);
    }
    
    public async Task<Account> CreateAccount(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidUsernameException();
        var rgx = new Regex("[^a-zA-Z0-9]");
        username = rgx.Replace(username, "");
        
        await using var ctx = new DiscoContext();
        
        // check if taken
        var name = await ctx.accounts.FirstOrDefaultAsync(a => a.username == username);
        if (name != null)
            throw new UsernameTakenException();
        
        var account = await ctx.accounts.AddAsync(new Account()
        {
            username = username,
            createdAt = DateTime.UtcNow,
            displayName = username,
        });
        await ctx.SaveChangesAsync();
        await ctx.accountPasswords.AddAsync(new AccountPassword()
        {
            hash = await HashPassword(password),
            accountId = account.Entity.accountId,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();
        return account.Entity;
    }

    public async Task<Account?> GetAccountByName(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Invalid username");
        var ctx = new DiscoContext();
        return await ctx.accounts.FirstOrDefaultAsync(a => a.username == username);
    }

    public async Task<bool> IsPasswordValid(long accountId, string value)
    {
        var ctx = new DiscoContext();
        var result = await ctx.accountPasswords.FirstOrDefaultAsync(a => a.accountId == accountId);
        if (result != null)
        {
            var ok = await IsPasswordValid(result.hash, value);
            return ok;
        }

        return false;
    }

    public async Task<AccountSession> CreateSession(long accountId)
    {
        // RNG instead of UUID: https://security.stackexchange.com/a/252551
        var sessionId = Convert.ToBase64String(RandomNumberGenerator.GetBytes(128)).ToUpper();
        var ctx = new DiscoContext();
        var result = await ctx.accountSessions.AddAsync(new AccountSession()
        {
            accountId = accountId,
            accountSessionId = sessionId,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<AccountSession?> GetSessionAndUpdate(string id)
    {
        var ctx = new DiscoContext();
        var item = await ctx.accountSessions.FirstOrDefaultAsync(a => a.accountSessionId == id);
        if (item != null && item.updatedAt > DateTime.UtcNow.Subtract(TimeSpan.FromDays(14)))
        {
            var accountBan = await ctx.accountBans.FirstOrDefaultAsync(a => a.bannedAccountId == item.accountId);
            if (accountBan != null)
                return null;
            item.updatedAt = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            return item;
        }
        
        return null;
    }

    public async Task<Account?> GetAccountById(long accountId)
    {
        var ctx = new DiscoContext();
        return await ctx.accounts.FindAsync(accountId);
    }

    public async Task<AccountDescription?> GetDescription(long accountId)
    {
        var ctx = new DiscoContext();
        return await ctx.accountDescriptions.FirstOrDefaultAsync(a => a.accountId == accountId);
    }

    public async Task<AccountTag> AddTag(long accountId, string tag)
    {
        var ctx = new DiscoContext();
        var existingTags = await ctx.accountTags.CountAsync(a => a.accountId == accountId);
        if (existingTags >= 100)
            throw new TooManyTagsException();
        
        var ent = await ctx.accountTags.AddAsync(new AccountTag()
        {
            accountId = accountId,
            tag = AccountTag.NormalizeTag(tag),
            displayTag = tag,
            createdAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();
        return ent.Entity;
    }

    public async Task DeleteTag(long accountId, long tagId)
    {
        var ctx = new DiscoContext();
        var toDelete = await ctx.accountTags.FindAsync(tagId);
        if (toDelete == null || toDelete.accountId != accountId)
            throw new TagNotFoundException();
        ctx.accountTags.Remove(toDelete);
        await ctx.SaveChangesAsync();
    }

    public async Task SetDescription(long accountId, string? description)
    {
        // normalize
        if (string.IsNullOrWhiteSpace(description))
            description = null;
        
        if (description?.Length > 4096)
            description = description.Substring(0, 4096);
        
        var ctx = new DiscoContext();
        var acc = await ctx.accountDescriptions.FirstOrDefaultAsync(a => a.accountId == accountId);
        if (acc == null)
        {
            await ctx.accountDescriptions.AddAsync(new AccountDescription()
            {
                accountId = accountId,
                description = description,
                updatedAt = DateTime.UtcNow,
            });
            await ctx.SaveChangesAsync();
        }
        else
        {
            ctx.accountDescriptions.Update(acc);
            acc.description = description;
            acc.updatedAt = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }
    }

    public async Task SetDisplayName(long accountId, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new InvalidDisplayNameException();
        
        if (displayName.Length > 64)
            displayName = displayName.Substring(0, 64);
        var rgx = new Regex("[^a-zA-Z0-9\\s]");
        displayName = rgx.Replace(displayName, "");

        if (displayName.Length > 64 || displayName.Length < 3)
            throw new InvalidDisplayNameException();
        
        var ctx = new DiscoContext();
        var acc = await ctx.accounts.FindAsync(accountId);
        if (acc == null)
            throw new UserNotFoundException();
        acc.displayName = displayName;
        await ctx.SaveChangesAsync();
    }

    public async Task<FetchUsersResponse> FetchUsersByStatus(long contextUserId, RelationStatus status, long startAccountId)
    {
        var users = new List<FetchUser>();
        var ctx = new DiscoContext();
        
        var hasDiscord = await ctx.accountDiscords.AnyAsync(a => a.accountId == contextUserId);
        var hasMatrix = await ctx.accountMatrix.AnyAsync(a => a.accountId == contextUserId);
        
        var me = await ctx.accounts.FindAsync(contextUserId);
        var amIOver18 = me.age == null || me.age >= 18;
        var myTags = await ctx.accountTags.Where(a => a.accountId == contextUserId).ToListAsync();
        
        var usersToInclude = await ctx.accountRelationships
            .Where(a => a.accountId == contextUserId && a.relationship == status).ToListAsync();
        var usersToIncludeSecondary = await ctx.accountRelationships
            .Where(a => a.otherAccountId == contextUserId && a.relationship == status).ToListAsync();
        
        
        foreach (var user in usersToInclude.OrderBy(a => a.accountId))
        {
            var id = user.otherAccountId;
            if (!usersToIncludeSecondary.Any(a => a.accountId == id))
                continue;
            
            if (id < startAccountId)
                continue;
            
            var info = await ctx.accounts.FindAsync(id);
            var isOver18 = info.age == null || info.age >= 18;
            if (isOver18 != amIOver18)
                continue;
                
            var desc = await ctx.accountDescriptions.FirstOrDefaultAsync(a
                => a.accountId == id);
            var disc = await ctx.accountDiscords.FirstOrDefaultAsync(a => a.accountId == id);
            var avatar = await ctx.accountAvatars.FirstOrDefaultAsync(a => a.accountId == id);
            var userTags = await ctx.accountTags.Where(a => a.accountId == id).ToListAsync();
            var socialMedia = new List<UserSocialMedia>();
            if (disc != null && hasDiscord)
                socialMedia.Add(new UserSocialMedia()
                {
                    type = SocialMedia.Discord,
                    displayString = disc.DisplayString,
                });
            var matrix = await ctx.accountMatrix.FirstOrDefaultAsync(a => a.accountId == id);
            if (matrix != null && hasMatrix)
                socialMedia.Add(new UserSocialMedia()
                {
                    type = SocialMedia.Matrix,
                    displayString = matrix.GetDisplayString(),
                });

            var item = new FetchUser()
            {
                accountId = id,
                username = info.username,
                displayName = info.displayName,
                description = desc?.description,
                age = info.age,
                pronouns = info.pronouns,
                gender = info.gender,
                tags = userTags.Select(c => new FetchUserTag()
                {
                    displayTag = c.displayTag,
                    isMatch = myTags.Any(a => a.tag == c.tag),
                }).ToList(),
                socialMedia = socialMedia,
            };
            await SetAvatar(ctx, item, avatar);
            users.Add(item);
            if (users.Count >= 100)
                break;
        }

        return new()
        {
            data = users,
        };
    }

    private async Task SetAvatar(DiscoContext ctx, FetchUser item, AccountAvatar? avatar)
    {
        if (avatar != null)
        {
            if (avatar.source == AvatarSource.UserUploadedImage && avatar.userUploadedImageId != null)
            {
                var imageData = await ctx.images.FirstOrDefaultAsync(a => a.userUploadedImageId == avatar.userUploadedImageId);
                if (imageData != null && imageData.status == ImageStatus.Approved)
                {
                    item.avatar = new()
                    {
                        source = avatar.source,
                        imageUrl = imageData.url,
                    };
                }
            }
            else if (avatar.url != null)
            {
                item.avatar = new()
                {
                    source = avatar.source,
                    imageUrl = avatar.url,
                };   
            }
        }
    }

    public async Task<FetchUsersResponse> FetchUsers(long contextUserId)
    {
        var ctx = new DiscoContext();
        var me = await ctx.accounts.FindAsync(contextUserId);
        var amIOver18 = me.age == null || me.age >= 18;
        var myTags = await ctx.accountTags.Where(a => a.accountId == contextUserId).ToListAsync();
        // User must have at least one tag.
        if (!myTags.Any())
            return new() { data = ArraySegment<FetchUser>.Empty };
        
        // User must have a social account attached.
        var hasDisc = await ctx.accountDiscords.AnyAsync(a => a.accountId == contextUserId);
        var hasMatrix = await ctx.accountMatrix.AnyAsync(a => a.accountId == contextUserId);
        if (!hasDisc && !hasMatrix)
            return new() { data = ArraySegment<FetchUser>.Empty };
        
        var usersToExclude = await ctx.accountRelationships
            .Where(a => a.accountId == contextUserId && a.relationship != RelationStatus.Unknown).ToListAsync();
        var users = new List<FetchUser>();

        var allUsers = await ctx.accountTags.Where(a => a.accountId != contextUserId).GroupBy(a => a.accountId).ToListAsync();
        allUsers.Sort((a,b) => a.Key > b.Key ? -1 : 1);
        foreach (var user in allUsers)
        {
            var id = user.Key;
            if (usersToExclude.Any(a => a.otherAccountId == id))
                continue;

            var inCommon = user.Count(tag => myTags.Any(a => a.tag == tag.tag));

            if (inCommon != 0)
            {
                var info = await ctx.accounts.FirstOrDefaultAsync(a => a.accountId == id);
                if (info == null)
                    continue;
                
                var isOver18 = info.age == null || info.age >= 18;
                if (isOver18 != amIOver18)
                    continue;
                
                var disc = await ctx.accountDiscords.FirstOrDefaultAsync(a => a.accountId == id);
                var matrix = await ctx.accountMatrix.AnyAsync(a => a.accountId == id);
                
                var mutualSocialSite = (hasMatrix && matrix) || (hasDisc && disc != null);
                if (!mutualSocialSite)
                    continue;

                var socialMedia = new List<UserSocialMedia>();
                if (disc != null)
                    socialMedia.Add(new UserSocialMedia()
                    {
                        type = SocialMedia.Discord,
                    });
                
                if (matrix)
                    socialMedia.Add(new UserSocialMedia()
                    {
                        type = SocialMedia.Matrix,
                    });

                var desc = await ctx.accountDescriptions.FirstOrDefaultAsync(a
                    => a.accountId == id);
                var avatar = await ctx.accountAvatars.FirstOrDefaultAsync(a => a.accountId == id);

                var item = new FetchUser()
                {
                    socialMedia = socialMedia,
                    accountId = id,
                    username = info.username,
                    displayName = info.displayName,
                    description = desc?.description,
                    age = info.age,
                    pronouns = info.pronouns,
                    gender = info.gender,
                    tags = user.Select(c => new FetchUserTag()
                    {
                        displayTag = c.displayTag,
                        isMatch = myTags.Any(a => a.tag == c.tag),
                    }).ToList(),
                };
                await SetAvatar(ctx, item, avatar);
                users.Add(item);
            }

            if (users.Count >= 100)
                break;

        }

        return new FetchUsersResponse()
        {
            data = users,
        };
    }

    private async Task<string> GetUrlFromSource(long accountId, AvatarSource source)
    {
        if (!Enum.IsDefined(source))
            throw new InvalidAvatarSourceException();
        var ctx = new DiscoContext();
        if (source == AvatarSource.Discord)
        {
            var entry = await ctx.accountDiscords.FirstOrDefaultAsync(a => a.accountId == accountId);
            if (entry == null || string.IsNullOrWhiteSpace(entry.avatarUrl))
                throw new AvatarNotFoundException();
            return entry.avatarUrl;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public async Task SetAvatarSource(long accountId, AvatarSource? source)
    {
        var ctx = new DiscoContext();
        var model = await ctx.accountAvatars.FirstOrDefaultAsync(a => a.accountId == accountId);
        if (source == null)
        {
            if (model != null)
            {
                ctx.accountAvatars.Remove(model);
                await ctx.SaveChangesAsync();
            }

            return;
        }
        
        var url = await GetUrlFromSource(accountId, source.Value);
        if (model == null)
        {
            await ctx.accountAvatars.AddAsync(new AccountAvatar()
            {
                accountId = accountId,
                source = source.Value,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                url = url,
            });
        }
        else
        {
            model.url = url;
            model.source = source.Value;
            model.updatedAt = DateTime.UtcNow;
        }
        
        await ctx.SaveChangesAsync();
    }

    public async Task<string> CreateDiscordToken(long accountId)
    {
        var id = Guid.NewGuid().ToString();
        var ctx = new DiscoContext();
        var item = await ctx.accountDiscordCodes.FirstOrDefaultAsync(a => a.accountId == accountId && !string.IsNullOrWhiteSpace(a.redirectUrl));
        if (item != null)
            return item.code;
        
        await ctx.accountDiscordCodes.AddAsync(new AccountDiscordCode()
        {
            accountId = accountId,
            code = id,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();
        return id;
    }

    public async Task SetDiscordTokenUrl(string code, string url)
    {
        var ctx = new DiscoContext();
        var item = await ctx.accountDiscordCodes.FirstOrDefaultAsync(a => a.code == code);
        if (item != null)
        {
            item.redirectUrl = url;
            await ctx.SaveChangesAsync();
        }
    }

    public async Task<DiscordStateResponse?> RedeemDiscordToken(string code, long accountId)
    {
        var ctx = new DiscoContext();
        var item = await ctx.accountDiscordCodes.FirstOrDefaultAsync(a => a.code == code && a.accountId == accountId);
        if (item != null)
        {
            var response = new DiscordStateResponse()
            {
                state = item.code,
                redirectUrl = item.redirectUrl,
            };
            var ok = item.updatedAt > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30));
            ctx.accountDiscordCodes.Remove(item);
            await ctx.SaveChangesAsync();
            if (ok)
            {
                return response;
            }
        }
        return null;
    }

    public async Task SetGender(long accountId, string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            gender = null;

        if (gender != null)
        {
            var rgx = new Regex("[^a-zA-Z]");
            gender = rgx.Replace(gender, "");
            
            // attempt normalization
            var lowerGender = gender.ToLowerInvariant();
            if (lowerGender is "male" or "man" or "he" or "him" or "guy" or "boy")
                gender = "Male";
            else if (lowerGender is "female" or "women" or "girl" or "she" or "her")
                gender = "Female";

            if (gender.Length is > 32 or < 3)
                throw new InvalidGenderLengthException();
        }

        var ctx = new DiscoContext();
        var account = await ctx.accounts.FindAsync(accountId);
        account.gender = gender;
        await ctx.SaveChangesAsync();
    }

    public async Task SetAge(long accountId, int? age)
    {
        if (age is > 100 or < 13)
            throw new InvalidAgeException();
        var ctx = new DiscoContext();
        var account = await ctx.accounts.FindAsync(accountId);
        account.age = age;
        await ctx.SaveChangesAsync();
    }

    public async Task SetPronouns(long accountId, string? pronouns)
    {
        if (pronouns != null)
        {
            if (!pronouns.Contains('/'))
                throw new InvalidPronounsException();
            var split = pronouns.Split("/");
            if (split.Length != 2)
                throw new InvalidPronounsException();

            // should not be equal since that's probably a typo (e.g. "he/he" when they meant "he/him")
            if (split[0].ToLowerInvariant() == split[1].ToLowerInvariant())
                throw new InvalidPronounsException();
            
            // TODO: is this limit ok? we might wanna revisit.
            
            if (split[0].Length is > 16 or < 1)
                throw new InvalidPronounsException();
            
            if (split[1].Length is > 16 or < 1)
                throw new InvalidPronounsException();
        }

        var ctx = new DiscoContext();
        var account = await ctx.accounts.FindAsync(accountId);
        account.pronouns = pronouns;
        await ctx.SaveChangesAsync();

    }

    public async Task<IEnumerable<AccountTag>> GetTags(long accountId)
    {
        var ctx = new DiscoContext();
        var tags = await ctx.accountTags.Where(a => a.accountId == accountId).ToListAsync();
        return tags;
    }

    private async Task<string?> GetMatrixProfilePictureUrl(string name, string domain)
    {
        // We use matrix.org to avoid exposing our server IP to random home servers. this should be configurable in the future.
        var url = "https://matrix.org/_matrix/client/r0/profile/" + System.Web.HttpUtility.UrlEncode("@" + name + ":" + domain);
        
        var parsedUrl = new Uri(url);
        if (parsedUrl.Host != "matrix.org" || parsedUrl.Scheme != "https")
            throw new Exception("Unsafe url");
        
        using var client = new HttpClient();
        var result = await client.GetAsync(parsedUrl);

        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            // User does not exist.
            throw new MatrixUserNotFoundException();
        }
        
        if (!result.IsSuccessStatusCode)
            throw new Exception("Failed to get profile picture");
        
        var body = await result.Content.ReadAsStringAsync();
        var jsonBody = JsonSerializer.Deserialize<MatrixUserInfoResponse>(body);
        if (string.IsNullOrWhiteSpace(jsonBody?.avatarUrl))
            return null;
        
        // Returns data like "mxc://matrix.org/XTuYJiMVIgfVHcMikXQSerEL"
        var parsed = new Uri(jsonBody.avatarUrl);
        if (parsed.Scheme != "mxc")
            throw new Exception("Invalid avatar url");
        var afterScheme = parsed.Host + parsed.PathAndQuery;
        var getImageUrl =
            "https://matrix.org/_matrix/media/r0/thumbnail/"+afterScheme+"?width=256&height=256";
        var parsedGetImageUrl = new Uri(getImageUrl);
        if (parsedGetImageUrl.Host != "matrix.org" || parsedGetImageUrl.Scheme != "https")
            throw new Exception("Unsafe url");
        return parsedGetImageUrl.ToString();
    }

    public async Task DeleteMatrix(long accountId)
    {
        var ctx = new DiscoContext();
        var toDelete = ctx.accountMatrix.Where(a => a.accountId == accountId);
        ctx.accountMatrix.RemoveRange(toDelete);
        
        var avatars = ctx.accountAvatars.Where(a => a.accountId == accountId && a.source == AvatarSource.Matrix);
        ctx.accountAvatars.RemoveRange(avatars);
        
        await ctx.SaveChangesAsync();
    }

    private async Task<string> GetHash(Stream file)
    {
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(file);
        return Convert.ToHexString(hash).ToLowerInvariant().Replace("-", "");
    }

    public async Task<long> InsertAndUploadImage(Stream rawImageStream, long accountId)
    {
        const int profileImageResolution = 256; // 256x256 is hard coded in a lot of places. don't change this.
        var imageStream = new MemoryStream(1024 * 1024 * 8); // Limit to 8mb. We'll resize which will make the final image considerably smaller, but we still don't want to DOS ourselves if we get too many images.
        await rawImageStream.CopyToAsync(imageStream);
        imageStream.Position = 0;
        var originalSize = imageStream.Length;
        
        // get original hash
        var originalHash = await GetHash(imageStream);
        imageStream.Position = 0;
        
        // Read, resize, convert to webp.
        using var image = await Image.LoadAsync(imageStream);
        var resized = image.Clone(a => a.Resize(new ResizeOptions()
        {
            Mode = ResizeMode.Crop,
            Size = new Size(profileImageResolution, profileImageResolution),
        }));
        imageStream = new MemoryStream();
        await resized.SaveAsync(imageStream, new WebpEncoder()
        {
            Quality = 50,
            FileFormat = WebpFileFormatType.Lossy,
            SkipMetadata = true,
            NearLossless = true,
            NearLosslessQuality = 50,
            Method = WebpEncodingMethod.BestQuality,
        });
        imageStream.Position = 0;
        
        var newHash = await GetHash(imageStream);
        imageStream.Position = 0;
        
        var newSize = imageStream.Length;
        logger.LogInformation("Image size diff: {0} vs {1} ({2})", originalSize, newSize, originalSize - newSize);
        
        // Try to lookup image
        var ctx = new DiscoContext();

        var exists = await ctx.images.FirstOrDefaultAsync(a => a.sha256Hash == newHash || a.originalSha256Hash == originalHash);
        if (exists != null)
        {
            // Image already exists, just return the id.
            return exists.userUploadedImageId;
        }

        // Upload under new hash.
        var fullUploadPath = uploadImagePath + newHash + ".webp";
        await using var fileStream = File.OpenWrite(fullUploadPath);
        await imageStream.CopyToAsync(fileStream);
        
        var dbImage = await ctx.AddAsync(new UserUploadedImage()
        {
            sha256Hash = newHash,
            originalSha256Hash = originalHash,
            createdAt = DateTime.UtcNow,
            fileSize = imageStream.Length,
            format = ImageFormat.FormatWebP,
            sizeX = profileImageResolution,
            sizeY = profileImageResolution,
            status = ImageStatus.AwaitingApproval,
            updatedAt = DateTime.UtcNow,
            accountId = accountId,
        });
        await ctx.SaveChangesAsync();
        return dbImage.Entity.userUploadedImageId;
    }
    
    public async Task SetMatrixAccount(long accountId, string fullUsername)
    {
        var separator = fullUsername.LastIndexOf(':');
        if (separator == -1)
            throw new InvalidMatrixUsernameException();
        
        var username = fullUsername.Substring(0, separator);
        var domain = fullUsername.Substring(separator + 1);
        
        if (username.StartsWith('@'))
            username = username.Substring(1);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(domain))
            throw new InvalidMatrixUsernameException();
        
        if (!domain.Contains('.') || domain.Length < 3 || domain.Length > 255)
            throw new InvalidMatrixUsernameException();

        if (username.Length is < 1 or > 64)
            throw new InvalidMatrixUsernameException();
        
        var imageUrl = await GetMatrixProfilePictureUrl(username, domain);
        var ctx = new DiscoContext();
        
        // We can't delete accounts like we do with discord since we don't verify matrix accounts.
        // We can delete the current accountId's accounts though.
        var toDelete = ctx.accountMatrix.Where(a => a.accountId == accountId);
        ctx.accountMatrix.RemoveRange(toDelete);
        // Add
        ctx.accountMatrix.Add(new AccountMatrix()
        {
            accountId = accountId,
            domain = domain,
            name = username,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
            avatarUrl = imageUrl,
        });
        // Update image, if required
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            // Fetch image
            var image = await new HttpClient().GetAsync(imageUrl);
            if (!image.IsSuccessStatusCode)
                throw new Exception("Failed to fetch image");
            
            var userUploadedImageId = await InsertAndUploadImage(await image.Content.ReadAsStreamAsync(), accountId);
            // Delete old.
            var avatars = ctx.accountAvatars.Where(a => a.accountId == accountId);
            ctx.accountAvatars.RemoveRange(avatars);
            // Insert new.
            ctx.accountAvatars.Add(new AccountAvatar()
            {
                source = AvatarSource.UserUploadedImage,
                accountId = accountId,
                url = null,
                userUploadedImageId = userUploadedImageId,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
            });
        }
        // Save
        await ctx.SaveChangesAsync();
    }

    public async Task AttachDiscordAccount(long accountId, long discordId, string fullDiscordName, string? imageUrl)
    {
        var parsed = AccountDiscord.Parse(fullDiscordName);
        var ctx = new DiscoContext();
        // check if this discord account is banned.
        var banned = await ctx.accountDiscordBans.FirstOrDefaultAsync(a => a.discordId == discordId);
        if (banned != null)
        {
            // Likely user trying to make another account. Ban them.
            await BanAccount(accountId, "Terms of service violation on a previous account.");
            throw new AccountBannedException("Terms of service violation on a previous account.");
        }
        // First, delete any existing entries
        var toDelete = ctx.accountDiscords.Where(a => a.discordId == discordId || a.accountId == accountId);
        ctx.accountDiscords.RemoveRange(toDelete);
        await ctx.SaveChangesAsync();
        long? userImageId = null;
        // Now add the account
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            var imageUrlToFetch = "https://cdn.discordapp.com/avatars/"+discordId+"/"+imageUrl+".png";
            var image = await new HttpClient().GetAsync(imageUrlToFetch);
            if (image.IsSuccessStatusCode)
            {
                var stream = await image.Content.ReadAsStreamAsync();
                // TODO: ideally, we would fail silently if image stuff errors.
                userImageId = await InsertAndUploadImage(stream, accountId);
            }
            else
            {
                logger.LogWarning("Failed to fetch discord avatar for {discordAccountId} ({imageUrl})", discordId, imageUrlToFetch);
            }
        }
        await ctx.accountDiscords.AddAsync(new AccountDiscord()
        {
            discordId = discordId,
            accountId = accountId,
            avatarUrl = imageUrl,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
            name = parsed.Item1,
            tag = parsed.Item2,
        });
        // If user has a discord avatar, update on-site
        if (userImageId != null)
        {
            var hasAvatar = await ctx.accountAvatars.FirstOrDefaultAsync(a => a.accountId == accountId);
            if (hasAvatar == null)
            {
                // Insert    
                await ctx.accountAvatars.AddAsync(new AccountAvatar()
                {
                    accountId = accountId,
                    createdAt = DateTime.UtcNow,
                    source = AvatarSource.UserUploadedImage,
                    updatedAt = DateTime.UtcNow,
                    userUploadedImageId = userImageId,
                    url = null,
                });
            }
            else if (hasAvatar.source == AvatarSource.UserUploadedImage && hasAvatar.userUploadedImageId != userImageId)
            {
                // Update
                hasAvatar.userUploadedImageId = userImageId;
            }
        }

        await ctx.SaveChangesAsync();
    }

    public async Task<AccountMatrix?> GetMatrixForAccount(long accountId)
    {
        var ctx = new DiscoContext();
        var account = await ctx.accountMatrix.FirstOrDefaultAsync(a => a.accountId == accountId);
        return account;
    }
    
    public async Task<AccountDiscord?> GetDiscordForAccount(long accountId)
    {
        var ctx = new DiscoContext();
        var account = await ctx.accountDiscords.FirstOrDefaultAsync(a => a.accountId == accountId);
        return account;
    }

    public async Task<IEnumerable<UserUploadedImage>> GetImagesAwaitingReview()
    {
        var ctx = new DiscoContext();
        var images = await ctx.images.Where(a => a.status == ImageStatus.AwaitingApproval).Take(100).ToListAsync();
        return images;
    }

    public async Task DeleteImage(long userUploadedImageId)
    {
        var ctx = new DiscoContext();
        var image = await ctx.images.FirstOrDefaultAsync(a => a.userUploadedImageId == userUploadedImageId);
        if (image == null) throw new Exception("Image not found");

        var path = uploadImagePath + image.sha256Hash + "." + image.extension;
        if (File.Exists(path))
            File.Delete(path);
    }

    public async Task SetImageStatus(long userUploadedImageId, ImageStatus status)
    {
        var ctx = new DiscoContext();
        var image = await ctx.images.FirstOrDefaultAsync(a => a.userUploadedImageId == userUploadedImageId);
        if (image == null) throw new Exception("Image not found");
        image.status = status;
        await ctx.SaveChangesAsync();
    }

    public async Task<AccountAvatar?> GetAvatarForAccount(long accountId)
    {
        var ctx = new DiscoContext();
        var avatar = await ctx.accountAvatars.FirstOrDefaultAsync(a => a.accountId == accountId);
        if (avatar == null) return avatar;
        if (avatar.source != AvatarSource.UserUploadedImage || avatar.userUploadedImageId == null) return avatar;
        
        // check status, then set url if approved.
        var uploadedImage = await ctx.images.FirstOrDefaultAsync(a => a.userUploadedImageId == avatar.userUploadedImageId.Value);
        if (uploadedImage != null)
        {
            if (uploadedImage.status == ImageStatus.Approved)
            {
                avatar.url = uploadedImage.url;
            }
            else
            {
                // not approved, so return null for now.
                return null;
            }
        }
        
        return avatar;
    }

    public async Task DeleteDiscord(long accountId)
    {
        var ctx = new DiscoContext();
        var linked = await ctx.accountDiscords.Where(a => a.accountId == accountId).ToListAsync();
        ctx.RemoveRange(linked);
        await ctx.SaveChangesAsync();
    }
    
    public async Task DeleteAvatar(long accountId)
    {
        var ctx = new DiscoContext();
        var linked = await ctx.accountAvatars.Where(a => a.accountId == accountId).ToListAsync();
        ctx.RemoveRange(linked);
        await ctx.SaveChangesAsync();
    }

    public async Task<bool> IsRelationshipMutual(long accountId, long targetId)
    {
        var ctx = new DiscoContext();
        var existing = await ctx.accountRelationships.FirstOrDefaultAsync(a => a.accountId == accountId && a.otherAccountId == targetId);
        if (existing == null)
            return false;
        
        var other = await ctx.accountRelationships.FirstOrDefaultAsync(a => a.accountId == targetId && a.otherAccountId == accountId);
        if (other == null)
            return false;
        
        return existing.relationship == RelationStatus.Accepted && other.relationship == RelationStatus.Accepted;
    }
    
    public async Task<AccountRelationshipUpdateResponse> UpdateRelationship(long accountId, long targetId, RelationStatus status)
    {
        if (!Enum.IsDefined(status))
            throw new InvalidRelationStatusException();
        
        var ctx = new DiscoContext();
        var existing = await ctx.accountRelationships.FirstOrDefaultAsync(a => a.accountId == accountId && a.otherAccountId == targetId);
        if (existing != null)
        {
            existing.relationship = status;
        }
        else
        {
            await ctx.accountRelationships.AddAsync(new AccountRelationship()
            {
                accountId = accountId,
                otherAccountId = targetId,
                relationship = status,
            });
        }
        await ctx.SaveChangesAsync();
        // check for mutual like
        if (status == RelationStatus.Accepted)
        {
            var other = await ctx.accountRelationships.FirstOrDefaultAsync(a => a.accountId == targetId && a.otherAccountId == accountId);
            if (other != null && other.relationship == RelationStatus.Accepted)
            {
                return new AccountRelationshipUpdateResponse()
                {
                    status = status,
                    isMutualLike = true,
                };
            }
        }

        return new AccountRelationshipUpdateResponse()
        {
            status = status,
            isMutualLike = false,
        };
    }

    public async Task UpdatePassword(long accountId, string originalPassword, string newPassword)
    {
        var ctx = new DiscoContext();
        var pass = await ctx.accountPasswords.FirstOrDefaultAsync(a => a.accountId == accountId);
        var ok = await IsPasswordValid(pass.hash, originalPassword);
        if (!ok)
            throw new InvalidUsernameOrPasswordException();
        // update
        pass.hash = await HashPassword(newPassword);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAccount(long accountId)
    {
        using var deleteAccountScope = logger.BeginScope("DeleteAccount");
        logger.LogInformation("Account deletion requested for {0}", accountId);
        // Delete all account data
        var ctx = new DiscoContext();
        var account = await ctx.accounts.FirstOrDefaultAsync(a => a.accountId == accountId);
        if (account == null)
            throw new Exception("Account not found");
        var tags = await ctx.accountTags.Where(a => a.accountId == accountId).ToListAsync();
        var avatars = await ctx.accountAvatars.Where(a => a.accountId == accountId).ToListAsync();
        var discords = await ctx.accountDiscords.Where(a => a.accountId == accountId).ToListAsync();
        var relationships = await ctx.accountRelationships.Where(a => a.accountId == accountId || a.otherAccountId == accountId).ToListAsync();
        var reports = await ctx.accountReports.Where(a => a.accountId == accountId || a.accountReportId == accountId).ToListAsync();
        var passwords = await ctx.accountPasswords.Where(a => a.accountId == accountId).ToListAsync();
        var tokens = await ctx.accountDiscordCodes.Where(a => a.accountId == accountId).ToListAsync();
        var sessions = await ctx.accountSessions.Where(a => a.accountId == accountId).ToListAsync();
        var matrixLinks = await ctx.accountMatrix.Where(a => a.accountId == accountId).ToListAsync();
        
        ctx.RemoveRange(tags);
        ctx.RemoveRange(avatars);
        ctx.RemoveRange(discords);
        ctx.RemoveRange(relationships);
        ctx.RemoveRange(reports);
        ctx.RemoveRange(passwords);
        ctx.RemoveRange(tokens);
        ctx.RemoveRange(sessions);
        ctx.RemoveRange(matrixLinks);
        ctx.Remove(account);
        
        // Look for image files and delete those if nobody else is using them.
        // This isn't really a PII issue - if two people upload the same image, the image is probably public/not owned by the authenticated user. They can always email the site owner if they have an issue with it (e.g. someone stole your pfp from twitter and used that).
        var userUploads = await ctx.images.Where(a => a.accountId == accountId).ToListAsync();
        foreach (var upload in userUploads)
        {
            var inUse = await ctx.accountAvatars.AnyAsync(a => a.userUploadedImageId == upload.userUploadedImageId && a.accountId != accountId);
            if (inUse)
            {
                logger.LogInformation("Skip deletion of user upload {userUploadImageId}, it is in use by at least one other user", upload.userUploadedImageId);
                continue;
            }
            
            // We can safely delete the images.
            var imagePath = uploadImagePath + upload.sha256Hash + "." + upload.extension;
            try
            {
                if (File.Exists(imagePath))
                    File.Delete(imagePath);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error deleting {imagePath}", imagePath);
            }
        }
        await ctx.SaveChangesAsync();
        logger.LogInformation("Successfully deleted userAccount {accountId}", accountId);
    }

    public async Task<IEnumerable<AccountReport>> GetPendingReports()
    {
        var ctx = new DiscoContext();
        var reports = await ctx.accountReports.Where(a => a.state == ReportState.Created).ToListAsync();
        return reports;
    }
    
    public async Task SetReportState(long reportId, ReportState state)
    {
        var ctx = new DiscoContext();
        var report = await ctx.accountReports.FirstOrDefaultAsync(a => a.accountReportId == reportId);
        if (report == null)
            throw new Exception("Report not found");
        report.state = state;
        report.updatedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();
    }

    public async Task ReportUser(long contextAccountId, long accountId, ReportReason reason, ReportField field)
    {
        if (!Enum.IsDefined(field))
            throw new Exception("Bad field");
        if (!Enum.IsDefined(reason))
            throw new Exception("Bad reason");
        var ctx = new DiscoContext();
        // make sure user exists
        var accountToReport = await ctx.accounts.FirstOrDefaultAsync(a => a.accountId == accountId);
        if (accountToReport == null)
            return;
        // check if this user has already been reported (unreviewed) by this user
        var exists = await ctx.accountReports.FirstOrDefaultAsync(a => a.accountId == contextAccountId && a.reportedAccountId == accountId && a.state == ReportState.Created);
        if (exists != null)
            return;
        // check if user has already been reported with this field (unreviewed)
        var exists2 = await ctx.accountReports.FirstOrDefaultAsync(a => a.reportedAccountId == accountId && a.field == field && a.state == ReportState.Created);
        if (exists2 != null)
            return;
        // check if contextAccountId has made over 10 reports in the last 5 minutes
        var recent = await ctx.accountReports.Where(a => a.accountId == contextAccountId && a.createdAt > DateTime.UtcNow.AddMinutes(-5)).ToListAsync();
        if (recent.Count >= 10)
            return;
        
        await ctx.accountReports.AddAsync(new AccountReport()
        {
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
            accountId = contextAccountId,
            reportedAccountId = accountId,
            reason = reason,
            field = field,
            state = ReportState.Created,
        });
        await ctx.SaveChangesAsync();
    }
    
    public async Task<AccountBan?> GetBan(long accountId)
    {
        var ctx = new DiscoContext();
        var ban = await ctx.accountBans.FirstOrDefaultAsync(a => a.bannedAccountId == accountId);
        return ban;
    }
    
    public async Task BanAccount(long bannedAccountId, string reason)
    {
        var ctx = new DiscoContext();
        var existingBan = await ctx.accountBans.FirstOrDefaultAsync(a => a.bannedAccountId == bannedAccountId);
        if (existingBan != null)
            return;
        var ban = await ctx.accountBans.AddAsync(new AccountBan()
        {
            bannedAccountId = bannedAccountId,
            reason = reason,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();
        // ban any discords
        var discords = await ctx.accountDiscords.Where(a => a.accountId == bannedAccountId).ToListAsync();
        foreach (var item in discords)
        {
            await ctx.accountDiscordBans.AddAsync(new AccountDiscordBan()
            {
                discordId = item.discordId,
                bannedAccountId = bannedAccountId,
            });
        }
        await ctx.SaveChangesAsync();
    }
    
    public async Task UnbanAccount(long bannedAccountId)
    {
        var ctx = new DiscoContext();
        var existingBan = await ctx.accountBans.FirstOrDefaultAsync(a => a.bannedAccountId == bannedAccountId);
        if (existingBan == null)
            return;
        ctx.accountBans.Remove(existingBan);
        var discords = await ctx.accountDiscordBans.Where(a => a.bannedAccountId == bannedAccountId).ToListAsync();
        ctx.accountDiscordBans.RemoveRange(discords);
        await ctx.SaveChangesAsync();
    }

    public async Task<IEnumerable<Account>> FetchAllUsers()
    {
        var ctx = new DiscoContext();
        var accounts = await ctx.accounts.ToListAsync();
        return accounts;
    }

    public async Task<IEnumerable<TopTagWithCount>> GetTopTags()
    {
        // TODO: cache
        var ctx = new DiscoContext();
        var normalizedTags = await ctx.accountTags.GroupBy(a => a.tag).OrderByDescending(a => a.Count()).Select(c => new TopTagWithCount()
        {
            tag = c.Key,
            count = c.Count(),
        }).Take(100).ToListAsync();
        // convert to admin tags
        var adminList = new List<TopTagWithCount>();
        foreach (var data in normalizedTags)
        {
            var adminData = await ctx.topTags.FirstOrDefaultAsync(a => a.tag == data.tag);
            if (adminData == null)
                continue; // not approved
            data.displayTag = adminData.displayTag;
            adminList.Add(data);
            if (adminList.Count == 25)
                break;
        }
        
        return adminList;
    }

    public async Task<IEnumerable<TopTagWithCount>> GetTopTagsUnfiltered()
    {
        var ctx = new DiscoContext();
        var normalizedTags = await ctx.accountTags.GroupBy(a => a.tag).OrderByDescending(a => a.Count()).Take(100).Select(a => new TopTagWithCount()
        {
            tag = a.Key,
            count = a.Count()
        }).ToListAsync();

        return normalizedTags;
    }

    public async Task ApproveTopTag(string tag, string displayTag)
    {
        var ctx = new DiscoContext();
        // add to toptags
        var existing = await ctx.topTags.FirstOrDefaultAsync(a => a.tag == tag);
        if (existing != null)
            return;

        await ctx.topTags.AddAsync(new TopTag()
        {
            displayTag = displayTag,
            tag = tag,
        });
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteTopTag(string tag)
    {
        var ctx = new DiscoContext();
        var existing = await ctx.topTags.FirstOrDefaultAsync(a => a.tag == tag);
        if (existing == null)
            return;
        ctx.Remove(existing);
        await ctx.SaveChangesAsync();
    }
}