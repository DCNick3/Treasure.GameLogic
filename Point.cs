using System;
using Treasure.GameLogic.State.Field;

namespace Treasure.GameLogic
{
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public readonly int X;
        public readonly int Y;

        // ReSharper disable once InconsistentNaming
        public (int, int) XY => (X, Y);
        
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static Point operator +(Point a, Direction d)
        {
            switch (d)
            {
                case Direction.Up:
                    return new Point(a.X, a.Y - 1);
                case Direction.Left:
                    return new Point(a.X - 1, a.Y);
                case Direction.Down:
                    return new Point(a.X, a.Y + 1);
                case Direction.Right:
                    return new Point(a.X + 1, a.Y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(d), d, null);
            }
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }

            var point = (Point)obj;
            return X == point.X &&
                   Y == point.Y;
        }

        public Point WrapAround(int width,int height)
        {
            return new Point(((X % width) + width) % width, ((Y % height) + height) % height);
        }
        
        public Point WrapAround(AbstractGameField gameField) => WrapAround(gameField.Width, gameField.Height);

        public Point WrapAroundIfNeeded(AbstractGameField gameField)
        {
            return gameField.Parameters.WrapAround 
                ? WrapAround(gameField) 
                : this;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

    }
}