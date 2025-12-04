using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Authentication.Common;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Contracts.Authentication;
using Questionnaire.Domain.Entities;
using Questionnaire.Domain.Users;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Authentication.Commands.Register;

internal sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthenticationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthenticationResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Login == command.Login, cancellationToken))
        {
            return Result.Failure<AuthenticationResponse>(AuthenticationErrors.DuplicateLogin);
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == command.Role, cancellationToken);
        if (role is null)
        {
            role = new Role { Name = command.Role };
            _context.Roles.Add(role);
        }

        string passwordHash = _passwordHasher.HashPassword(command.Password);

        var user = new User
        {
            Login = command.Login,
            PasswordHash = passwordHash
        };

        user.UserRoles.Add(new UserRole { Role = role });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        user.UserRoles = new List<UserRole> { new() { Role = role } };
        string token = _jwtTokenGenerator.GenerateToken(user);

        var response = new AuthenticationResponse(user.Id, user.Login, token);

        return Result.Success(response);
    }
}