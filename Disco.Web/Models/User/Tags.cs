namespace Disco.Web.Models.User;

public class AddTagRequest
{
    public string tag { get; set; }
}

public class RemoveTagRequest
{
    public long tagId { get; set; }
}