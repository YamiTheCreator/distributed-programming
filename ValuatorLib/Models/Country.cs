namespace ValuatorLib.Models;

public record Country(string Code, string Name, Region Region);

public enum Region
{
    RU,
    EU,
    ASIA
}

public static class Countries
{
    public static readonly Country Russia = new("RU", "Russia", Region.RU);
    public static readonly Country France = new("FR", "France", Region.EU);
    public static readonly Country Germany = new("DE", "Germany", Region.EU);
    public static readonly Country UAE = new("AE", "UAE", Region.ASIA);
    public static readonly Country India = new("IN", "India", Region.ASIA);

    public static readonly IReadOnlyList<Country> All = new List<Country>
    {
        Russia,
        France,
        Germany,
        UAE,
        India
    };

    public static Country? GetByCode(string code)
    {
        return All.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }
}
