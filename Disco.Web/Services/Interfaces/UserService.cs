using Disco.Web.Data;
using Disco.Web.Models.User;

namespace Disco.Web.Services;

public interface IUserService
{
    Task<Account> CreateAccount(string username, string password);
    Task<Account?> GetAccountByName(string username);
    Task<Account?> GetAccountById(long accountId);
    Task<AccountDescription?> GetDescription(long accountId);
    Task<IEnumerable<AccountTag>> GetTags(long accountId);
    Task<bool> IsPasswordValid(long accountId, string providedValue);
    Task<AccountSession> CreateSession(long accountId);
    Task<AccountSession?> GetSessionAndUpdate(string id);
    Task<AccountTag> AddTag(long accountId, string tag);
    Task DeleteTag(long accountId, long tagId);
    Task DeleteAvatar(long accountId);
    Task SetDescription(long accountId, string? description);
    Task SetDisplayName(long accountId, string displayName);
    Task SetGender(long accountId, string? gender);
    Task SetAge(long accountId, int? age);
    Task SetPronouns(long accountId, string? pronouns);
    /// <summary>
    /// Fetch users similar to the contextAccountId.
    /// </summary>
    /// <remarks>
    /// Full rules (so far):
    /// - The users returned must have at least one normalized "tag" in common with the contextAccountId.
    /// - The users returned must have at least one social media account in common with the contextAccountId. For example, if the contextAccountId has a discord account attached, all users returned must have a discord account.
    /// - If the contextAccountId does not have any tags, this method should return nothing.
    /// - Users cannot be matched with accounts that have been offline for over 1 month
    /// - Users cannot be matched with accounts that have a entry in the AccountRelationship matching the contextAccountId
    /// - If the user age is unknown or 18+, they can only be matched with users who have null age or 18+ age. Likewise, if the age is 17 and below, the user can only be matched with users 17 and below.
    /// - There is no expected sort, startId/cursor, or limit at this time.
    /// </remarks>
    /// <param name="contextAccountId"></param>
    /// <returns>A list of users who might be a good match for the contextAccountId</returns>
    Task<FetchUsersResponse> FetchUsers(long contextAccountId);

    Task<FetchUsersResponse> FetchUsersByStatus(long contextUserId, RelationStatus status, long startAccountId);
    Task SetAvatarSource(long accountId, AvatarSource? source);
    Task<string> CreateDiscordToken(long accountId);
    Task SetDiscordTokenUrl(string code, string newUrl);
    Task<DiscordStateResponse?> RedeemDiscordToken(string code, long accountId);
    Task SetMatrixAccount(long accountId, string fullUsername);
    Task DeleteMatrix(long accountId);
    Task AttachDiscordAccount(long accountId, long discordId, string fullDiscordName, string? imageUrl);
    Task<AccountDiscord?> GetDiscordForAccount(long accountId);
    Task<AccountMatrix?> GetMatrixForAccount(long accountId);
    Task<AccountAvatar?> GetAvatarForAccount(long accountId);
    Task DeleteDiscord(long accountId);
    Task<bool> IsRelationshipMutual(long accountId, long targetId);
    Task<AccountRelationshipUpdateResponse> UpdateRelationship(long accountId, long targetAccountId, RelationStatus status);
    Task UpdatePassword(long accountId, string originalPassword, string newPassword);
    Task ReportUser(long contextAccountId, long accountId, ReportReason reason, ReportField field);
    Task<IEnumerable<AccountReport>> GetPendingReports();
    Task SetReportState(long reportId, ReportState state);
    Task<AccountBan?> GetBan(long accountId);
    Task BanAccount(long accountId, string reason);
    Task UnbanAccount(long accountId);
    Task<IEnumerable<Account>> FetchAllUsers();
    Task DeleteAccount(long accountId);
    Task<IEnumerable<UserUploadedImage>> GetImagesAwaitingReview();
    Task SetImageStatus(long userUploadedImageId, ImageStatus status);
    Task DeleteImage(long userUploadedImageId);
    Task<IEnumerable<TopTagWithCount>> GetTopTags();
    Task<IEnumerable<TopTagWithCount>> GetTopTagsUnfiltered();
    Task ApproveTopTag(string tag, string displayTag);
    Task DeleteTopTag(string tag);
}