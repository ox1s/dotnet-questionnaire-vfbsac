using Application.Abstractions.Messaging;
using Application.Users.Import;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Import : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/import", async (
            IFormFile file,
            ICommandHandler<ImportStudentsCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("File is empty");
            }

            using Stream stream = file.OpenReadStream();
            var command = new ImportStudentsCommand(stream);

            Result<int> result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return CustomResults.Problem(result);
            }

            return Results.Ok(new { ImportedCount = result.Value });
        })
        .WithTags("Users")
        .HasPermission(Permissions.Admin)
        .DisableAntiforgery();
    }
}
