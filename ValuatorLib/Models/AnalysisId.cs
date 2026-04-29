namespace ValuatorLib.Models;

public readonly record struct AnalysisId(Guid Value)
{
    public static AnalysisId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator string(AnalysisId id) => id.ToString();
    public static implicit operator AnalysisId(Guid guid) => new(guid);
}