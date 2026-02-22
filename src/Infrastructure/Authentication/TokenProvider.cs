using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Domain.UserAggregate;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider(IConfiguration configuration) : ITokenProvider
{
    public string Create(User user)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("role", user.Role.ToString())
        };

        IEnumerable<string> permissions = GetPermissionsForRole(user.Role);
        foreach (string permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }
    private static IEnumerable<string> GetPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => [Permissions.Admin, Permissions.UsersAccess, Permissions.DictionariesWrite, Permissions.ReportsView],
            UserRole.StudentGroup => [Permissions.SubmitForms],
            UserRole.Staff or UserRole.DeputyHead => [Permissions.ReportsView],
            _ => []
        };
    }
}
