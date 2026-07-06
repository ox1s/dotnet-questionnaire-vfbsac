namespace Domain.Questionnaires.Forms;

[Flags]
public enum QuestionType
{
    Text = 1,
    Number = 2,
    MultipleChoice = 3,
    SingleChoice = 4,
    WeightedRating = 6
}
