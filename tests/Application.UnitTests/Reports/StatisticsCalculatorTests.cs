using Application.Reports.Queries.Shared;

namespace Application.UnitTests.Reports;

public class StatisticsCalculatorTests
{
    [Fact]
    public void CalculateSatisfactionPercentage_WithUniformWeights_ReturnsMeanRatioAsPercentage()
    {
        List<(decimal Score, decimal Weight)> ratings = [(8, 10), (6, 10), (10, 10)];

        decimal result = StatisticsCalculator.CalculateSatisfactionPercentage(ratings);

        result.ShouldBe(80m);
    }

    [Fact]
    public void CalculateSatisfactionPercentage_WithVaryingWeights_WeightsEachRespondentIndividually()
    {
        // Formula (1): mean of (score / weight) per respondent, not sum(scores) / sum(weights).
        List<(decimal Score, decimal Weight)> ratings = [(5, 5), (2, 10)];

        decimal result = StatisticsCalculator.CalculateSatisfactionPercentage(ratings);

        result.ShouldBe(60m);
    }

    [Fact]
    public void CalculateSatisfactionPercentage_ExcludesZeroWeightRespondents()
    {
        List<(decimal Score, decimal Weight)> ratings = [(5, 10), (999, 0)];

        decimal result = StatisticsCalculator.CalculateSatisfactionPercentage(ratings);

        result.ShouldBe(50m);
    }

    [Fact]
    public void CalculateSatisfactionPercentage_AllZeroWeight_ReturnsZero()
    {
        List<(decimal Score, decimal Weight)> ratings = [(5, 0), (3, 0)];

        decimal result = StatisticsCalculator.CalculateSatisfactionPercentage(ratings);

        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateSatisfactionPercentage_EmptyList_ReturnsZero()
    {
        decimal result = StatisticsCalculator.CalculateSatisfactionPercentage([]);

        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateAverageScore_ReturnsArithmeticMean()
    {
        List<decimal> scores = [4, 6, 8];

        decimal result = StatisticsCalculator.CalculateAverageScore(scores);

        result.ShouldBe(6m);
    }

    [Fact]
    public void CalculateAverageScore_EmptyList_ReturnsZero()
    {
        decimal result = StatisticsCalculator.CalculateAverageScore([]);

        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateStandardDeviation_ReturnsPopulationStandardDeviation()
    {
        List<decimal> scores = [2, 4, 4, 4, 5, 5, 7, 9];

        decimal result = StatisticsCalculator.CalculateStandardDeviation(scores);

        result.ShouldBe(2m);
    }

    [Fact]
    public void CalculateStandardDeviation_SingleValue_ReturnsZero()
    {
        decimal result = StatisticsCalculator.CalculateStandardDeviation([5]);

        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateStandardDeviation_EmptyList_ReturnsZero()
    {
        decimal result = StatisticsCalculator.CalculateStandardDeviation([]);

        result.ShouldBe(0m);
    }

    [Theory]
    [InlineData(0, SatisfactionRating.Unsatisfactory)]
    [InlineData(39, SatisfactionRating.Unsatisfactory)]
    [InlineData(40, SatisfactionRating.Satisfactory)]
    [InlineData(59, SatisfactionRating.Satisfactory)]
    [InlineData(60, SatisfactionRating.Good)]
    [InlineData(79, SatisfactionRating.Good)]
    [InlineData(80, SatisfactionRating.Excellent)]
    [InlineData(100, SatisfactionRating.Excellent)]
    public void ClassifySatisfaction_UsesTable1Thresholds(int satisfactionPercentage, SatisfactionRating expected)
    {
        SatisfactionRating result = StatisticsCalculator.ClassifySatisfaction(satisfactionPercentage);

        result.ShouldBe(expected);
    }

    [Fact]
    public void CalculateOverallSatisfaction_ReturnsMeanPercentageAverageStdDevAndRating()
    {
        // Formulas (5) and (6): mean of per-question percentages/std-devs, classified via Table 1.
        List<decimal> perQuestionPercentages = [90, 70, 50];
        List<decimal> perQuestionStandardDeviations = [1, 2, 3];

        OverallSatisfaction result = StatisticsCalculator.CalculateOverallSatisfaction(
            perQuestionPercentages,
            perQuestionStandardDeviations);

        result.MeanPercentage.ShouldBe(70m);
        result.AverageStandardDeviation.ShouldBe(2m);
        result.Rating.ShouldBe(SatisfactionRating.Good);
        result.HasData.ShouldBeTrue();
    }

    [Fact]
    public void CalculateOverallSatisfaction_EmptyLists_ReturnsHasDataFalse()
    {
        // Empty input (no submissions/questions) must be reported as "no data", not as a
        // misleading 0% Unsatisfactory result.
        OverallSatisfaction result = StatisticsCalculator.CalculateOverallSatisfaction([], []);

        result.HasData.ShouldBeFalse();
    }
}
