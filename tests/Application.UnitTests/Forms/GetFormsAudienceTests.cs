using Application.Forms.GetList;
using Application.UnitTests.Database;
using Domain.Questionnaires.Forms;
using Domain.User;
using SharedKernel;

namespace Application.UnitTests.Forms;

// Teachers are also college staff (personnel category ППС), so a signed-in teacher
// must see Staff-targeted questionnaires as well as Teacher-targeted and general ones.
public sealed class GetFormsAudienceTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public GetFormsAudienceTests()
    {
        _database.Context.Forms.AddRange(
            Form.Create("General").Value,
            Form.Create("Students", targetRole: UserRole.StudentGroup).Value,
            Form.Create("Staff", targetRole: UserRole.Staff).Value,
            Form.Create("Teachers", targetRole: UserRole.Teacher).Value,
            Form.Create("Employers", targetRole: UserRole.Employer).Value);
        _database.Context.SaveChanges();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Teacher_SeesGeneralStaffAndTeacherForms()
    {
        string[] titles = await GetTitlesFor(UserRole.Teacher);

        titles.ShouldBe(["General", "Staff", "Teachers"], ignoreOrder: true);
    }

    [Fact]
    public async Task Staff_DoesNotSeeTeacherForms()
    {
        string[] titles = await GetTitlesFor(UserRole.Staff);

        titles.ShouldBe(["General", "Staff"], ignoreOrder: true);
    }

    private async Task<string[]> GetTitlesFor(UserRole role)
    {
        var handler = new GetFormsQueryHandler(_database.Context);

        Result<List<GetFormsQueryResponse>> result = await handler.Handle(
            new GetFormsQuery(IsActive: null, CallerRole: role),
            CancellationToken.None);

        return result.Value.Select(f => f.Title).ToArray();
    }
}
