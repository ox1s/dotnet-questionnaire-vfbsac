using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse>
{
    public async Task<Result<GetUserByIdQueryResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        GetUserByIdQueryResponse? user = await context.Users
            .Where(u => u.Id == query.UserId)
            .Select(u => new GetUserByIdQueryResponse
            {
                Id = u.Id,
                Login = u.Login.Value,
                DisplayName = u.DisplayName
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<GetUserByIdQueryResponse>(UserErrors.NotFound(query.UserId));
        }

        return user;
    }
}
