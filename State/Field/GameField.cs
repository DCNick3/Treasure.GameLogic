using System;
using System.Collections.Immutable;

namespace Treasure.GameLogic.State.Field
{
    public class GameField : AbstractGameField
    {
        public ImmutableArray<Tile> Tiles { get; }
        public ImmutableList<WallType> HorizontalWalls { get; }
        public ImmutableList<WallType> VerticalWalls { get; }
        public ImmutableArray<Point> PortalPositions { get; }
        public ImmutableArray<Point> HomePositions { get; }
        public override Point TreasurePosition { get; }

        internal GameField(GameFieldParams parameters, ImmutableArray<Tile> tiles, ImmutableList<WallType> horizontalWalls, 
            ImmutableList<WallType> verticalWalls, Point treasurePosition, ImmutableArray<Point> portalPositions, ImmutableArray<Point> homePositions) : base(parameters)
        {
            // Not much invariants are actually checked. That's why the constructor is internal
            if (tiles.Length != Height * Width)
                throw new ArgumentException("has invalid size.", nameof(tiles));
            if (horizontalWalls.Count != (Height + 1) * Width)
                throw new ArgumentException("has invalid size.", nameof(horizontalWalls));
            if (verticalWalls.Count != Height * (Width + 1))
                throw new ArgumentException("has invalid size.", nameof(verticalWalls));
            
            Tiles = tiles;
            VerticalWalls = verticalWalls;
            HorizontalWalls = horizontalWalls;
            TreasurePosition = treasurePosition;
            PortalPositions = portalPositions;
            HomePositions = homePositions;
        }

        public override WallType GetHorizontalWall(int wallX, int wallY) => HorizontalWalls[wallY + wallX * (Height + 1)];
        public override WallType GetVerticalWall(int wallX, int wallY) => VerticalWalls[wallY + wallX * Height];
        public override Tile GetTileAt(int x, int y) => Tiles[y + x * Height];
        public override Point GetPortal(int index) => PortalPositions[index];
        public override Point GetHome(int index) => HomePositions[index];

        public GameField WithTreasurePosition(Point treasurePosition)
        {
            if (treasurePosition != TreasurePosition)
                return new GameField(Parameters, Tiles, HorizontalWalls, VerticalWalls, treasurePosition, PortalPositions, HomePositions);
            return this;
        }

        public GameField WithDestroyedWall(int wallX, int wallY, bool isHorizontal)
        {
            var horizontalWalls = HorizontalWalls;
            var verticalWalls = VerticalWalls;

            if (isHorizontal)
                horizontalWalls = horizontalWalls.SetItem(wallY + wallX * (Height + 1), WallType.None);
            else
                verticalWalls = verticalWalls.SetItem(wallY + wallX * Height, WallType.None);
            
            return new GameField(Parameters, Tiles, horizontalWalls, verticalWalls, TreasurePosition, PortalPositions, HomePositions);
        }

        public GameField WithDestroyedWall(Point p, Direction direction)
        {
            var (x, y) = p.XY;
            return direction switch
            {
                Direction.Up => WithDestroyedWall(x, y, true),
                Direction.Left => WithDestroyedWall(x, y, false),
                Direction.Down => WithDestroyedWall(x, y + 1, true),
                Direction.Right => WithDestroyedWall(x + 1, y, false),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}