namespace Valuator.DTO.Responses;

public class ProcessTextResponse
{
    public string Id { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}