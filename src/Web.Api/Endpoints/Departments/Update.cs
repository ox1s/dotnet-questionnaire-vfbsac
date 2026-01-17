using Application.Abstractions.Messaging;
using Application.Departments.Update;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Departments;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("departments/{departmentId:guid}", async (
            Guid departmentId,
            UpdateDepartmentCommand command, 
            ICommandHandler<UpdateDepartmentCommand> handler,
            CancellationToken cancellationToken) =>
        {
            if (command.DepartmentId != departmentId)
            {
                 command = new UpdateDepartmentCommand(departmentId, command.Name);
            }

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("Dictionaries")
        .RequireAuthorization();
    }
}
