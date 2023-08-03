using System.Net;

namespace Disco.Web.Exceptions.Matrix;

public class InvalidMatrixUsernameException : BaseWebException
{
    public InvalidMatrixUsernameException() : base(HttpStatusCode.BadRequest, "Invalid matrix username")
    {
        
    }
}

public class MatrixUserNotFoundException : BaseWebException
{
    public MatrixUserNotFoundException() : base(HttpStatusCode.NotFound, "Matrix user not found")
    {
        
    }
}