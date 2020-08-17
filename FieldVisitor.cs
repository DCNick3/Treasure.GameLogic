using System.Collections.Generic;
using System.Collections.Immutable;
using Treasure.GameLogic.Message;
using Treasure.GameLogic.State.Field;

namespace Treasure.GameLogic
{
    public class FieldVisitor
    {
        private readonly AbstractGameField _gameField;
        private readonly List<Point> _visitedPoints = new List<Point>();
        private readonly PlayerMessageBuilder _messageBuilder = new PlayerMessageBuilder();
        private Point _position;
        private Point _treasurePosition;

        public FieldVisitor(AbstractGameField gameField, Point initialPosition)
        {
            _gameField = gameField;
            _position = initialPosition;
            _treasurePosition = _gameField.TreasurePosition;
            _visitedPoints.Add(_position);
        }

        public void Visit(Point p)
        {
            var t = _gameField.GetTileAt(p);
            var newPosition = t.Visit(_gameField, _position, p, _messageBuilder);

            if (_treasurePosition == _position)
                _treasurePosition = newPosition;
            else if (newPosition == _treasurePosition) 
                _messageBuilder.WithTreasure();

            _position = newPosition;
            _visitedPoints.Add(newPosition);
        }

        public void Grate()
        {
            _messageBuilder.Grate();
        }

        public void Wall()
        {
            _messageBuilder.Wall();
        }

        public (ImmutableArray<Point> path, Point newTreasurePosition, ImmutableArray<PlayerMessageElement> message) Finish()
        {
            return (_visitedPoints.ToImmutableArray(), _treasurePosition, _messageBuilder.Build());
        }
    }
}