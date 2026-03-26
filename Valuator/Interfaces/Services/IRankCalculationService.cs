using Contracts;

namespace Valuator.Interfaces.Services;

public interface IRankCalculationService
{
    Task SendRankCalculationTaskAsync(RankCalculationMessage message);
}