using System;
using System.Collections.Immutable;
using System.Linq;
using Treasure.GameLogic.Tiles;

namespace Treasure.GameLogic.State.Field
{
    public class GameFieldBuilder : AbstractGameField
    {
        private Point _treasurePosition;
        
        public Tile[,] Tiles { get; }
        public WallType[,] HorizontalWalls { get; }
        public WallType[,] VerticalWalls { get; }
        public Point[] PortalPositions { get; }
        public Point[] HomePositions { get; }

        public override Point TreasurePosition => _treasurePosition;
        
        public override Tile GetTileAt(int x, int y)
        {
            return Tiles[x, y];
        }

        public override WallType GetHorizontalWall(int wallX, int wallY)
        {
            return HorizontalWalls[wallX, wallY];
        }

        public override WallType GetVerticalWall(int wallX, int wallY)
        {
            return VerticalWalls[wallX, wallY];
        }

        public override Point GetPortal(int index)
        {
            return PortalPositions[index];
        }

        public override Point GetHome(int index)
        {
            return HomePositions[index];
        }

        public GameFieldBuilder(GameFieldParams parameters) : base(parameters)
        {
            Tiles = new Tile[Width, Height];
            HorizontalWalls = new WallType[Width, Height + 1];
            VerticalWalls = new WallType[Width + 1, Height];
            PortalPositions = new Point[Parameters.PortalCount];
            HomePositions = new Point[Parameters.PlayerCount];
        }

        public void SetTreasurePosition(Point p)
        {
            _treasurePosition = p;
        }
        
        #region Validation

        public bool Validate()
        {
            // TODO: do actual validation
            return ValidateTiles();
        }

        private bool ValidateTiles()
        {
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
            {
                var t = Tiles[i, j];
                switch (t)
                {
                    case FieldTile _:
                    case SwampTile _:
                    case WaterTile _:
                    case PortalTile _:
                    case HomeTile _:
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        private bool ValidateCondensation()
        {
            throw new NotImplementedException();
        }

        #endregion

        public GameField Build()
        {
            return new GameField(Parameters, Tiles.Cast<Tile>().ToImmutableArray(),
                HorizontalWalls.Cast<WallType>().ToImmutableList(), 
                VerticalWalls.Cast<WallType>().ToImmutableList(),
                TreasurePosition,
                PortalPositions.ToImmutableArray(),
                HomePositions.ToImmutableArray());
        }

        public Tile this[Point point]
        {
            get => Tiles[point.X, point.Y];
            set => Tiles[point.X, point.Y] = value;
        }

        public Tile this[int x, in int y]
        {
            get => Tiles[x, y];
            set => Tiles[x, y] = value;
        }
    }
}