using Disco.Web.Data;
using Disco.Web.Models.Matrix;

public interface IMatrixSpaceService
{
    Task<MatrixSpace> AddOrUpdateMatrixSpace(string invite, string name, string? description, int memberCount,
        string? avatar, bool is18Plus, string[]? admins);
    Task<IEnumerable<MatrixSpace>> GetManagedSpaces(long accountId);
    Task<IEnumerable<MatrixSpaceWithDetails>> GetAllSpaces();
    Task SetIs18Plus(long accountId, long matrixSpaceId, bool is18Plus);
    Task<MatrixSpaceTag> AddTag(long accountId, long matrixSpaceId, string tag);
    Task DeleteTag(long accountId, long matrixSpaceId, long tagId);
    Task BanSpace(long matrixSpaceId);
}