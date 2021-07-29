using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Treasure.GameLogic.Message;
using Treasure.GameLogic.State.Field;
using Treasure.GameLogic.State.History;

namespace Treasure.GameLogic.State
{
    public class GameState
    {
        public GameState(GameField gameField)
        {
            GameField = gameField;
            CurrentPlayer = 0;
            PlayerStates = Enumerable.Range(0, gameField.Parameters.PlayerCount)
                .Select(_ => new PlayerState(gameField.GetHome(_), 0)).ToImmutableArray();
            Winner = null;
            History = new GameHistory();
        }
        
        public GameState(int currentPlayer, GameField gameField, ImmutableArray<PlayerState> playerStates, int? winner, GameHistory history)
        {
            CurrentPlayer = currentPlayer;
            GameField = gameField;
            PlayerStates = playerStates;
            Winner = winner;
            History = history;

            Debug.Assert(PlayerStates.Length == gameField.Parameters.PlayerCount);
        }
        
        public int CurrentPlayer { get; }
        public GameField GameField { get; }
        public ImmutableArray<PlayerState> PlayerStates { get; }
        public int? Winner { get; }
        public GameHistory History { get; }

        private ImmutableArray<PlayerState> PropagateDeaths(List<int> deadPlayers, ImmutableArray<PlayerState> playerStates)
        {
            while (deadPlayers.Count > 0)
            {
                var deadCopy = deadPlayers.ToArray();
                deadPlayers.Clear();

                foreach (var i in deadCopy)
                {
                    playerStates = playerStates.SetItem(i,
                        new PlayerState(GameField.GetHome(i), PlayerStates[i].LastRoundDeathCount + 1));

                    deadPlayers.AddRange(
                        Enumerable
                            .Range(0, PlayerStates.Length)
                            .Where(_ => _ != i && playerStates[_].Position == GameField.GetHome(i)));
                }
            }

            return playerStates;
        }

        private GameHistory NewHistory(PlayerAction.ActionType actionType, Direction actionDirection,
            ImmutableArray<PlayerMessageElement>? result, int newCurrentPlayer)
        {
            return History.WithAction(new PlayerAction(actionType, actionDirection), 
                    result ?? Enumerable.Empty<PlayerMessageElement>().ToImmutableArray())
                .WithPreMove(new PreMoveInfo(PlayerStates[newCurrentPlayer].LastRoundDeathCount));
        }
        
        public (GameState, ImmutableArray<PlayerMessageElement>) WithAppliedMove(Direction direction)
        {
            var (path, newTreasurePosition, message) = 
                GameField.ApplyMove(PlayerStates[CurrentPlayer].Position, direction);

            var deadPlayers = Enumerable.Range(0, PlayerStates.Length)
                .Where(_ => _ != CurrentPlayer && path.Contains(PlayerStates[_].Position)).ToList();
            
            var newCurrentPlayerState = new PlayerState(path[^1], 0);
            var newPlayerStates = PlayerStates.SetItem(CurrentPlayer, newCurrentPlayerState);

            newPlayerStates = PropagateDeaths(deadPlayers, newPlayerStates);

            var newWinner = Enumerable
                .Range(0, PlayerStates.Length)
                .Where(_ => newPlayerStates[_].Position == GameField.GetHome(_) &&
                            newTreasurePosition == GameField.GetHome(_))
                .Cast<int?>()
                .SingleOrDefault();

            var newCurrentPlayer = (CurrentPlayer + 1) % PlayerStates.Length;

            var newHistory = NewHistory(PlayerAction.ActionType.Move, direction, message, newCurrentPlayer);
            
            return (new GameState(newCurrentPlayer, GameField.WithTreasurePosition(newTreasurePosition), newPlayerStates, 
                newWinner, newHistory), message);
        }

        public GameState WithAppliedShoot(Direction direction)
        {
            var deadPlayers = new List<int>();
            var newGameField = GameField;

            var bulletPosition = PlayerStates[CurrentPlayer].Position;
            
            while (true)
            {
                var wall = GameField.GetWallAt(bulletPosition, direction);
                if (wall != WallType.None)
                {
                    if (wall == WallType.Breakable)
                        newGameField = GameField.WithDestroyedWall(bulletPosition, direction);
                    break;
                }
                
                bulletPosition = (bulletPosition + direction).WrapAroundIfNeeded(newGameField);
                
                deadPlayers = Enumerable.Range(0, PlayerStates.Length)
                    .Where(_ => PlayerStates[_].Position == bulletPosition).ToList();
                if (deadPlayers.Count > 0)
                    break;
            }
            

            var newCurrentPlayerState = new PlayerState(PlayerStates[CurrentPlayer].Position, 0);
            var newCurrentPlayer = (CurrentPlayer + 1) % PlayerStates.Length;
            
            var newPlayerStates = PlayerStates.SetItem(CurrentPlayer, newCurrentPlayerState);

            newPlayerStates = PropagateDeaths(deadPlayers, newPlayerStates);
            
            var newHistory = NewHistory(PlayerAction.ActionType.Shoot, direction, null, newCurrentPlayer);
            
            return new GameState(newCurrentPlayer, newGameField, newPlayerStates, Winner, newHistory);
        }

        public (GameState, ImmutableArray<PlayerMessageElement>) WithAppliedAction(PlayerAction action)
        {
            return action.Type switch
            {
                PlayerAction.ActionType.Move => WithAppliedMove(action.Direction),
                PlayerAction.ActionType.Shoot => (WithAppliedShoot(action.Direction),
                    ImmutableArray<PlayerMessageElement>.Empty),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}