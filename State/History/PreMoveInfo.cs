namespace Treasure.GameLogic.State.History
{
    public class PreMoveInfo
    {
        public PreMoveInfo(int lastDeathCount)
        {
            LastDeathCount = lastDeathCount;
        }
        
        public int LastDeathCount { get; }
    }
}