using System.Threading;
using System.Threading.Tasks;
using Treasure.GameLogic.State;
using Treasure.GameLogic.State.Field;
using Treasure.GameLogic.State.History;

namespace Treasure.GameLogic.Controllers
{
    public interface IPlayerController
    {
        void OnGameStart(int yourIndex, GameFieldParams fieldParams);
        Task<PlayerAction> GetActionAsync(int yourIndex, CancellationToken cancellationToken, GameHistory history);
    }
}