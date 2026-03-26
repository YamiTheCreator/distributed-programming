using System.ComponentModel.DataAnnotations;

namespace Valuator.DTO.Requests;

public class ProcessTextRequest
{
    [Required(ErrorMessage = "Текст обязателен для заполнения")]
    [StringLength(10000, ErrorMessage = "Текст не может превышать 10000 символов")]
    public string Text { get; set; } = string.Empty;
}