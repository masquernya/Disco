using Disco.Web.Data;

namespace Disco.Web.Models.User;

public class FullUserInfoResponse
{
    public Account account { get; set; }
    public IEnumerable<AccountTag> accountTags { get; set; }
    public AccountDescription description { get; set; }
    public AccountDiscord discord { get; set; }
    public AccountAvatar avatar { get; set; }
}