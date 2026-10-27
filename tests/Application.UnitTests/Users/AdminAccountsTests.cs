using Application.Abstractions.Authentication;
using Application.UnitTests.Database;
using Application.Users.CreateAdmin;
using Application.Users.CreateStaff;
using Application.Users.GetAdmins;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Users;

// Admins add other admins from the settings; an admin's login is also their
// display name, so renaming goes through the generic user update.
public sealed class AdminAccountsTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task CreateAdmin_CreatesAdminWhoseNameIsTheLogin()
    {
        Result<Guid> result = await CreateAdmin("DEPUTY");

        result.IsSuccess.ShouldBeTrue(result.IsFailure ? result.Error.Description : null);
        User user = await _database.Context.Users.SingleAsync(u => u.Id == result.Value);
        user.Role.ShouldBe(UserRole.Admin);
        user.Login.Value.ShouldBe("DEPUTY");
        user.DisplayName.ShouldBe("DEPUTY");
        user.PasswordHash.ShouldBe("hashed:secret-password");
    }

    [Fact]
    public async Task CreateAdmin_WithTakenLogin_Fails()
    {
        await CreateStaffUser("DEPUTY");

        Result<Guid> result = await CreateAdmin("DEPUTY");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.UserExists");
    }

    [Fact]
    public async Task GetAdmins_ListsAdminsOnlyOrderedByLogin()
    {
        Result<Guid> zeta = await CreateAdmin("ZETA");
        Result<Guid> alpha = await CreateAdmin("ALPHA");
        await CreateStaffUser("STAFF_ONLY");

        var handler = new GetAdminsQueryHandler(_database.Context);
        Result<List<GetAdminsQueryResponse>> result = await handler.Handle(new GetAdminsQuery(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(a => a.Id).ShouldBe([alpha.Value, zeta.Value]);
        result.Value.Select(a => a.Login).ShouldBe(["ALPHA", "ZETA"]);
    }

    private Task<Result<Guid>> CreateAdmin(string login)
    {
        var handler = new CreateAdminUserCommandHandler(_database.Context, new FakePasswordHasher());

        return handler.Handle(new CreateAdminUserCommand(login, "secret-password"), CancellationToken.None);
    }

    private Task<Result<Guid>> CreateStaffUser(string login)
    {
        var handler = new CreateStaffUserCommandHandler(_database.Context, new FakePasswordHasher());

        return handler.Handle(
            new CreateStaffUserCommand(login, "Петров Пётр", "secret-password", DepartmentId: null),
            CancellationToken.None);
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string password, string passwordHash) => Hash(password) == passwordHash;
    }
}
