using Application.Reports.Queries.GetAnalytics;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Reports.Queries.GetAnalytics;

public sealed class GetAnalyticsReportTests
{
    [Fact]
    public async Task BuildAsync_ShouldCountDistinctSubmissions_NotAnswers()
    {
        await using TestDbContext db = await TestDbContext.CreateAsync();

        Result<Form> formResult = Form.Create(
            "Numeric form");
        formResult.IsSuccess.ShouldBeTrue();
        Form form = formResult.Value;
        
        form.AddQuestion("Question 1", QuestionType.Number, 1).IsSuccess.ShouldBeTrue();
        form.AddQuestion("Question 2", QuestionType.Number, 2).IsSuccess.ShouldBeTrue();

        db.Context.Forms.Add(form);

        Submission firstSubmission = CreateSubmission(
            form,
            submittedAt: new DateTime(2026, 04, 20, 8, 0, 0, DateTimeKind.Utc),
            ("Question 1", 8m, null),
            ("Question 2", 6m, null));

        Submission secondSubmission = CreateSubmission(
            form,
            submittedAt: new DateTime(2026, 04, 20, 9, 0, 0, DateTimeKind.Utc),
            ("Question 1", 7m, null));

        db.Context.Submissions.AddRange(firstSubmission, secondSubmission);
        await db.Context.SaveChangesAsync();

        AnalyticsReportBuilder builder = CreateBuilder(db.Context);

        Result<AnalyticsReportResponse> result = await builder.BuildAsync(
            form.Id,
            [new AnalyticsSliceRequest("All", UtcDate(2026, 04, 20), UtcDate(2026, 04, 21))],
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        AnalyticsSliceResponse slice = result.Value.Slices.ShouldHaveSingleItem();
        slice.TotalSubmissions.ShouldBe(2);
        slice.OverallAverage.ShouldBe(6.75m); // Q1: (8+7)/2=7.5, Q2: 6/1=6, Overall: (7.5+6)/2=6.75
    }

    [Fact]
    public async Task BuildAsync_ShouldReturnZeroes_ForFormWithoutNumericQuestions()
    {
        await using TestDbContext db = await TestDbContext.CreateAsync();

        Result<Form> formResult = Form.Create("Text-only form");
        formResult.IsSuccess.ShouldBeTrue();
        Form form = formResult.Value;
        
        form.AddQuestion("Comment", QuestionType.Text, 1).IsSuccess.ShouldBeTrue();

        db.Context.Forms.Add(form);
        await db.Context.SaveChangesAsync();

        AnalyticsReportBuilder builder = CreateBuilder(db.Context);

        Result<AnalyticsReportResponse> result = await builder.BuildAsync(
            form.Id,
            [new AnalyticsSliceRequest("All", UtcDate(2026, 04, 20), UtcDate(2026, 04, 21))],
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Questions.ShouldBeEmpty();
        AnalyticsSliceResponse slice = result.Value.Slices.ShouldHaveSingleItem();
        slice.TotalSubmissions.ShouldBe(0);
        slice.OverallAverage.ShouldBe(0);
        slice.OverallStandardDeviation.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenSlicesEmpty()
    {
        await using TestDbContext db = await TestDbContext.CreateAsync();
        AnalyticsReportBuilder builder = CreateBuilder(db.Context);
        GetAnalyticsReportQueryHandler handler = new(builder);

        Result<AnalyticsReportResponse> result = await handler.Handle(
            new GetAnalyticsReportQuery(Guid.NewGuid(), []),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Analytics.SlicesRequired");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFormMissing()
    {
        await using TestDbContext db = await TestDbContext.CreateAsync();
        AnalyticsReportBuilder builder = CreateBuilder(db.Context);
        GetAnalyticsReportQueryHandler handler = new(builder);

        Result<AnalyticsReportResponse> result = await handler.Handle(
            new GetAnalyticsReportQuery(
                Guid.NewGuid(),
                [new AnalyticsSliceRequest("All", UtcDate(2026, 04, 20), UtcDate(2026, 04, 21))]),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Forms.NotFound");
    }

    private static AnalyticsReportBuilder CreateBuilder(ApplicationDbContext context)
    {
        SubmissionQueryBuilder queryBuilder = new(context);
        QuestionAggregator aggregator = new();
        MetricCalculator calculator = new();
        ResponseMapper mapper = new();
        
        return new AnalyticsReportBuilder(context, queryBuilder, aggregator, calculator, mapper);
    }

    private static Submission CreateSubmission(
        Form form,
        DateTime submittedAt,
        params (string QuestionText, decimal NumericValue, decimal? Weight)[] answers)
    {
        Result<Submission> submissionResult = Submission.Create(
            form.Id, 
            Guid.NewGuid().ToString(), 
            Guid.NewGuid(), 
            submittedAt);
        
        submissionResult.IsSuccess.ShouldBeTrue();
        Submission submission = submissionResult.Value;

        foreach ((string QuestionText, decimal NumericValue, decimal? Weight) answer in answers)
        {
            Question question = form.Questions.Single(q => q.Text == answer.QuestionText);
            Result<Answer> answerResult = submission.AddAnswer(
                question.Id, 
                numericValue: answer.NumericValue, 
                weight: answer.Weight);
            answerResult.IsSuccess.ShouldBeTrue();
        }

        return submission;
    }

    private sealed class TestDbContext : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDbContext(SqliteConnection connection, ApplicationDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public static async Task<TestDbContext> CreateAsync()
        {
            SqliteConnection connection = new("Data Source=:memory:");
            
            try
            {
                await connection.OpenAsync();

                DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                ApplicationDbContext context = new(options);
                await context.Database.EnsureCreatedAsync();

                return new TestDbContext(connection, context);
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private static DateTime UtcDate(int year, int month, int day)
    {
        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}
