using Treasure.GameLogic.Message;
using Treasure.GameLogic.State.Field;

namespace Treasure.GameLogic.Tiles
{
    public class PortalTile : Tile
    {
        public int PortalNumber { get; }

        public PortalTile(int portalNumber)
        {
            PortalNumber = portalNumber;
        }

        public override void ApplyMove(AbstractGameField gameField, Point fromPosition, Direction direction,
            FieldVisitor fieldVisitor)
        {
            var position = (fromPosition + direction).WrapAroundIfNeeded(gameField);
            fieldVisitor.Visit(position);
            position = gameField.GetPortal((PortalNumber + 1) % gameField.Parameters.PortalCount);
            fieldVisitor.Visit(position);
        }

        public override Point Visit(AbstractGameField gameField, Point fromPosition, Point toPosition, PlayerMessageBuilder messageBuilder)
        {
            messageBuilder.Portal(PortalNumber);
            return toPosition;
        }
    }
}