using Treasure.GameLogic.Message;
using Treasure.GameLogic.State.Field;

namespace Treasure.GameLogic.Tiles
{
    public class FieldTile : Tile
    {
        public override void ApplyMove(AbstractGameField gameField, Point fromPosition, Direction direction,
            FieldVisitor fieldVisitor)
        {
            fieldVisitor.Visit((fromPosition + direction).WrapAroundIfNeeded(gameField));
        }

        public override Point Visit(AbstractGameField gameField, Point fromPosition, Point toPosition, PlayerMessageBuilder messageBuilder)
        {
            messageBuilder.Field();
            return toPosition;
        }
    }
}