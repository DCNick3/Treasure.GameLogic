using System;

namespace Treasure.GameLogic.Message
{
    public readonly struct PlayerMessageElement
    {
        public PlayerMessageElement(ElementType type, int intParam, bool withTreasure)
        {
            Type = type;
            IntParam = intParam;
            WithTreasure = withTreasure;
        }

        public ElementType Type { get; }
        public int IntParam { get; }
        public bool WithTreasure { get; }

        public override string ToString()
        {
            var baseMessage = Type switch
            {
                ElementType.Field => "Field",
                ElementType.River => "River",
                ElementType.Swamp => "Swamp",
                ElementType.Home => $"Home {IntParam + 1}",
                ElementType.Portal => $"Portal {IntParam + 1}",
                ElementType.Wall => "Wall",
                ElementType.Grate => "Grate",
                _ => throw new ArgumentOutOfRangeException()
            };

            if (WithTreasure)
                return baseMessage + " with Treasure";
            return baseMessage;
        }
        

        public enum ElementType
        {
            Field  = 0,
            River  = 1,
            Swamp  = 2,
            Home   = 3,
            Portal = 4,
        
            Wall   = 5,
            Grate  = 6,
        }
    }
}