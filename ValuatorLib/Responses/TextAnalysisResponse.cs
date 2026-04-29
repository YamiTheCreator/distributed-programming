using ValuatorLib.Models;

namespace ValuatorLib.Responses;

public sealed class TextAnalysisResponse
{
    public AnalysisId Id { get; set; }
    public double? Rank { get; set; }
    public double Similarity { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static TextAnalysisResponse Successful(AnalysisId id, double? rank, double similarity) => new()
    {
        Id = id,
        Rank = rank,
        Similarity = similarity,
        Success = true
    };

    public static TextAnalysisResponse Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}