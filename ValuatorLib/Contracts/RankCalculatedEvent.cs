using ValuatorLib.Models;

namespace ValuatorLib.Contracts;

public record RankCalculatedEvent
{
    public AnalysisId Id { get; init; }
    public double Rank { get; init; }
}
