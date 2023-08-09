using Disco.Web.Data;

namespace Disco.Web.Models.User;

public class FullUserInfoResponse
{
    public FullUserInfoResponse(Account account)
    {
        this.account = account;
    }
    public Account account { get; set; }
    public IEnumerable<AccountTag> accountTags { get; set; } = ArraySegment<AccountTag>.Empty;
    public AccountDescription? description { get; set; }
    public AccountDiscord? discord { get; set; }
    public AccountAvatar? avatar { get; set; }
    public AccountMatrix? matrix { get; set; }
}