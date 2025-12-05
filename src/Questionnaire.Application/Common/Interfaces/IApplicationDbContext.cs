using Microsoft.EntityFrameworkCore;
using Questionnaire.Domain.Answers;
using Questionnaire.Domain.Forms;
using Questionnaire.Domain.Questions;
using Questionnaire.Domain.Users;

namespace Questionnaire.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Form> Forms { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<FormQuestion> FormQuestions { get; }
    DbSet<Answer> Answers { get; }
    DbSet<AnswerDetail> AnswerDetails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}