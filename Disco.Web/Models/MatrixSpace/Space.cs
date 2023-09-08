using Disco.Web.Data;

namespace Disco.Web.Models.Matrix;

public class MatrixSpaceWithDetails
{
    public MatrixSpace space { get; set; } = new();
    public IEnumerable<MatrixSpaceTag> tags { get; set; } = new List<MatrixSpaceTag>();
    public string? imageUrl { get; set; }
}

public class SetIsMatrixSpace18PlusRequest
{
    public long matrixSpaceId { get; set; }
    public bool is18Plus { get; set; }
}

public class AddTagRequest
{
    public long matrixSpaceId { get; set; }
    public string tag { get; set; } = null!;
}

public class DeleteTagRequest
{
    public long matrixSpaceId { get; set; }
    public long tagId { get; set; }
}