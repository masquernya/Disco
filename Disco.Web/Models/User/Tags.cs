namespace Disco.Web.Models.User;

public class AddTagRequest
{
    public string tag { get; set; }
}

public class RemoveTagRequest
{
    public long tagId { get; set; }
}

public class TopTagWithCount
{
    public string tag { get; set; }
    public long count { get; set; }
}

public class ApproveTopTagRequest
{
    public string tag { get; set; }
    public string displayTag { get; set; }
}

public class DeleteTopTagRequest
{
    public string tag { get; set; }
}