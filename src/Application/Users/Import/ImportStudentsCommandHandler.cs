using System.Globalization;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Import;

internal sealed class ImportStudentsCommandHandler(
    IStudentImporter studentImporter)
    : ICommandHandler<ImportStudentsCommand, int>
{
    public async Task<Result<int>> Handle(ImportStudentsCommand command, CancellationToken cancellationToken)
    {
        return await studentImporter.ImportAsync(command.FileStream, cancellationToken);
    }
}
