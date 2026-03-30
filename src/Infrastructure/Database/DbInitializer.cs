using System.Reflection;
using Application.Abstractions.Authentication;
using Domain.College.Departments;
using Domain.College.Disciplines;
using Domain.College.Specialities;
using Domain.College.Specializations;
using Domain.Questionnaires.Forms;
using Domain.User;
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

        Dictionary<int, Department> depts = [];
        var deptData = new Dictionary<int, (string Name, Guid Id)>
        {
            {1, ("ПОСТ", Guid.NewGuid())},
            {2, ("ИКТ", Guid.NewGuid())},
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
            d.SetIdForSeeding(kvp.Value.Id);
            context.Departments.Add(d);
            depts.Add(kvp.Key, d);
        }
        await context.SaveChangesAsync();

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

        Dictionary<int, Specialization> specializations = [];
        var speczData = new List<(int Id, string Name, int SpecId)>
        {
            (2, "ПО сетей телеком", 1),
            (4, "Тех.экспл. сетей телеком", 1),
            (6, "Экспл. инфо-тех сетей", 2),
            (8, "Радиосистемы охраны", 3),
            (10, "Орг. торговли почтой", 2)
        };
        foreach ((int id, string name, int specId) in speczData)
        {
            Specialization s = Specialization.Create(name, specs[specId].Id).Value;
            context.Specializations.Add(s);
            specializations.Add(id, s);
        }
        await context.SaveChangesAsync();

        Dictionary<int, Discipline> disciplines = [];
        var discData = new List<(int Id, string Name, int DeptId)>
        {
            (5, "КПиЯП", 1),
            (7, "ИТ", 2),
            (8, "ООП", 1),
            (29, "Теор.алг", 1),
            (33, "Тех.комм и ОК", 6),
            (50, "Инф", 2)
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


        string defaultPass = passwordHasher.Hash("12345678");

        User admin = User.CreateAdmin(Login.Create("ADMIN").Value, defaultPass).Value;
        context.Users.Add(admin);

        string[] groups = ["ПО111", "ТС111", "РТ111", "ПС111"];
        foreach (string gName in groups)
        {
            User u = User.CreateGroupUser(GroupName.Create(gName).Value, Guid.NewGuid(), defaultPass).Value;
            context.Users.Add(u);
        }

        if (depts.TryGetValue(2, out Department? deptIct))
        {
            User staffUser = User.CreateStaff(
                Login.Create("HEAD_ICT").Value,
                "Зав. Кафедрой ИКТ",
                teacherId: null,
                departmentId: deptIct.Id,
                passwordHash: defaultPass,
                role: UserRole.DeputyHead
            ).Value;
            context.Users.Add(staffUser);
        }

        await context.SaveChangesAsync();

        Form f1 = Form.Create("Оценка удовлетворённости обучающихся преподаванием учебных дисциплин",
            [FilterField.Discipline]).Value;

        f1.SetIdForSeeding(Guid.NewGuid());

        f1.AddQuestion("Содержание образовательной программы", QuestionType.WeightedRating, 1);
        f1.AddQuestion("Лекционные занятия (методы)", QuestionType.WeightedRating, 2);
        f1.AddQuestion("Практические и лабораторные занятия", QuestionType.WeightedRating, 3);
        f1.AddQuestion("Информационное обеспечение", QuestionType.WeightedRating, 4);
        f1.AddQuestion("Материально-техническое обеспечение", QuestionType.WeightedRating, 5);
        f1.AddQuestion("Ваши предложения по улучшению", QuestionType.Text, 6);

        context.Forms.Add(f1);

        Form f7 = Form.Create("Оценка руководителей производственной практики",
            [FilterField.Speciality]).Value;

        f7.SetIdForSeeding(Guid.NewGuid());

        f7.AddQuestion("Актуальность теоретических знаний", QuestionType.Number, 1);
        f7.AddQuestion("Качество практических навыков", QuestionType.Number, 2);
        f7.AddQuestion("Дисциплина и исполнительность", QuestionType.Number, 3);
        f7.AddQuestion("Затруднения при работе", QuestionType.MultipleChoice, 4);
        f7.AddQuestion("Предложения", QuestionType.Text, 5);

        context.Forms.Add(f7);

        await context.SaveChangesAsync();

        logger.LogInformation("Seeding completed. Fixed Form ID restored.");
    }
}
