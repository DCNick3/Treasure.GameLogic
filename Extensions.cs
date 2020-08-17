using System;
using System.Collections.Immutable;
using System.Linq;
using Treasure.GameLogic.Message;

namespace Treasure.GameLogic
{
    public static class Extensions
    {
        public static Direction Reverse(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Right:
                    return Direction.Left;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static string ConvertToString(this ImmutableArray<PlayerMessageElement> message)
        {
            return string.Join(" - ", message.Select(_ => _.ToString()));
        }
    }
}