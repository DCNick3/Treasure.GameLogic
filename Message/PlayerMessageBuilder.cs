using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Treasure.GameLogic.Message
{
    public class PlayerMessageBuilder
    {
        private PlayerMessageElement.ElementType? _type;
        private int _intParam;
        private bool _withTreasure;

        private readonly List<PlayerMessageElement> _elements = new List<PlayerMessageElement>();
        
        private void Flush()
        {
            if (_type != null)
            {
                switch (_type)
                {
                    case PlayerMessageElement.ElementType.River:
                    case PlayerMessageElement.ElementType.Field:
                    case PlayerMessageElement.ElementType.Swamp:
                    case PlayerMessageElement.ElementType.Wall:
                    case PlayerMessageElement.ElementType.Grate:
                        _elements.Add(new PlayerMessageElement(_type.Value, 0, _withTreasure));
                        break;
                    case PlayerMessageElement.ElementType.Home:
                    case PlayerMessageElement.ElementType.Portal:
                        _elements.Add(new PlayerMessageElement(_type.Value, _intParam, _withTreasure));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _type = null;
            _intParam = 0;
            _withTreasure = false;
        }
        public void Field()
        {
            Flush();
            _type = PlayerMessageElement.ElementType.Field;
        }
        public void Water()
        {
            Flush();
            _type = PlayerMessageElement.ElementType.River;
        }
        public void Swamp()
        {
            Flush();
            _type = PlayerMessageElement.ElementType.Swamp;
        }
        public void Home(in int playerIndex)
        {
            Flush();
            _type = PlayerMessageElement.ElementType.Home;
            _intParam = playerIndex;
        }
        public void Portal(in int portalIndex)
        {
            Flush();
            _type = PlayerMessageElement.ElementType.Portal;
            _intParam = portalIndex;
        }
        
        public void Wall()
        {
            Flush();
            _type = PlayerMessageElement.ElementType.Wall;
        }
        public void Grate()
        {
            Flush();
            _type = PlayerMessageElement.ElementType.Grate;
        }

        public void WithTreasure()
        {
            _withTreasure = true;
        }

        public ImmutableArray<PlayerMessageElement> Build()
        {
            Flush();
            return _elements.ToImmutableArray();
        }
    }
}