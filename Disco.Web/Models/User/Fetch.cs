using Disco.Web.Data;

namespace Disco.Web.Models.User;

public class FetchUserTag
{
    public string displayTag { get; set; }
    public bool isMatch { get; set; }
}

public class UserAvatar
{
    public AvatarSource source { get; set; }
    public string imageUrl { get; set; }
}

public class UserSocialMedia
{
    public SocialMedia type { get; set; }
    public string displayString { get; set; }
}

public class FetchUser
{
    public long accountId { get; set; }
    public string displayName { get; set; }
    public string username { get; set; }
    public int? age { get; set; }
    public string? pronouns { get; set; }
    public string? gender { get; set; }
    public string? description { get; set; }
    public IEnumerable<UserSocialMedia> socialMedia { get; set; }
    public UserAvatar? avatar { get; set; }
    public List<FetchUserTag> tags { get; set; }
}

public class FetchUsersResponse
{
    public IEnumerable<FetchUser> data { get; set; }
}