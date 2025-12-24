using FluentValidation;

namespace Application.Submissions.Create;

internal sealed class CreateSubmissionCommandValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionCommandValidator()
    {
        RuleFor(x => x.FormId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Answers)
            .NotEmpty();

        RuleForEach(x => x.Answers)
            .SetValidator(new AnswerRequestValidator());

    }
}

internal sealed class AnswerRequestValidator : AbstractValidator<AnswerRequest>
{
    public AnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Value) || x.NumericValue.HasValue)
            .WithMessage("Either Value or NumericValue must be provided");

        RuleFor(x => x.NumericValue)
        .InclusiveBetween(1, 10)
        .When(x => x.NumericValue.HasValue)
        .WithMessage("Оценка должна быть от 1 до 10");

        RuleFor(x => x.Weight)
            .InclusiveBetween(1, 10)
            .When(x => x.Weight.HasValue)
            .WithMessage("Вес должен быть от 1 до 10");
    }
}
