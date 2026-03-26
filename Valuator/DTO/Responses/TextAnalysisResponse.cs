namespace Valuator.DTO.Responses;

public class TextAnalysisResponse
{
    public string Id { get; set; } = string.Empty;
    public double? Rank { get; set; }
    public double Similarity { get; set; }
    public bool IsRankCalculationCompleted => Rank.HasValue;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}