namespace Disco.Web.Models.User;

public class MyTag
{
    public long tagId { get; set; }
    public string tag { get; set; }
}

public class MyAccountResponse
{
    public long accountId { get; set; }
    public string username { get; set; }
    public string displayName { get; set; }
    public string? gender { get; set; }
    public int? age { get; set; }
    public string? pronouns { get; set; }
    public string? description { get; set; }
    public IEnumerable<MyTag> tags { get; set; }
    public bool isAdmin => Config.IsAdmin(accountId);
}