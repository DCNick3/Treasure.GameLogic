using System.Collections.Immutable;
using Treasure.GameLogic.Message;

namespace Treasure.GameLogic.State.History
{
    public class GameHistoryItem
    {
        public GameHistoryItem(PreMoveInfo preMoveInfo)
        {
            PreMoveInfo = preMoveInfo;
        }

        public GameHistoryItem(PreMoveInfo preMoveInfo, PlayerAction action, ImmutableArray<PlayerMessageElement> actionResult)
        {
            PreMoveInfo = preMoveInfo;
            Action = action;
            ActionResult = actionResult;
        }
        
        public PreMoveInfo PreMoveInfo { get; }
        public PlayerAction? Action { get; }
        public ImmutableArray<PlayerMessageElement>? ActionResult { get; }

        public bool IsComplete => Action != null;
        
        public GameHistoryItem WithAction(PlayerAction action, ImmutableArray<PlayerMessageElement> postMoveInfo)
            => new GameHistoryItem(PreMoveInfo, action, postMoveInfo);
    }
}