using ValuatorLib.Models;

namespace ValuatorLib.Contracts;

public record RankCalculationMessage
{
    public AnalysisId Id { get; init; }
}
