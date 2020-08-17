using System;
using System.Collections.Immutable;
using Treasure.GameLogic.Message;
using Treasure.GameLogic.State;

namespace Treasure.GameLogic.Controllers
{
    public class ConsoleNotifier : IPlayerNotifier
    {
        private string FormatMove(PlayerAction move)
        {
            var c = move.Direction switch
            {
                Direction.Up => "u",
                Direction.Left => "l",
                Direction.Down => "d",
                Direction.Right => "r",
                _ => throw new ArgumentException()
            };

            if (move.Type == PlayerAction.ActionType.Shoot)
                c = c.ToUpper();
            return c;
        }

        public void PreMove(int playerIndex, int lastRoundDeathCount)
        {
            if (lastRoundDeathCount > 0)
            {
                Console.WriteLine($"Player {playerIndex + 1} died {lastRoundDeathCount} times");
            }
        }

        public void PostMove(int playerIndex, PlayerAction move, ImmutableArray<PlayerMessageElement> message)
        {
            Console.WriteLine($"{playerIndex + 1}: {FormatMove(move)} -> {message.ConvertToString()}");
        }
    }
}