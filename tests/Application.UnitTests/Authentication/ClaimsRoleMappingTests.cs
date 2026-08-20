using System.Security.Claims;
using Domain.User;
using Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Application.UnitTests.Authentication;

// Regression coverage for a bug where GetRole() read the raw "role" claim key,
// but ASP.NET Core's JwtBearerHandler remaps it to ClaimTypes.Role by default
// (MapInboundClaims = true, never overridden in this project) before any
// handler sees the validated principal - so the claim was never found and
// GetRole() always threw. Nothing else in this repo issues and validates a
// real token end to end, so this is the only guard against that class of bug.
public class ClaimsRoleMappingTests
{
    private const string Secret = "unit-test-signing-key-at-least-256-bits-long!!";
    private const string Issuer = "unit-test-issuer";
    private const string Audience = "unit-test-audience";

    private static readonly TokenValidationParameters ValidationParameters = new()
    {
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Secret)),
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        ClockSkew = TimeSpan.Zero
    };

    [Fact]
    public async Task GetRole_OnAPrincipalValidatedTheWayWeb_ApiValidatesIt_ReturnsTheTokenIssuedRole()
    {
        User employer = User.CreateEmployer(
            Login.Create("EMPLOYER_TEST").Value,
            "Test Employer",
            "Test Org",
            passwordHash: "hash").Value;

        string token = CreateToken(employer);

        ClaimsPrincipal principal = await ValidateAsync(token);

        principal.GetRole().ShouldBe(UserRole.Employer);
        principal.GetUserId().ShouldBe(employer.Id);
    }

    [Fact]
    public async Task GetRole_WithoutClaimsMappingApplied_StillFindsTheRoleViaTheRawClaimFallback()
    {
        // Guards the `?? principal?.FindFirstValue("role")` fallback: if MapInboundClaims
        // is ever turned off, the raw "role" claim key must still resolve correctly.
        var identity = new ClaimsIdentity([new Claim("role", UserRole.DeputyHead.ToString())]);
        var principal = new ClaimsPrincipal(identity);

        principal.GetRole().ShouldBe(UserRole.DeputyHead);
    }

    private static string CreateToken(User user)
    {
        // Hand-rolled rather than ConfigurationBuilder+AddInMemoryCollection: those live in
        // Microsoft.Extensions.Configuration(.Memory), which isn't referenced by any project
        // here (TokenProvider.cs only needs Microsoft.Extensions.Configuration.Abstractions,
        // already transitive via the Infrastructure project reference) and isn't worth adding
        // just for this test.
        IConfiguration configuration = new FakeConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = Secret,
            ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience,
            ["Jwt:ExpirationInMinutes"] = "60"
        });

        var tokenProvider = new TokenProvider(configuration);

        return tokenProvider.Create(user);
    }

    private static async Task<ClaimsPrincipal> ValidateAsync(string token)
    {
        // Matches JwtBearerOptions.MapInboundClaims (default true, never overridden in
        // Infrastructure/DependencyInjection.cs's AddJwtBearer call) - JwtBearerHandler
        // propagates that setting onto its internal JsonWebTokenHandler, so a bare `new
        // JsonWebTokenHandler()` would NOT reproduce the real remapping and would let a
        // regression here pass silently.
        var handler = new JsonWebTokenHandler { MapInboundClaims = true };
        TokenValidationResult result = await handler.ValidateTokenAsync(token, ValidationParameters);

        result.IsValid.ShouldBeTrue(result.Exception?.ToString());

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }

    private sealed class FakeConfiguration(Dictionary<string, string?> values) : IConfiguration
    {
        public string? this[string key]
        {
            get => values.GetValueOrDefault(key);
            set => values[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new FakeChangeToken();
        public IConfigurationSection GetSection(string key) => new FakeConfigurationSection(key, this[key]);
    }

    private sealed class FakeConfigurationSection(string sectionKey, string? sectionValue) : IConfigurationSection
    {
        public string Key { get; } = sectionKey;
        public string Path { get; } = sectionKey;
        public string? Value { get => sectionValue; set => throw new NotSupportedException(); }

        public string? this[string key]
        {
            get => null;
            set => throw new NotSupportedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new FakeChangeToken();
        public IConfigurationSection GetSection(string key) => new FakeConfigurationSection(key, null);
    }

    private sealed class FakeChangeToken : IChangeToken
    {
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NullDisposable.Instance;

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
