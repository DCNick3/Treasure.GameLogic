namespace Treasure.GameLogic.State
{
    public class PlayerAction
    {
        public PlayerAction(ActionType type, Direction direction)
        {
            Type = type;
            Direction = direction;
        }

        public ActionType Type { get; }
        public Direction Direction { get; }
        
        public enum ActionType
        {
            Move,
            Shoot,
        }
    }
}