using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Treasure.GameLogic.Message;

namespace Treasure.GameLogic.State.History
{
    public class GameHistory
    {
        public GameHistory()
        {
            Items = new []
            {
                new GameHistoryItem(new PreMoveInfo(0)),
            }.ToImmutableList();
        }
        
        public GameHistory(ImmutableList<GameHistoryItem> items)
        {
            Items = items;
        }
        
        public ImmutableList<GameHistoryItem> Items { get; }

        public GameHistoryItem Last => Items[^1];

        public GameHistory WithPreMove(PreMoveInfo preMoveInfo)
        {
            Debug.Assert(Items.Count == 0 || Last.IsComplete);
            return new GameHistory(Items.Add(new GameHistoryItem(preMoveInfo)));
        }

        public GameHistory WithAction(PlayerAction action, ImmutableArray<PlayerMessageElement> result)
        {
            Debug.Assert(Items.Count > 0 && !Last.IsComplete);
            return new GameHistory(Items.SetItem(Items.Count - 1, Last.WithAction(action, result)));
        }
    }
}