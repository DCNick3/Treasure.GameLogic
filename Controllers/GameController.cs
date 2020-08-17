using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Treasure.GameLogic.State;
using Treasure.GameLogic.State.Field;

namespace Treasure.GameLogic.Controllers
{
    public class GameController
    {
        private readonly IEnumerable<IPlayerNotifier> _notifiers;
        private ImmutableArray<IPlayerController> _playerControllers;

        public GameController(GameFieldParams fieldParams, IEnumerable<IPlayerController> playerControllers,
            IEnumerable<IPlayerNotifier> notifiers) : this(new GameFieldGenerator(fieldParams, new Random()).Generate(),
            playerControllers,
            notifiers)
        {
        }
        public GameController(GameFieldParams fieldParams, Random rnd, IEnumerable<IPlayerController> playerControllers,
            IEnumerable<IPlayerNotifier> notifiers) : this(new GameFieldGenerator(fieldParams, rnd).Generate(),
            playerControllers,
            notifiers)
        {
        }

        public GameController(GameField field, IEnumerable<IPlayerController> playerControllers,
            IEnumerable<IPlayerNotifier> notifiers) : this(new GameState(field), playerControllers,
            notifiers)
        {
        }

        public GameController(GameState state, IEnumerable<IPlayerController> playerControllers,
            IEnumerable<IPlayerNotifier> notifiers)
        {
            _notifiers = notifiers;
            GameState = state;
            _playerControllers = playerControllers.ToImmutableArray();
            
            Debug.Assert(GameState.PlayerStates.Length == _playerControllers.Length);
        }
        
        public GameState GameState { get; private set; }

        public async Task<int> DoGameLoopAsync(CancellationToken cancellationToken)
        {
            for (var i = 0; i < _playerControllers.Length; i++)
                _playerControllers[i].OnGameStart(i, GameState.GameField.Parameters);
            
            while (GameState.Winner == null)
            {
                var currentPlayer = GameState.CurrentPlayer;
                var currentPlayerState = GameState.PlayerStates[currentPlayer];

                foreach (var notifier in _notifiers)
                    notifier.PreMove(currentPlayer, currentPlayerState.LastRoundDeathCount);
                
                
                var action = await _playerControllers[currentPlayer].GetActionAsync(currentPlayer, cancellationToken,
                    GameState.History);
                var (newGameState, message) = GameState.WithAppliedAction(action);

                foreach (var notifier in _notifiers)
                    notifier.PostMove(currentPlayer, action, message);

                GameState = newGameState;
            }

            return GameState.Winner.Value;
        }
        
    }
}