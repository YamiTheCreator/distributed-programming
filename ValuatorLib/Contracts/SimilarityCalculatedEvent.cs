using ValuatorLib.Models;

namespace ValuatorLib.Contracts;

public record SimilarityCalculatedEvent
{
    public AnalysisId Id { get; init; }
    public double Similarity { get; init; }
}
