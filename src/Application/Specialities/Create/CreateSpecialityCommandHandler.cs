using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Specialities;
using SharedKernel;

namespace Application.Specialities.Create;

internal sealed class CreateSpecialityCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateSpecialityCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSpecialityCommand command, CancellationToken cancellationToken)
    {
        Result<Speciality> specialityResult = Speciality.Create(command.Name);
        if (specialityResult.IsFailure)
        {
            return Result.Failure<Guid>(specialityResult.Error);
        }

        context.Specialities.Add(specialityResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return specialityResult.Value.Id;
    }
}
