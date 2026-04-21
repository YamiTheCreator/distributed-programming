namespace ValuatorLib.Contracts;

public record SimilarityCalculatedEvent
{
    public string Id { get; init; } = string.Empty;
    public double Similarity { get; init; }
}
