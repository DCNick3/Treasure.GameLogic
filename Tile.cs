using Treasure.GameLogic.Message;
using Treasure.GameLogic.State.Field;

namespace Treasure.GameLogic
{
    public abstract class Tile
    {
        public abstract void ApplyMove(AbstractGameField gameField, Point fromPosition, Direction direction, FieldVisitor fieldVisitor);
        public abstract Point Visit(AbstractGameField gameField, Point fromPosition, Point toPosition, PlayerMessageBuilder messageBuilder);
    }
}