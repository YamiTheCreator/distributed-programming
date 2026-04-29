using ValuatorLib.Requests;
using ValuatorLib.Responses;

namespace Application.Services;

public interface ITextAnalysisApiService
{
    Task<ProcessTextResponse?> ProcessTextAsync(ProcessTextRequest request);
    Task<TextAnalysisResponse?> GetAnalysisAsync(Guid id);
}