using System.Collections.Immutable;
using Treasure.GameLogic.Message;
using Treasure.GameLogic.State;

namespace Treasure.GameLogic.Controllers
{
    public interface IPlayerNotifier
    {
        void PreMove(int playerIndex, int lastRoundDeathCount);
        void PostMove(int playerIndex, PlayerAction move,
            ImmutableArray<PlayerMessageElement> message);
    }
}