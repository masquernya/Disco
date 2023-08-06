using Disco.Web.Data;

namespace Disco.Web.Models.User;

public class ReportUserRequest
{
    public long accountId { get; set; }
    public ReportField field { get; set; }
    public ReportReason reason { get; set; }
}

public class BanAccountRequest
{
    public long accountId { get; set; }
    public string reason { get; set; }
}

public class ToggleImageStatusRequest
{
    public long imageId { get; set; }
}