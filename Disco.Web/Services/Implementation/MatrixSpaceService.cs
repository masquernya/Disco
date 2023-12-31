using Disco.Web.Data;
using Disco.Web.Exceptions.User;
using Disco.Web.Models;
using Disco.Web.Models.Matrix;
using Microsoft.EntityFrameworkCore;

namespace Disco.Web.Services;

public class MatrixSpaceService : IMatrixSpaceService, IDiscoService, ITransientDiscoService<IMatrixSpaceService>
{
    private ILogger logger { get; set; }
    private ICacheHelperService cache { get; set; }
    private IUserService userService { get; set; }

    public MatrixSpaceService(ILogger<MatrixSpaceService> logger, ICacheHelperService cache, IUserService userService)
    {
        this.logger = logger;
        this.cache = cache;
        this.userService = userService; // Temporary hack for uploads.
    }

    private async Task<long> UploadImageForMatrix(DiscoContext ctx, string matrixAvatarUrl, long matrixSpaceId)
    {
        var safeUrl = MatrixHelpers.GetPictureUrlFromMatrixUrl(matrixAvatarUrl);
        var exists = await ctx.images.FirstOrDefaultAsync(a => a.originalUrl == safeUrl || a.originalUrl == matrixAvatarUrl);
        if (exists != null)
            return exists.userUploadedImageId;
        // Fetch image
        var image = await new HttpClient().GetAsync(safeUrl);
        if (!image.IsSuccessStatusCode)
            throw new Exception("Failed to fetch image");
            
        var userUploadedImageId = await userService.InsertAndUploadImage(await image.Content.ReadAsStreamAsync(), matrixAvatarUrl, 1);
        return userUploadedImageId;
    }

    public async Task<MatrixSpace> AddOrUpdateMatrixSpace(string invite, string name, string? description, int memberCount, string? avatar, bool is18Plus,
        string[]? admins)
    {
        await using var ctx = new DiscoContext();

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
                            createdAt = DateTime.UtcNow,
                            updatedAt = DateTime.UtcNow,
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
            is18Plus = is18Plus,
        });
        await ctx.SaveChangesAsync();
        if (avatar != null)
        {
            result.Entity.imageId = await UploadImageForMatrix(ctx, avatar, result.Entity.matrixSpaceId);
            await ctx.SaveChangesAsync();
        }

        if (admins != null && admins.Length > 0)
        {
            foreach (var admin in admins)
            {
                await ctx.matrixSpaceAdmins.AddAsync(new MatrixSpaceAdmin()
                {
                    matrixSpaceId = result.Entity.matrixSpaceId,
                    matrixUserId = admin,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                });
            }
            await ctx.SaveChangesAsync();
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
                var spaceData = await ctx.matrixSpaces.FirstOrDefaultAsync(a => a.matrixSpaceId == space.matrixSpaceId && a.isBanned);
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
        
        var spaces = await ctx.matrixSpaces.Where(x => !x.isBanned).ToListAsync();
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
                tags = tags.Where(a => a.matrixSpaceId == space.matrixSpaceId).ToList(),
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

    public async Task<MatrixSpaceTag> AddTag(long accountId, long matrixSpaceId, string tag)
    {
        if (!await DoesHavePermission(accountId, matrixSpaceId))
            throw new UnauthorizedException();
        
        await using var ctx = new DiscoContext();
        var normalizedTag = AccountTag.NormalizeTag(tag);
        
        var exists = await ctx.matrixSpaceTags.FirstOrDefaultAsync(a => a.matrixSpaceId == matrixSpaceId && a.tag == normalizedTag);
        if (exists != null)
            return exists;

        var result = await ctx.matrixSpaceTags.AddAsync(new MatrixSpaceTag()
        {
            matrixSpaceId = matrixSpaceId,
            tag = normalizedTag,
            displayTag = tag,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteTag(long accountId, long matrixSpaceId, long tagId)
    {
        if (!await DoesHavePermission(accountId, matrixSpaceId))
            throw new UnauthorizedException();
        
        await using var ctx = new DiscoContext();
        var tag = await ctx.matrixSpaceTags.FirstOrDefaultAsync(a => a.matrixSpaceTagId == tagId && a.matrixSpaceId == matrixSpaceId);
        if (tag == null)
            throw new ArgumentException("Tag not found", nameof(tagId));
        
        ctx.matrixSpaceTags.Remove(tag);
        await ctx.SaveChangesAsync();
    }

    public async Task BanSpace(long matrixSpaceId)
    {
        await using var ctx = new DiscoContext();
        var space = await ctx.matrixSpaces.FirstOrDefaultAsync(a => a.matrixSpaceId == matrixSpaceId);
        if (space == null)
            throw new ArgumentException("Space not found", nameof(matrixSpaceId));
        space.isBanned = true;
        await ctx.SaveChangesAsync();
    }
}