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
