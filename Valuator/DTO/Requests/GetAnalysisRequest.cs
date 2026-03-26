using System.ComponentModel.DataAnnotations;

namespace Valuator.DTO.Requests;

public class GetAnalysisRequest
{
    [Required(ErrorMessage = "ID обязателен")]
    public string Id { get; set; } = string.Empty;
}