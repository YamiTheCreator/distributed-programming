using ValuatorLib.Contracts;

namespace ValuatorLib.Interfaces;

public interface IRankCalculationService
{
    Task SendRankCalculationTaskAsync(RankCalculationMessage message);
}