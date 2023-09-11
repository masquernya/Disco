using Disco.Web.Data;

namespace Disco.Web.Models.User;

public class UserUploadedImageReview
{
    public UserUploadedImage image { get; set; }
    /// <summary>
    /// Matrix spaces using this image.
    /// </summary>
    public IEnumerable<MatrixSpace> spaces { get; set; }
    /// <summary>
    /// Accounts using this image.
    /// </summary>
    public IEnumerable<Account> accounts { get; set; }
}