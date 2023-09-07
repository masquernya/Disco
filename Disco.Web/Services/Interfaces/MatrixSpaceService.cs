using Disco.Web.Data;
using Disco.Web.Models.Matrix;

public interface IMatrixSpaceService
{
    Task<MatrixSpace> AddOrUpdateMatrixSpace(string invite, string name, string? description, int memberCount,
        string? avatar, string[]? admins);
    Task<IEnumerable<MatrixSpace>> GetManagedSpaces(long accountId);
    Task<IEnumerable<MatrixSpaceWithDetails>> GetAllSpaces();
    Task SetIs18Plus(long accountId, long matrixSpaceId, bool is18Plus);
}