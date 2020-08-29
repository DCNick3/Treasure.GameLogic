using System;
using Treasure.GameLogic.Message;
using Treasure.GameLogic.State.Field;

namespace Treasure.GameLogic.Tiles
{
    public class WaterTile : Tile
    {
        public Direction FlowDirection { get; }

        public WaterTile(Direction flowDirection)
        {
            FlowDirection = flowDirection;
        }

        public override void ApplyMove(AbstractGameField gameField, Point fromPosition, Direction direction,
            FieldVisitor fieldVisitor)
        {
            var position = (fromPosition + direction).WrapAroundIfNeeded(gameField);

            if (gameField.Parameters.WrapAround)
                position = position.WrapAround(gameField);

            for (var i = 0; i < 3; i++)
            {
                if (!gameField.Parameters.WrapAround && position.Y == gameField.Parameters.FieldHeight)
                {
                    fieldVisitor.Grate();
                    break;
                }
                fieldVisitor.Visit(position);
                position = position + ((WaterTile) gameField.GetTileAt(position)).FlowDirection;
                position = position.WrapAroundIfNeeded(gameField);
            }
        }

        public override Point Visit(AbstractGameField gameField, Point fromPosition, Point toPosition, PlayerMessageBuilder messageBuilder)
        {
            messageBuilder.Water();
            return toPosition;
        }
        
        public override string Stringify() => "W" + FlowDirection switch
        {
            Direction.Up => 'U', 
            Direction.Left => 'L',
            Direction.Down => 'D',
            Direction.Right => 'R', 
            _ => throw new ArgumentException()
        };
    }
}