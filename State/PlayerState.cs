namespace Treasure.GameLogic.State
{
    public class PlayerState
    {
        public Point Position { get; }
        public int LastRoundDeathCount { get; }

        public PlayerState(Point position, int lastRoundDeathCount)
        {
            Position = position;
            LastRoundDeathCount = lastRoundDeathCount;
        }

        public PlayerState(Point position) : this(position, 0)
        {
        }
    }
}