using Disco.Web.Data;

namespace Disco.Web.Models.Matrix;

public class MatrixSpaceWithDetails
{
    public MatrixSpace space { get; set; } = new();
    public IEnumerable<string> tags { get; set; } = new List<string>();
    public string? imageUrl { get; set; }
}

public class SetIsMatrixSpace18PlusRequest
{
    public long matrixSpaceId { get; set; }
    public bool is18Plus { get; set; }
}