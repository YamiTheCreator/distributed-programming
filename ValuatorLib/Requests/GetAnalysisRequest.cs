using ValuatorLib.Models;

namespace ValuatorLib.Requests;

public sealed class GetAnalysisRequest
{
    public AnalysisId Id { get; set; }
}