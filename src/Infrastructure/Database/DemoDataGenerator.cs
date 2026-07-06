using Domain.College.Departments;
using Domain.College.Disciplines;
using Domain.College.Specialities;
using Domain.College.Specializations;
using Domain.College.Teachers;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System.Security.Cryptography;

namespace Infrastructure.Database;

public sealed class DemoDataGenerator
{
    private static readonly string[] DepartmentNames =
    [
        "ГН",
        "ЗОЖ",
        "ИКТ",
        "ОТПС",
        "ПОСТ",
        "РИТ",
        "ТКС",
        "ФИМОИ",
        "ЦЭ"
    ];

    private static readonly string[] GroupNames =
    [
        "КС411",
        "РТ411",
        "ПД411",
        "ТК521",
        "ТК522",
        "РТ521",
        "ПД521",
        "ТК311",
        "ТК312",
        "ТК421",
        "ТК422",
        "КС311",
        "РТ311",
        "РТ421",
        "ПД311",
        "ПД421",
        "ПО211",
        "ТЭ211",
        "ТЭ212",
        "ТК321",
        "ТК322",
        "РТ321"
    ];

    private static readonly string[] TeacherNames =
    [
        "Белан Елена Михайловна",
        "Берестень Татьяна Александровна",
        "Богданов Кирилл Викторович",
        "Борейко Елена Александровна",
        "Бочарова Елена Владимировна",
        "Варнава Анастасия Анатольевна",
        "Васильчук Наталья Викторовна",
        "Васина Светлана Александровна",
        "Воеводская Оксана Михайловна",
        "Воронова Олеся Анатольевна",
        "Воронович Ирина Тихоновна",
        "Воропаева Елена Валерьевна",
        "Галузо Светлана Борисовна",
        "Дисько Ирина Васильевна",
        "Дук Михаил Леонидович",
        "Емельянов Сергей Владимирович",
        "Ерошевич Марина Юрьевна",
        "Жилинская Татьяна Николаевна",
        "Иванова Ирина Владимировна",
        "Иванова Анастасия Юрьевна",
        "Иващенко Виктор Николаевич",
        "Исаченко Людмила Геннадьевна"
    ];

    private static readonly Dictionary<string, string[]> DisciplineTemplates = new()
    {
        ["ГН"] = ["Психология общения", "История Беларуси"],
        ["ЗОЖ"] = ["Физическая культура", "Основы здорового образа жизни"],
        ["ИКТ"] = ["Web-технологии", "Базы данных"],
        ["ОТПС"] = ["Автоматизация почтовых процессов", "Технологии почтовой связи"],
        ["ПОСТ"] = ["Организация почтовой связи", "Логистика почтовых отправлений"],
        ["РИТ"] = ["Радиотехнические измерения", "Цифровая обработка сигналов"],
        ["ТКС"] = ["Сети связи", "Маршрутизация и коммутация"],
        ["ФИМОИ"] = ["Математика", "Информатика"],
        ["ЦЭ"] = ["Экономика", "Цифровая экономика"]
    };

    private static readonly string[] SpecialtyNames =
    [
        "Сети телекоммуникаций",
        "Почтовая связь",
        "Системы радиосвязи, радиовещания и телевидения"
    ];

    private static readonly (string Name, int SpecialtyIndex)[] SpecializationData =
    [
        ("ПО сетей телеком", 0),
        ("Тех. экспл. сетей телеком", 0),
        ("Экспл. инфо-тех сетей", 1),
        ("Радиосистемы охраны", 2),
        ("Орг. торговли почтой", 1)
    ];

    private static readonly string[] SuggestionTexts =
    [
        "Хотелось бы больше практических занятий.",
        "Нужны современные кейсы и реальные примеры.",
        "Все хорошо, особенно удобная подача материала.",
        "Стоит добавить больше материалов для самостоятельной работы.",
        "Полезно увеличить количество лабораторных работ."
    ];

    private static readonly string[] PracticeDifficulties =
    [
        "Адаптация к рабочему месту",
        "Недостаток практических навыков",
        "Нехватка теоретической базы",
        "Сложности с документооборотом",
        "Серьезных затруднений не было"
    ];

