using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Treasure.GameLogic.Message;
using Treasure.GameLogic.State;
using Treasure.GameLogic.State.Field;
using Treasure.GameLogic.State.History;

namespace Treasure.GameLogic.Controllers
{
    public abstract class BotBase : IPlayerController, IPlayerNotifier
    {
        public abstract void OnGameStart(int yourIndex, GameFieldParams fieldParams);

        protected abstract PlayerAction GetAction(int yourIndex, CancellationToken cancellationToken,
            GameHistory lastRoundDeathCount);
        public Task<PlayerAction> GetActionAsync(int yourIndex, CancellationToken cancellationToken, GameHistory history) => 
            Task.FromResult(GetAction(yourIndex, cancellationToken, history));

        public abstract void PreMove(int playerIndex, int lastRoundDeathCount);
        public abstract void PostMove(int playerIndex, PlayerAction move, ImmutableArray<PlayerMessageElement> message);
    }
}