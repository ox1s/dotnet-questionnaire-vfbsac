namespace Questionnaire.SharedKernel;

public record ValidationError(Error[] Errors) : Error(
    "Validation.General",
    "One or more validation errors occurred",
    ErrorType.Validation);
