namespace Questionnaire.SharedKernel;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
