using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Authentication.Common;
using Questionnaire.Application.Common;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Users;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Authentication.Queries.Login;

internal sealed class LoginQueryHandler : IQueryHandler<LoginQuery, AuthenticationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginQueryHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthenticationResponse>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role) 
            .FirstOrDefaultAsync(u => u.Login == query.Login, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(query.Password, user.PasswordHash))
        {
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.InvalidCredentials);
        }

        var userTokenData = new UserTokenData(
            user.Id,
            user.Login,
            user.UserRoles.Select(ur => ur.Role.Name));
        string token = _jwtTokenGenerator.GenerateToken(userTokenData);

        var response = new AuthenticationResponse(user.Id, user.Login, token);

        return Result.Success(response);
    }
}