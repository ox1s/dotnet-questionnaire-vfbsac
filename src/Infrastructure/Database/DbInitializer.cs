using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Domain.College.DepartmentAggregate;
using Domain.College.DisciplineAggregate;
using Domain.College.SpecialityAggregate;
using Domain.College.SpecializationAggregate;
using Domain.College.TeacherAggregate;
using Domain.Questionnaires.FormAggregate;
using Domain.Questionnaires.SubmissionAggregate;
using Domain.UserAggregate;
using Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database;

public class DbInitializer(
    IServiceProvider serviceProvider,
    ILogger<DbInitializer> logger)
{
    public async Task InitializeAsync()
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IPasswordHasher passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        if (await context.Forms.AnyAsync())
        {
            logger.LogInformation("Database already initialized.");
            return;
        }

        logger.LogInformation("Seeding database with legacy data...");

        // ================= 1. REFERENCE DATA =================

        // Departments
        Dictionary<int, Department> depts = [];
        Dictionary<int, string> deptNames = new()
        {
            {1, "ПОСТ"}, {2, "ИКТ"}, {3, "ТКС"}, {4, "ОТПС"}, {5, "РИТ"},
            {6, "ГУМ"}, {7, "ЗОЖ"}, {8, "ФИМОИ"}, {9, "ЦЭ"}
        };

        foreach (KeyValuePair<int, string> kvp in deptNames)
        {
            Department d = Department.Create(kvp.Value).Value;
            context.Departments.Add(d);
            depts.Add(kvp.Key, d);
        }
        await context.SaveChangesAsync();

        // Specialities
        Dictionary<int, Speciality> specs = [];
        Dictionary<int, string> specNames = new()
        {
            {1, "Сети телекоммуникаций"},
            {2, "Почтовая связь"},
            {3, "Системы радиосвязи, радиовещания и телевидения"}
        };
        foreach (KeyValuePair<int, string> kvp in specNames)
        {
            Speciality s = Speciality.Create(kvp.Value).Value;
            context.Specialities.Add(s);
            specs.Add(kvp.Key, s);
        }
        await context.SaveChangesAsync();

        // Specializations
        Dictionary<int, Specialization> specializations = [];
        List<(int Id, string Name, int SpecId)> specData =
        [
            (2, "ПО сетей телеком", 1), (4, "Тех.экспл. сетей телеком", 1),
            (6, "Экспл. инфо-тех сетей", 2), (8, "Радиосистемы охраны", 3),
            (10, "Орг. торговли почтой", 2)
        ];
        foreach ((int Id, string Name, int SpecId) item in specData)
        {
            Specialization s = Specialization.Create(item.Name, specs[item.SpecId].Id).Value;
            context.Specializations.Add(s);
            specializations.Add(item.Id, s);
        }
        await context.SaveChangesAsync();

        // Disciplines (Selected sample)
        Dictionary<int, Discipline> disciplines = [];
        List<(int Id, string Name, int DeptId)> discData =
        [
            (5, "КПиЯП", 1), (7, "ИТ", 2), (8, "ООП", 1), (29, "Теор.алг", 1),
            (33, "Тех.комм и ОК", 6), (50, "Инф", 2)
        ];
        foreach ((int Id, string Name, int DeptId) item in discData)
        {
            if (!depts.TryGetValue(item.DeptId, out Department? dept))
            {

                continue;
            }

            // Передаем dept.Id (он точно Guid, не null)
            Discipline d = Discipline.Create(item.Name, dept.Id).Value;

            context.Disciplines.Add(d);
            disciplines.Add(item.Id, d);
        }
        await context.SaveChangesAsync();

        // ================= 2. USERS =================

        string defaultPass = passwordHasher.Hash("12345678");

        // Admin
        User admin = User.CreateAdmin(Login.Create("ADMIN").Value, defaultPass).Value;
        context.Users.Add(admin);

        // Student Groups (Logins from description)
        string[] groups = ["ПО111", "ТС111", "РТ111", "ПС111"];
        List<User> groupUsers = [];

        foreach (string gName in groups)
        {
            User u = User.CreateGroupUser(GroupName.Create(gName).Value, 1, defaultPass).Value;
            context.Users.Add(u);
            groupUsers.Add(u);
        }
        await context.SaveChangesAsync();

        // ================= 3. FORMS =================

        Dictionary<int, Form> forms = [];

        // Form 1: Удовл. преподаванием (Discipline)
        Form f1 = Form.Create("Оценка удовлетворённости обучающихся преподаванием учебных дисциплин",
            [FilterField.Discipline]).Value;

        f1.AddQuestion("Содержание образовательной программы", QuestionType.WeightedRating, 1);
        f1.AddQuestion("Лекционные занятия (методы)", QuestionType.WeightedRating, 2);
        f1.AddQuestion("Практические и лабораторные занятия", QuestionType.WeightedRating, 3);
        f1.AddQuestion("Информационное обеспечение (доступ к ПК, библиотека)", QuestionType.WeightedRating, 4);
        f1.AddQuestion("Материально-техническое обеспечение (аудитории)", QuestionType.WeightedRating, 5);
        f1.AddQuestion("Ваши предложения по улучшению", QuestionType.Text, 6);

        context.Forms.Add(f1);
        forms.Add(1, f1);

        // Form 7: Рук. тех. практики (Hirer)
        Form f7 = Form.Create("Оценка руководителей производственной практики (Предприятие)",
            [FilterField.Speciality]).Value;

        f7.AddQuestion("Актуальность теоретических знаний", QuestionType.Number, 1);
        f7.AddQuestion("Качество практических навыков", QuestionType.Number, 2);
        f7.AddQuestion("Дисциплина и исполнительность", QuestionType.Number, 3);
        f7.AddQuestion("Затруднения при работе (выбор)", QuestionType.MultipleChoice, 4);
        f7.AddQuestion("Предложения", QuestionType.Text, 5);

        context.Forms.Add(f7);
        forms.Add(7, f7);

        await context.SaveChangesAsync();

        // ================= 4. SUBMISSIONS (DATA) =================

        // User studentUser = groupUsers[0]; // Use first group for seeding

        // // --- Seeding Form 1 (Discipline Satisfaction) ---
        // // Data: `1-3` (23-12-16): Marks[High] Weights[10s] | Disc:33 (Тех.комм), Edu:ДФПО
        // Discipline disc33 = disciplines[33];

        // for (int i = 0; i < 3; i++)
        // {
        //     CreateSubmission(context, f1, studentUser.Id, new DateTime(2023, 12, 16, 0, 0, 0, DateTimeKind.Utc),
        //         ctx => new SubmissionContext(DisciplineId: disc33.Id, EducationForm: "ДФПО"),
        //         (q) => q.Type == QuestionType.WeightedRating ? (10, 10) : (null, null));
        // }

        // // Data: `36` (23-12-18): Mixed marks, Angry Rec
        // Submission sub36 = CreateSubmission(context, f1, studentUser.Id, new DateTime(2023, 12, 18, 0, 0, 0, DateTimeKind.Utc),
        //      ctx => new SubmissionContext(DisciplineId: disc33.Id, EducationForm: "ДФПО"),
        //      (q) => q.Order == 1 ? (10, 10) : (8, 10));

        // Question qText = f1.Questions.First(q => q.Type == QuestionType.Text);
        // sub36.AddAnswer(qText.Id, value: "Пусть умники из министерства сами отсидят 30 часов... напишут ОКР...");

        // // Data: `75` (23-12-26): Disc:7 (ИТ), Rec: "Фотошоп глюкает"
        // Discipline disc7 = disciplines[7];
        // Submission sub75 = CreateSubmission(context, f1, studentUser.Id, new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Utc),
        //     ctx => new SubmissionContext(DisciplineId: disc7.Id, EducationForm: "ДФПО"),
        //     (q) => (8, 10));
        // sub75.AddAnswer(qText.Id, value: "Фотошоп глюкает, надо обновить видеокарту...");

        // // Data: `128`, `138`, `144` (25-06-24): Disc:8 (ООП), Bad Internet
        // Discipline disc8 = disciplines[8];
        // Submission badInternetSub = CreateSubmission(context, f1, studentUser.Id, new DateTime(2024, 06, 25, 0, 0, 0, DateTimeKind.Utc),
        //     ctx => new SubmissionContext(DisciplineId: disc8.Id, EducationForm: "ДФПО"),
        //     (q) => (1, 10));
        // badInternetSub.AddAnswer(qText.Id, value: "ИНТЕРНЕТ СДЕЛАТЬ ЛУЧШЕ");


        // // --- Seeding Form 7 (Hirer Feedback) ---
        // // Data: `99` (25-01-28): M[6-9] | Login: "Мядельский УЭС"
        // var f7Qs = f7.Questions.Where(q => q.Type == QuestionType.Number).ToList();
        // Submission sub99 = Submission.Create(f7.Id, admin.Id,
        //     organizationName: "Мядельский УЭС").Value;

        // typeof(Submission).GetProperty(nameof(Submission.SubmittedAt))?
        //     .SetValue(sub99, new DateTime(2025, 01, 28, 0, 0, 0, DateTimeKind.Utc));

        // foreach (Question q in f7Qs)
        // {
        //     sub99.AddAnswer(q.Id, numericValue: RandomNumberGenerator.GetInt32(6, 10));
        // }
        // context.Submissions.Add(sub99);

        // await context.SaveChangesAsync();
        logger.LogInformation("Seeding completed.");
    }

    // private Submission CreateSubmission(
    //     ApplicationDbContext context,
    //     Form form,
    //     Guid userId,
    //     DateTime date,
    //     Func<SubmissionContext, SubmissionContext> contextBuilder,
    //     Func<Question, (decimal? Val, decimal? Weight)> valueProvider)
    // {
    //     SubmissionContext subCtx = contextBuilder(new SubmissionContext());

    //     Submission submission = Submission.Create(
    //         form.Id, userId,
    //         subCtx.DisciplineId, subCtx.TeacherId, subCtx.DepartmentId,
    //         subCtx.SpecialityId, subCtx.SpecializationId, subCtx.OrganizationName).Value;

    //     typeof(Submission).GetProperty(nameof(Submission.SubmittedAt))?
    //         .SetValue(submission, date);

    //     if (subCtx.EducationForm != null)
    //     {
    //         // Logic to update context if needed
    //     }

    //     foreach (Question q in form.Questions)
    //     {
    //         if (q.Type == QuestionType.Text)
    //         {
    //             continue;
    //         }

    //         (decimal? val, decimal? weight) = valueProvider(q);
    //         if (val.HasValue)
    //         {
    //             submission.AddAnswer(q.Id, numericValue: val, weight: weight);
    //         }
    //     }

    //     context.Submissions.Add(submission);
    //     return submission;
    // }
}
