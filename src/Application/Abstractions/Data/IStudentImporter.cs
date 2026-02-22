using SharedKernel;

namespace Application.Abstractions.Data;

public interface IStudentImporter
{
    Task<Result<int>> ImportAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
