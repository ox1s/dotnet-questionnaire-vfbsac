using Application.Abstractions.Authentication;
using Application.Teachers.Delete;
using Application.UnitTests.Database;
using Application.Users.CreateStaff;
using Application.Users.CreateTeacher;
using Application.Users.GetStaff;
using Application.Users.GetTeacherUsers;
using Domain.College.Teachers;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Users;

// Admins hand out logins to teachers (linked to a Teacher record) and to plain
// staff members ("Сотрудник"), the same way they already do for employers.
public sealed class TeacherAndStaffAccountsTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly Teacher _teacher = Teacher.Create("Иванова Анна Петровна").Value;

    public TeacherAndStaffAccountsTests()
    {
        _database.Context.Teachers.Add(_teacher);
        _database.Context.SaveChanges();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task CreateTeacherUser_CreatesTeacherLoginLinkedToTheTeacher()
    {
        Result<Guid> result = await CreateTeacherUser(_teacher.Id, "IVANOVA");

        result.IsSuccess.ShouldBeTrue(result.IsFailure ? result.Error.Description : null);
        User user = await _database.Context.Users.SingleAsync(u => u.Id == result.Value);
        user.Role.ShouldBe(UserRole.Teacher);
        user.TeacherId.ShouldBe(_teacher.Id);
        user.DisplayName.ShouldBe("Иванова Анна Петровна");
        user.PasswordHash.ShouldBe("hashed:secret-password");
    }

    [Fact]
    public async Task CreateTeacherUser_WhenTeacherAlreadyHasALogin_Fails()
    {
        await CreateTeacherUser(_teacher.Id, "IVANOVA");

        Result<Guid> second = await CreateTeacherUser(_teacher.Id, "IVANOVA2");

        second.IsFailure.ShouldBeTrue();
        second.Error.Code.ShouldBe("Users.TeacherAccountExists");
    }

    [Fact]
    public async Task CreateTeacherUser_ForUnknownTeacher_Fails()
    {
        Result<Guid> result = await CreateTeacherUser(Guid.NewGuid(), "NOBODY");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Teachers.NotFound");
    }

    [Fact]
    public async Task CreateTeacherUser_WithTakenLogin_Fails()
    {
        await CreateStaffUser("IVANOVA");

        Result<Guid> result = await CreateTeacherUser(_teacher.Id, "IVANOVA");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.UserExists");
    }

    [Fact]
    public async Task GetTeacherUsers_ListsTeacherLoginsOnly()
    {
        Result<Guid> teacherUser = await CreateTeacherUser(_teacher.Id, "IVANOVA");
        await CreateStaffUser("STAFF_ONLY");

        var handler = new GetTeacherUsersQueryHandler(_database.Context);
        Result<List<GetTeacherUsersQueryResponse>> result =
            await handler.Handle(new GetTeacherUsersQuery(), CancellationToken.None);

        GetTeacherUsersQueryResponse account = result.Value.ShouldHaveSingleItem();
        account.Id.ShouldBe(teacherUser.Value);
        account.Login.ShouldBe("IVANOVA");
        account.TeacherId.ShouldBe(_teacher.Id);
    }

    [Fact]
    public async Task CreateStaffUser_CreatesStaffLogin()
    {
        Result<Guid> result = await CreateStaffUser("PETROV");

        result.IsSuccess.ShouldBeTrue(result.IsFailure ? result.Error.Description : null);
        User user = await _database.Context.Users.SingleAsync(u => u.Id == result.Value);
        user.Role.ShouldBe(UserRole.Staff);
        user.TeacherId.ShouldBeNull();
        user.DisplayName.ShouldBe("Петров Пётр");
    }

    [Fact]
    public async Task GetStaff_ListsStaffLoginsOnly()
    {
        Result<Guid> staffUser = await CreateStaffUser("PETROV");
        await CreateTeacherUser(_teacher.Id, "IVANOVA");

        var handler = new GetStaffQueryHandler(_database.Context);
        Result<List<GetStaffQueryResponse>> result =
            await handler.Handle(new GetStaffQuery(), CancellationToken.None);

        GetStaffQueryResponse account = result.Value.ShouldHaveSingleItem();
        account.Id.ShouldBe(staffUser.Value);
        account.Login.ShouldBe("PETROV");
        account.DisplayName.ShouldBe("Петров Пётр");
    }

    [Fact]
    public async Task DeleteTeacher_AlsoRemovesTheTeachersLogin()
    {
        Result<Guid> teacherUser = await CreateTeacherUser(_teacher.Id, "IVANOVA");

        var handler = new DeleteTeacherCommandHandler(_database.Context);
        Result result = await handler.Handle(new DeleteTeacherCommand(_teacher.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue(result.IsFailure ? result.Error.Description : null);
        (await _database.Context.Users.AnyAsync(u => u.Id == teacherUser.Value)).ShouldBeFalse();
    }

    private Task<Result<Guid>> CreateTeacherUser(Guid teacherId, string login)
    {
        var handler = new CreateTeacherUserCommandHandler(_database.Context, new FakePasswordHasher());

        return handler.Handle(
            new CreateTeacherUserCommand(teacherId, login, "secret-password"),
            CancellationToken.None);
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
