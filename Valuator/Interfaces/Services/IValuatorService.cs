using Valuator.DTO.Requests;
using Valuator.DTO.Responses;

namespace Valuator.Interfaces.Services;

public interface IValuatorService
{
    Task<ProcessTextResponse> ProcessTextAsync(ProcessTextRequest request);
    Task<TextAnalysisResponse> GetAnalysisResultAsync(GetAnalysisRequest request);
}