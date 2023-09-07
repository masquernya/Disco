using Disco.Web.Data;
using Disco.Web.Exceptions.User;
using Disco.Web.Models.Matrix;
using Microsoft.EntityFrameworkCore;

namespace Disco.Web.Services;

public class MatrixSpaceService : IMatrixSpaceService
{
    private ILogger logger { get; set; }
    private ICacheHelperService cache { get; set; }
    private IUserService userService { get; set; }

    public MatrixSpaceService(ILogger logger, ICacheHelperService cache, IUserService userService)
    {
        this.logger = logger;
        this.cache = cache;
        this.userService = userService; // Temporary hack for uploads.
    }
    
    public string GetMatrixProfilePictureUrl(string mxcUrl)
    {
        // Returns data like "mxc://matrix.org/1234"
        var parsed = new Uri(mxcUrl);
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

    private async Task<long> UploadImageForMatrix(DiscoContext ctx, string avatar, long matrixSpaceId)
    {
        // Fetch image
        var image = await new HttpClient().GetAsync(avatar);
        if (!image.IsSuccessStatusCode)
            throw new Exception("Failed to fetch image");
            
        var userUploadedImageId = await userService.InsertAndUploadImage(await image.Content.ReadAsStreamAsync(), 1);
        return userUploadedImageId;
    }

    public async Task<MatrixSpace> AddOrUpdateMatrixSpace(string invite, string name, string? description, int memberCount, string? avatar,
        string[]? admins)
    {
        await using var ctx = new DiscoContext();

        if (avatar != null)
            avatar = GetMatrixProfilePictureUrl(avatar);
        
        var exists = await ctx.matrixSpaces.FirstOrDefaultAsync(a => a.invite == invite);
        if (exists != null)
        {
            if (admins != null)
            {
                var currentAdmins = await ctx.matrixSpaceAdmins.Where(a => a.matrixSpaceId == exists.matrixSpaceId)
                    .ToListAsync();
                
                // Remove old admins
                foreach (var admin in currentAdmins)
                {
                    if (admins.All(a => a != admin.matrixUserId))
                    {
                        ctx.matrixSpaceAdmins.Remove(admin);
                    }
                }

                // Add new admins
                foreach (var admin in admins)
                {
                    if (currentAdmins.All(a => a.matrixUserId != admin))
                    {
                        ctx.matrixSpaceAdmins.Add(new MatrixSpaceAdmin()
                        {
                            matrixSpaceId = exists.matrixSpaceId,
                            matrixUserId = admin,
                        });
                    }
                }
            }

            exists.name = name;
            exists.description = description;
            exists.memberCount = memberCount;
            exists.updatedAt = DateTime.UtcNow;
            if (avatar != null)
            {
                exists.imageId = await UploadImageForMatrix(ctx, avatar, exists.matrixSpaceId);
            }
            await ctx.SaveChangesAsync();
            return exists;
        }

        // Insert
        var result = await ctx.matrixSpaces.AddAsync(new MatrixSpace()
        {
            invite = invite,
            name = name,
            description = description,
            memberCount = memberCount,
            imageId = null,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();
        if (avatar != null)
        {
            result.Entity.imageId = await UploadImageForMatrix(ctx, avatar, result.Entity.matrixSpaceId);
            await ctx.SaveChangesAsync();
        }
        foreach (var admin in admins)
        {
            await ctx.matrixSpaceAdmins.AddAsync(new MatrixSpaceAdmin()
            {
                matrixSpaceId = result.Entity.matrixSpaceId,
                matrixUserId = admin,
            });
        }

        return result.Entity;
    }

    public async Task<IEnumerable<MatrixSpace>> GetManagedSpaces(long accountId)
    {
        await using var ctx = new DiscoContext();
        var myMatrixAccount = await ctx.accountMatrix.Where(a => a.accountId == accountId).ToListAsync();
        var all = new List<MatrixSpace>();
        foreach (var account in myMatrixAccount)
        {
            var str = account.GetDisplayString();
            var adminSpaces = await ctx.matrixSpaceAdmins.Where(a => a.matrixUserId == str).ToListAsync();
            foreach (var space in adminSpaces)
            {
                var spaceData = await ctx.matrixSpaces.FirstOrDefaultAsync(a => a.matrixSpaceId == space.matrixSpaceId);
                if (spaceData != null)
                    all.Add(spaceData);
            }
        }
        
        return all;
    }

    private async Task<bool> DoesHavePermission(long accountId, long matrixSpaceId)
    {
        return (await GetManagedSpaces(accountId)).Any(a => a.matrixSpaceId == matrixSpaceId);
    }

    public async Task<IEnumerable<MatrixSpaceWithDetails>> GetAllSpaces()
    {
        await using var ctx = new DiscoContext();
        
        var spaces = await ctx.matrixSpaces.ToListAsync();
        var spaceIds = spaces.Select(a => a.matrixSpaceId).ToList();
        var tags = await ctx.matrixSpaceTags.Where(a => spaceIds.Contains(a.matrixSpaceId)).ToListAsync();
        
        var result = new List<MatrixSpaceWithDetails>();
        foreach (var space in spaces)
        {
            var image = space.imageId != null
                ? (await ctx.images.FirstOrDefaultAsync(a =>
                    a.userUploadedImageId == space.imageId))
                : null;
            result.Add(new MatrixSpaceWithDetails()
            {
                space = space,
                tags = tags.Where(a => a.matrixSpaceId == space.matrixSpaceId).Select(a => a.tag).ToList(),
                imageUrl = image?.status == ImageStatus.Approved ? image.url : null
            });
        }

        return result;
    }

    public async Task SetIs18Plus(long accountId, long matrixSpaceId, bool is18Plus)
    {
        if (!await DoesHavePermission(accountId, matrixSpaceId))
            throw new UnauthorizedException();
        
        await using var ctx = new DiscoContext();
        var space = await ctx.matrixSpaces.FirstOrDefaultAsync(a => a.matrixSpaceId == matrixSpaceId);
        if (space == null)
            throw new ArgumentException("Space not found", nameof(matrixSpaceId));
        
        space.is18Plus = is18Plus;
        await ctx.SaveChangesAsync();
    }
}