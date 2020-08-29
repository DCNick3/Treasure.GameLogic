using System;
using Treasure.GameLogic.Message;
using Treasure.GameLogic.State.Field;
using Treasure.GameLogic.Tiles;

namespace Treasure.GameLogic
{
    public abstract class Tile
    {
        public abstract void ApplyMove(AbstractGameField gameField, Point fromPosition, Direction direction, FieldVisitor fieldVisitor);
        public abstract Point Visit(AbstractGameField gameField, Point fromPosition, Point toPosition, PlayerMessageBuilder messageBuilder);
        public abstract string Stringify();

        public static Tile ReadString(ref ReadOnlySpan<char> s)
        {
            var t = s[0];
            s = s[1..];

            int ReadInt(ref ReadOnlySpan<char> s)
            {
                string r = "";
                while (s.Length > 0 && s[0] >= '0' && s[0] <= '9')
                {
                    r += s[0];
                    s = s[1..];
                }

                return r.Length == 0 ? 0 : int.Parse(r);
            }
            switch (t)
            {
                case 'F':
                    return new FieldTile();
                case 'H':
                    return new HomeTile(ReadInt(ref s));
                case 'P':
                    return new PortalTile(ReadInt(ref s));
                case 'S':
                    return new SwampTile();
                case 'W':
                    var d = s[0];
                    s = s[1..];
                    return new WaterTile(d switch
                    {
                        'U' => Direction.Up,
                        'D' => Direction.Down,
                        'L' => Direction.Left,
                        'R' => Direction.Right,
                        _ => throw new ArgumentException($"Unknown direction type: {d}")
                    });
                default:
                    throw new ArgumentException($"Unknown tile type: {t}");
            }
        }
    }
}