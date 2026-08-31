using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Domain.User;
using Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Database;

// Regression coverage for the "Оценка удовлетворенности персонала работой в колледже"
// request: demo/seed data must include a Staff-targeted form that requires
// FilterField.EmployeeCategory, with seeded submissions covering all four
// personnel categories (АУП/ППС/УВП/ПОП) used by the college's actual paper form.
public sealed class DemoDataGeneratorTests : IDisposable
{
    private static readonly string[] ExpectedEmployeeCategories = ["АУП", "ППС", "УВП", "ПОП"];

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;

    public DemoDataGeneratorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task SeedAsync_CreatesStaffSatisfactionForm_RequiringEmployeeCategory()
    {
        var generator = new DemoDataGenerator();

        await generator.SeedAsync(_context, "demo-password-hash");

        Form? staffForm = await _context.Forms.FirstOrDefaultAsync(f => f.TargetRole == UserRole.Staff);

        staffForm.ShouldNotBeNull();
        staffForm.RequiredFilters.ShouldNotBeNull();
        staffForm.RequiredFilters.ShouldContain(FilterField.EmployeeCategory);
    }

    [Fact]
    public async Task SeedAsync_SeedsStaffFormSubmissions_CoveringAllFourEmployeeCategories()
    {
        var generator = new DemoDataGenerator();

        await generator.SeedAsync(_context, "demo-password-hash");

        Form staffForm = await _context.Forms.FirstAsync(f => f.TargetRole == UserRole.Staff);

        List<string?> seededCategories = await _context.Submissions
            .Where(s => s.FormId == staffForm.Id)
            .Select(s => s.Context.EmployeeCategory)
            .ToListAsync();

        foreach (string expectedCategory in ExpectedEmployeeCategories)
        {
            seededCategories.ShouldContain(expectedCategory);
        }
    }
}
