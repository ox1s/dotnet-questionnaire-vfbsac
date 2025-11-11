namespace Questionnaire.Contracts.Authentication;

public record AuthenticationResponse(
    int Id,
    string Login,
    string Token);