    public async Task SeedAsync(ApplicationDbContext context, string defaultPasswordHash, CancellationToken cancellationToken = default)
    {
        Dictionary<string, Department> departments = CreateDepartments();
        await context.Departments.AddRangeAsync(departments.Values, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        List<Teacher> teachers = CreateTeachers(departments);
        await context.Teachers.AddRangeAsync(teachers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        List<Speciality> specialities = CreateSpecialities();
        await context.Specialities.AddRangeAsync(specialities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        List<Specialization> specializations = CreateSpecializations(specialities);
        await context.Specializations.AddRangeAsync(specializations, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        List<Discipline> disciplines = CreateDisciplines(departments);
        await context.Disciplines.AddRangeAsync(disciplines, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        List<User> users = CreateUsers(defaultPasswordHash, departments, teachers);
        await context.Users.AddRangeAsync(users, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        List<Form> forms = CreateForms();
        await context.Forms.AddRangeAsync(forms, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await SeedSubmissionsAsync(
            context,
            forms,
            users.Where(user => user.Role == UserRole.StudentGroup).ToList(),
            departments.Values.ToList(),
            disciplines,
            teachers,
            specialities,
            specializations,
            cancellationToken);
    }

    private Dictionary<string, Department> CreateDepartments()
    {
        Dictionary<string, Department> departments = [];

        foreach (string departmentName in DepartmentNames)
        {
            Department department = Department.Create(departmentName).Value;
            departments[departmentName] = department;
        }

        return departments;
    }

    private List<Teacher> CreateTeachers(Dictionary<string, Department> departments)
    {
        List<Teacher> teachers = [];

        for (int index = 0; index < TeacherNames.Length; index++)
        {
            string teacherName = TeacherNames[index];
            Guid departmentId = departments[DepartmentNames[index % DepartmentNames.Length]].Id;
            Teacher teacher = Teacher.Create(teacherName, departmentId).Value;
            teachers.Add(teacher);
        }

        return teachers;
    }

    private List<Speciality> CreateSpecialities()
    {
        List<Speciality> specialities = [];

        foreach (string specialtyName in SpecialtyNames)
        {
            Speciality speciality = Speciality.Create(specialtyName).Value;
            specialities.Add(speciality);
        }

        return specialities;
    }

    private List<Specialization> CreateSpecializations(List<Speciality> specialities)
    {
        List<Specialization> specializations = [];

        foreach ((string name, int specialtyIndex) in SpecializationData)
        {
            Specialization specialization = Specialization.Create(name, specialities[specialtyIndex].Id).Value;
            specializations.Add(specialization);
        }

        return specializations;
    }

    private List<Discipline> CreateDisciplines(Dictionary<string, Department> departments)
    {
        List<Discipline> disciplines = [];

        foreach (KeyValuePair<string, string[]> template in DisciplineTemplates)
        {
            Department department = departments[template.Key];

            foreach (string disciplineName in template.Value)
            {
                Discipline discipline = Discipline.Create(disciplineName, department.Id).Value;
                disciplines.Add(discipline);
            }
        }

        return disciplines;
    }

    private List<User> CreateUsers(
        string defaultPasswordHash,
        Dictionary<string, Department> departments,
        List<Teacher> teachers)
    {
        List<User> users = [];

        User admin = User.CreateAdmin(Login.Create("ADMIN").Value, defaultPasswordHash).Value;
        users.Add(admin);

        foreach (string groupName in GroupNames)
        {
            User groupUser = User.CreateGroupUser(
                GroupName.Create(groupName).Value,
                Guid.NewGuid(),
                defaultPasswordHash).Value;

            users.Add(groupUser);
        }

        Department ictDepartment = departments["ИКТ"];
        User deputyHead = User.CreateStaff(
            Login.Create("HEAD_ICT").Value,
            "Зав. Кафедрой ИКТ",
            teacherId: null,
            departmentId: ictDepartment.Id,
            passwordHash: defaultPasswordHash,
            role: UserRole.DeputyHead).Value;
        users.Add(deputyHead);

        for (int index = 0; index < Math.Min(teachers.Count, DepartmentNames.Length); index++)
        {
            Teacher teacher = teachers[index];
            Department department = departments[DepartmentNames[index]];
            string loginValue = $"STAFF{index + 1:00}";

            User staffUser = User.CreateStaff(
                Login.Create(loginValue).Value,
                teacher.FullName,
                teacherId: teacher.Id,
                departmentId: department.Id,
                passwordHash: defaultPasswordHash,
                role: UserRole.Staff).Value;

            users.Add(staffUser);
        }

        return users;
    }

    private List<Form> CreateForms()
    {
        List<Form> forms = [];

        Form disciplineForm = Form.Create(
            "Оценка удовлетворённости обучающихся преподаванием учебных дисциплин",
            [FilterField.Discipline]).Value;
        disciplineForm.SetIdForSeeding(Guid.NewGuid());
        disciplineForm.AddQuestion("Содержание образовательной программы", QuestionType.WeightedRating, 1);
        disciplineForm.AddQuestion("Лекционные занятия (методы)", QuestionType.WeightedRating, 2);
        disciplineForm.AddQuestion("Практические и лабораторные занятия", QuestionType.WeightedRating, 3);
        disciplineForm.AddQuestion("Информационное обеспечение", QuestionType.WeightedRating, 4);
        disciplineForm.AddQuestion("Материально-техническое обеспечение", QuestionType.WeightedRating, 5);
        disciplineForm.AddQuestion("Ваши предложения по улучшению", QuestionType.Text, 6);
        forms.Add(disciplineForm);

        Form practiceForm = Form.Create(
            "Оценка руководителей производственной практики",
            [FilterField.Speciality]).Value;
        practiceForm.SetIdForSeeding(Guid.NewGuid());
        practiceForm.AddQuestion("Актуальность теоретических знаний", QuestionType.Number, 1);
        practiceForm.AddQuestion("Качество практических навыков", QuestionType.Number, 2);
        practiceForm.AddQuestion("Дисциплина и исполнительность", QuestionType.Number, 3);
        practiceForm.AddQuestion("Затруднения при работе", QuestionType.MultipleChoice, 4);
        practiceForm.AddQuestion("Предложения", QuestionType.Text, 5);
        forms.Add(practiceForm);

        return forms;
    }

    private async Task SeedSubmissionsAsync(
        ApplicationDbContext context,
        List<Form> forms,
        List<User> groupUsers,
        List<Department> departments,
        List<Discipline> disciplines,
        List<Teacher> teachers,
        List<Speciality> specialities,
        List<Specialization> specializations,
        CancellationToken cancellationToken)
    {
        Form disciplineForm = forms[0];
        Form practiceForm = forms[1];

        List<Submission> submissions = [];
        submissions.AddRange(CreateDisciplineFormSubmissions(disciplineForm, groupUsers, departments, disciplines, teachers, specialities, specializations));
        submissions.AddRange(CreatePracticeFormSubmissions(practiceForm, groupUsers, departments, disciplines, teachers, specialities, specializations));

        await context.Submissions.AddRangeAsync(submissions, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private List<Submission> CreateDisciplineFormSubmissions(
        Form form,
        List<User> groupUsers,
        List<Department> departments,
        List<Discipline> disciplines,
        List<Teacher> teachers,
        List<Speciality> specialities,
        List<Specialization> specializations)
    {
        List<Submission> submissions = [];

        for (int index = 0; index < 140; index++)
        {
            User groupUser = groupUsers[NextInt(groupUsers.Count)];
            Discipline discipline = disciplines[NextInt(disciplines.Count)];
            Department department = departments.First(item => item.Id == discipline.DepartmentId);
            Teacher teacher = teachers[NextInt(teachers.Count)];
            Speciality speciality = specialities[NextInt(specialities.Count)];
            var specializationCandidates = specializations
                .Where(item => item.SpecialityId == speciality.Id)
                .ToList();
            Specialization specialization = specializationCandidates[NextInt(specializationCandidates.Count)];

            Result<Submission> submissionResult = Submission.Create(
                form.Id,
                $"demo-discipline-{index:000}",
                groupUser.Id,
                DateTime.UtcNow,
                disciplineId: discipline.Id,
                teacherId: teacher.Id,
                departmentId: department.Id,
                specialityId: speciality.Id,
                specializationId: specialization.Id,
                organizationName: "Белорусская государственная академия связи");

            Submission submission = submissionResult.Value;
            SetSubmittedAt(submission, GetHistoricalDate(index));

            decimal baseScore = GetDepartmentScore(department.Name);

            foreach (Question question in form.Questions.OrderBy(item => item.Order))
            {
                if (question.Type == QuestionType.WeightedRating)
                {
                    decimal weight = NextInt(6, 11);
                    decimal rawScore = baseScore + NextDecimalDelta(1.2m);
                    decimal boundedScore = Math.Max(1, Math.Min(weight, Math.Round(rawScore, 0, MidpointRounding.AwayFromZero)));
                    submission.AddAnswer(question.Id, numericValue: boundedScore, weight: weight);
                    continue;
                }

                submission.AddAnswer(question.Id, value: SuggestionTexts[NextInt(SuggestionTexts.Length)]);
            }

            submissions.Add(submission);
        }

        return submissions;
    }

    private List<Submission> CreatePracticeFormSubmissions(
        Form form,
        List<User> groupUsers,
        List<Department> departments,
        List<Discipline> disciplines,
        List<Teacher> teachers,
        List<Speciality> specialities,
        List<Specialization> specializations)
    {
        List<Submission> submissions = [];

        for (int index = 0; index < 90; index++)
        {
            User groupUser = groupUsers[NextInt(groupUsers.Count)];
            Discipline discipline = disciplines[NextInt(disciplines.Count)];
            Department department = departments.First(item => item.Id == discipline.DepartmentId);
            Teacher teacher = teachers[NextInt(teachers.Count)];
            Speciality speciality = specialities[NextInt(specialities.Count)];
            var specializationCandidates = specializations
                .Where(item => item.SpecialityId == speciality.Id)
                .ToList();
            Specialization specialization = specializationCandidates[NextInt(specializationCandidates.Count)];

            Result<Submission> submissionResult = Submission.Create(
                form.Id,
                $"demo-practice-{index:000}",
                groupUser.Id,
                DateTime.UtcNow,
                disciplineId: discipline.Id,
                teacherId: teacher.Id,
                departmentId: department.Id,
                specialityId: speciality.Id,
                specializationId: specialization.Id,
                organizationName: "Белорусская государственная академия связи");

            Submission submission = submissionResult.Value;
            SetSubmittedAt(submission, GetHistoricalDate(index + 35));

            decimal baseScore = GetDepartmentScore(department.Name);

            foreach (Question question in form.Questions.OrderBy(item => item.Order))
            {
                if (question.Type == QuestionType.Number)
                {
                    decimal numericValue = Math.Max(
                        1,
                        Math.Min(10, Math.Round(baseScore + NextDecimalDelta(1.5m), 0, MidpointRounding.AwayFromZero)));
                    submission.AddAnswer(question.Id, numericValue: numericValue);
                    continue;
                }

                if (question.Type == QuestionType.MultipleChoice)
                {
                    submission.AddAnswer(question.Id, value: PracticeDifficulties[NextInt(PracticeDifficulties.Length)]);
                    continue;
                }

                submission.AddAnswer(question.Id, value: SuggestionTexts[NextInt(SuggestionTexts.Length)]);
            }

            submissions.Add(submission);
        }

        return submissions;
    }

    private DateTime GetHistoricalDate(int offset)
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime month = utcNow.AddMonths(-(offset % 14));
        int day = Math.Min(25, 1 + offset * 3 % 27);
        int hour = 8 + offset % 9;

        return new DateTime(month.Year, month.Month, day, hour, 30, 0, DateTimeKind.Utc);
    }

    private static int NextInt(int exclusiveMax)
    {
        return RandomNumberGenerator.GetInt32(exclusiveMax);
    }

    private static int NextInt(int inclusiveMin, int exclusiveMax)
    {
        return RandomNumberGenerator.GetInt32(inclusiveMin, exclusiveMax);
    }

    private static decimal NextDecimalDelta(decimal halfRange)
    {
        decimal normalized = RandomNumberGenerator.GetInt32(0, 10_000) / 10_000m;
        return normalized * halfRange * 2 - halfRange;
    }

    private static void SetSubmittedAt(Submission submission, DateTime submittedAt)
    {
        typeof(Submission)
            .GetProperty(nameof(Submission.SubmittedAt))!
            .SetValue(submission, submittedAt);
    }

    private static decimal GetDepartmentScore(string departmentName)
    {
        return departmentName switch
        {
            "ИКТ" => 8.8m,
            "ТКС" => 8.5m,
            "РИТ" => 8.1m,
            "ПОСТ" => 7.8m,
            "ОТПС" => 7.6m,
            "ФИМОИ" => 7.4m,
            "ЦЭ" => 7.2m,
            "ГН" => 6.9m,
            "ЗОЖ" => 8.3m,
            _ => 7.0m
        };
    }
}
