namespace Disco.Web.Models;

public static class MatrixHelpers
{
    public static string GetPictureUrlFromMatrixUrl(string mxcUrl)
    {
        // Returns data like "mxc://matrix.org/1234"
        var parsed = new Uri(mxcUrl);
        if (parsed.Scheme != "mxc")
            throw new Exception("Invalid avatar url");
        var afterScheme = parsed.Host + parsed.PathAndQuery;
        var getImageUrl =
            "https://matrix.org/_matrix/media/r0/thumbnail/"+afterScheme+"?width=256&height=256";
        var parsedGetImageUrl = new Uri(getImageUrl);
        if (parsedGetImageUrl.Host != "matrix.org" || parsedGetImageUrl.Scheme != "https")
            throw new Exception("Unsafe url");
        return parsedGetImageUrl.ToString();
    }
}