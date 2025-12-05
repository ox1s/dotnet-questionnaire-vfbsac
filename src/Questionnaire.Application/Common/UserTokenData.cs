namespace Questionnaire.Application.Common;

public record UserTokenData(int Id, string Login, IEnumerable<string> Roles);
