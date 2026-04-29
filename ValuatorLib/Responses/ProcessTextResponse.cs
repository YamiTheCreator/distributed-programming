using ValuatorLib.Models;

namespace ValuatorLib.Responses;

public sealed class ProcessTextResponse
{
    public AnalysisId Id { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static ProcessTextResponse Successful(AnalysisId id) => new()
    {
        Id = id,
        Success = true
    };

    public static ProcessTextResponse Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}