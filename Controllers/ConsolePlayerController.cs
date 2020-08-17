using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Treasure.GameLogic.State;
using Treasure.GameLogic.State.Field;
using Treasure.GameLogic.State.History;

namespace Treasure.GameLogic.Controllers
{
    public class ConsolePlayerController : IPlayerController
    {
        public void OnGameStart(int yourIndex, GameFieldParams fieldParams)
        { }

        public Task<PlayerAction> GetActionAsync(int yourIndex, CancellationToken cancellationToken, GameHistory history)
        {
            while (true)
            {
                Console.Write($"{yourIndex + 1}{string.Join(" ", Enumerable.Repeat("DEAD", history.Last.PreMoveInfo.LastDeathCount))}> ");
                var input = Console.ReadLine();
                PlayerAction action;
                switch (input)
                {
                    case "u":
                        action = new PlayerAction(PlayerAction.ActionType.Move, Direction.Up);
                        break;
                    case "d":
                        action = new PlayerAction(PlayerAction.ActionType.Move, Direction.Down);
                        break;
                    case "l":
                        action = new PlayerAction(PlayerAction.ActionType.Move, Direction.Left);
                        break;
                    case "r":
                        action = new PlayerAction(PlayerAction.ActionType.Move, Direction.Right);
                        break;
                    case "U":
                        action = new PlayerAction(PlayerAction.ActionType.Shoot, Direction.Up);
                        break;
                    case "D":
                        action = new PlayerAction(PlayerAction.ActionType.Shoot, Direction.Down);
                        break;
                    case "L":
                        action = new PlayerAction(PlayerAction.ActionType.Shoot, Direction.Left);
                        break;
                    case "R":
                        action = new PlayerAction(PlayerAction.ActionType.Shoot, Direction.Right);
                        break;
                    default:
                        continue;
                }

                return Task.FromResult(action);
            }
        }
    }
}