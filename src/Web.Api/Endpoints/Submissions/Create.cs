using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Submissions.Create;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Submissions;

internal sealed class Create : IEndpoint
{
    public sealed record CreateSubmissionRequest(
        Guid FormId,
        List<AnswerRequest> Answers,
        Guid? DisciplineId = null,
        Guid? TeacherId = null,
        Guid? DepartmentId = null,
        Guid? SpecialityId = null,
        Guid? SpecializationId = null,
        string? OrganizationName = null,
        string? EducationForm = null,
        string? EmployeeCategory = null,
        string? Position = null);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("submissions", async (
            CreateSubmissionRequest request,
            IUserContext userContext,
            ICommandHandler<CreateSubmissionCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSubmissionCommand(
                request.FormId,
                userContext.UserId,
                request.Answers,
                request.DisciplineId,
                request.TeacherId,
                request.DepartmentId,
                request.SpecialityId,
                request.SpecializationId,
                request.OrganizationName,
                request.EducationForm,
                request.EmployeeCategory,
                request.Position
            );

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("Submissions")
        .HasPermission(Permissions.SubmitForms);
    }
}
