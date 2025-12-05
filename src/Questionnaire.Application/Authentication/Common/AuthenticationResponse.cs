namespace Questionnaire.Application.Authentication.Common;

public record AuthenticationResponse(
    int Id,
    string Login,
    string Token);
