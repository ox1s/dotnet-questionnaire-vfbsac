using System.Reflection;
using Application.Abstractions.Authentication;
using Domain.College.DepartmentAggregate;
using Domain.College.DisciplineAggregate;
using Domain.College.SpecialityAggregate;
using Domain.College.SpecializationAggregate;
using Domain.Questionnaires.FormAggregate;
using Domain.UserAggregate;
using Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;

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

        logger.LogInformation("Seeding database...");

        // ================= 1. REFERENCE DATA =================

        // Departments
        Dictionary<int, Department> depts = [];
        var deptData = new Dictionary<int, (string Name, Guid Id)>
        {
            {1, ("ПОСТ", Guid.Parse("11111111-1111-1111-1111-111111111111"))},
            {2, ("ИКТ", Guid.Parse("22222222-2222-2222-2222-222222222222"))},
            {3, ("ТКС", Guid.NewGuid())},
            {4, ("ОТПС", Guid.NewGuid())},
            {5, ("РИТ", Guid.NewGuid())},
            {6, ("ГУМ", Guid.NewGuid())},
            {7, ("ЗОЖ", Guid.NewGuid())},
            {8, ("ФИМОИ", Guid.NewGuid())},
            {9, ("ЦЭ", Guid.NewGuid())}
        };

        foreach (KeyValuePair<int, (string Name, Guid Id)> kvp in deptData)
        {
            Department d = Department.Create(kvp.Value.Name).Value;
            SetId(d, kvp.Value.Id); // Фиксируем ID
            context.Departments.Add(d);
            depts.Add(kvp.Key, d);
        }
        await context.SaveChangesAsync();

        // Specialities
        Dictionary<int, Speciality> specs = [];
        var specDataList = new Dictionary<int, string>
        {
            {1, "Сети телекоммуникаций"},
            {2, "Почтовая связь"},
            {3, "Системы радиосвязи, радиовещания и телевидения"}
        };
        foreach (KeyValuePair<int, string> kvp in specDataList)
        {
            Speciality s = Speciality.Create(kvp.Value).Value;
            context.Specialities.Add(s);
            specs.Add(kvp.Key, s);
        }
        await context.SaveChangesAsync();

        // Specializations
        Dictionary<int, Specialization> specializations = [];
        var speczData = new List<(int Id, string Name, int SpecId)>
        {
            (2, "ПО сетей телеком", 1), (4, "Тех.экспл. сетей телеком", 1),
            (6, "Экспл. инфо-тех сетей", 2), (8, "Радиосистемы охраны", 3),
            (10, "Орг. торговли почтой", 2)
        };
        foreach ((int Id, string Name, int SpecId) item in speczData)
        {
            Specialization s = Specialization.Create(item.Name, specs[item.SpecId].Id).Value;
            context.Specializations.Add(s);
            specializations.Add(item.Id, s);
        }
        await context.SaveChangesAsync();

        // Disciplines
        Dictionary<int, Discipline> disciplines = [];
        var discData = new List<(int Id, string Name, int DeptId)>
        {
            (5, "КПиЯП", 1), (7, "ИТ", 2), (8, "ООП", 1), (29, "Теор.алг", 1),
            (33, "Тех.комм и ОК", 6), (50, "Инф", 2)
        };
        foreach ((int Id, string Name, int DeptId) item in discData)
        {
            if (depts.TryGetValue(item.DeptId, out Department? dept))
            {
                Discipline d = Discipline.Create(item.Name, dept.Id).Value;
                context.Disciplines.Add(d);
                disciplines.Add(item.Id, d);
            }
        }
        await context.SaveChangesAsync();

        // ================= 2. USERS =================

        string defaultPass = passwordHasher.Hash("12345678");

        // 2.1 Admin
        User admin = User.CreateAdmin(Login.Create("ADMIN").Value, defaultPass).Value;
        context.Users.Add(admin);

        // 2.2 Student Groups
        string[] groups = ["ПО111", "ТС111", "РТ111", "ПС111"];
        foreach (string gName in groups)
        {
            User u = User.CreateGroupUser(GroupName.Create(gName).Value, Guid.NewGuid(), defaultPass).Value;
            context.Users.Add(u);
        }

        // 2.3 Staff (Зав. Кафедрой ИКТ)
        if (depts.TryGetValue(2, out Department? deptICT))
        {
            User staffUser = User.CreateStaff(
                Login.Create("HEAD_ICT").Value,
                "Зав. Кафедрой ИКТ",
                teacherId: null,
                departmentId: deptICT.Id,
                passwordHash: defaultPass,
                role: UserRole.DeputyHead
            ).Value;
            context.Users.Add(staffUser);
        }

        await context.SaveChangesAsync();

        // ================= 3. FORMS =================

        // Form 1: Удовл. преподаванием
        Form f1 = Form.Create("Оценка удовлетворённости обучающихся преподаванием учебных дисциплин",
            [FilterField.Discipline]).Value;

        // ВАЖНО: Устанавливаем тот самый ID, который ищет фронтенд/лог
        SetId(f1, Guid.Parse("2ccf04c2-0197-4d71-8b46-ac3394bfc8e5"));

        f1.AddQuestion("Содержание образовательной программы", QuestionType.WeightedRating, 1);
        f1.AddQuestion("Лекционные занятия (методы)", QuestionType.WeightedRating, 2);
        f1.AddQuestion("Практические и лабораторные занятия", QuestionType.WeightedRating, 3);
        f1.AddQuestion("Информационное обеспечение", QuestionType.WeightedRating, 4);
        f1.AddQuestion("Материально-техническое обеспечение", QuestionType.WeightedRating, 5);
        f1.AddQuestion("Ваши предложения по улучшению", QuestionType.Text, 6);

        context.Forms.Add(f1);

        Form f7 = Form.Create("Оценка руководителей производственной практики",
            [FilterField.Speciality]).Value;

        SetId(f7, Guid.Parse("77777777-7777-7777-7777-777777777777"));

        f7.AddQuestion("Актуальность теоретических знаний", QuestionType.Number, 1);
        f7.AddQuestion("Качество практических навыков", QuestionType.Number, 2);
        f7.AddQuestion("Дисциплина и исполнительность", QuestionType.Number, 3);
        f7.AddQuestion("Затруднения при работе", QuestionType.MultipleChoice, 4);
        f7.AddQuestion("Предложения", QuestionType.Text, 5);

        context.Forms.Add(f7);

        await context.SaveChangesAsync();

        logger.LogInformation("Seeding completed. Fixed Form ID restored.");
    }

    // Хелпер для установки ID (так как set; private)
    private static void SetId<T>(T entity, Guid id) where T : Entity
    {
        typeof(T).GetProperty(nameof(Entity.Id))?.SetValue(entity, id);
    }
}
