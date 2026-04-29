namespace ValuatorLib.Requests;

public sealed class ProcessTextRequest
{
    public string Text { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}