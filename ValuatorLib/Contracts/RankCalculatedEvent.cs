namespace ValuatorLib.Contracts;

public record RankCalculatedEvent
{
    public string Id { get; init; } = string.Empty;
    public double Rank { get; init; }
}
