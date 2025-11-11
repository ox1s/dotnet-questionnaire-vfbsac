namespace Questionnaire.Contracts.Authentication;

public record RegisterRequest(
    string Login,
    string Password,
    string Role); 