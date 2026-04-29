namespace ValuatorLib.Models;

public record ShardKey(Region Region)
{
    public string Value => Region.ToString();

    public static ShardKey FromCountry(Country country) => new(country.Region);
    
    public static ShardKey FromRegion(Region region) => new(region);

    public override string ToString() => Value;
}
