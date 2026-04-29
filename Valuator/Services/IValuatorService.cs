using ValuatorLib.Requests;
using ValuatorLib.Responses;

namespace Valuator.Services;

public interface IValuatorService
{
    Task<ProcessTextResponse> ProcessTextAsync(ProcessTextRequest request);
    Task<TextAnalysisResponse> GetAnalysisResultAsync(GetAnalysisRequest request);
}