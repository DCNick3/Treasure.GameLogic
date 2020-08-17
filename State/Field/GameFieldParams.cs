namespace Treasure.GameLogic.State.Field
{
    public class GameFieldParams
    {
        public GameFieldParams(int fieldWidth, int fieldHeight, int playerCount, int portalCount, 
            int swampCount, int swampSize, double wallChance, bool wrapAround)
        {
            FieldWidth = fieldWidth;
            FieldHeight = fieldHeight;
            PlayerCount = playerCount;
            PortalCount = portalCount;
            SwampCount = swampCount;
            SwampSize = swampSize;
            WallChance = wallChance;
            WrapAround = wrapAround;
        }

        public int FieldWidth { get; }
        public int FieldHeight { get; }
        public int PlayerCount { get; }
        public int PortalCount { get; }
        public int SwampCount { get; }
        public int SwampSize { get; }
        public double WallChance { get; }
        public bool WrapAround { get; }
    }
}