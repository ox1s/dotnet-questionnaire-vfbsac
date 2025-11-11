using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Authentication.Common;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthenticationResult>>
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

    public async Task<ErrorOr<AuthenticationResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Login == command.Login, cancellationToken))
        {
            return AuthenticationErrors.DuplicateLogin;
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == command.Role, cancellationToken);
        if (role is null)
        {
            role = new Role { Name = command.Role };
            _context.Roles.Add(role);
        }

        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var user = new User
        {
            Login = command.Login,
            PasswordHash = passwordHash
        };

        user.UserRoles.Add(new UserRole { Role = role });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        user.UserRoles = new List<UserRole> { new() { Role = role } };
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthenticationResult(user, token);
    }
}