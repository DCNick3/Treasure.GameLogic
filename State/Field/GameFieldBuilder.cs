using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
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

        /// <summary>
        /// Validates the field
        /// Does check PortalPositions and HomePositions
        /// </summary>
        /// <returns>null if no validity problems found, error description otherwise</returns>
        public string? Validate()
        {
            if (!ValidateTiles()) return "Tiles are wrong";
            if (!ValidateRiver()) return "River is wrong";
            if (!ValidateSwamps()) return "Swamps are wrong";
            if (!ValidatePortals()) return "Portals are wrong";
            if (!ValidateHomes()) return "Homes are wrong";
            if (!ValidateWalls()) return "Walls are wrong";
            if (!ValidateTreasure()) return "Treasure is invalid";
            
            return null;
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

        private IEnumerable<Point> Neighbours(int i, int j)
        {
            var pps = new []
            {
                (i - 1, j), (i + 1, j), (i, j - 1), (i, j + 1)
            }.Select(_ => new Point(_.Item1, _.Item2));

            if (Parameters.WrapAround)
                pps = pps.Select(_ => _.WrapAroundIfNeeded(this));
            else
                pps = pps.Where(IsPointInside);

            return pps;
        }
        
        private bool ValidateRiver()
        {
            int GetNeighWaterCount(int i, int j)
            {
                // precondition: i and j are in field
                var pps = Neighbours(i, j);

                return pps.Count(_ => GetTileAt(_) is WaterTile);
            }
            
            var exceptions = new HashSet<(int, int)>();

            if (!Parameters.WrapAround)
            {
                var sources = new List<Point>();
                var sinks = new List<Point>();
                for (var i = 0; i < Width; i++)
                {
                    if (GetTileAt(i, 0) is WaterTile && GetNeighWaterCount(i, 0) == 1)
                        sources.Add(new Point(i, 0));
                    if (GetTileAt(i, Height - 1) is WaterTile && GetNeighWaterCount(i, Height - 1) == 1)
                        sinks.Add(new Point(i, Height - 1));
                }

                if (sources.Count != 1 || sinks.Count != 1)
                    return false;
                
                var sink = sinks.Single();
                var source = sources.Single();

                exceptions.Add(sink.XY);
                exceptions.Add(source.XY);
            }

            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
                if (GetTileAt(i, j) is WaterTile && !exceptions.Contains((i, j)))
                    if (GetNeighWaterCount(i, j) != 2)
                        return false;

            return true;
        }

        private bool ValidateSwamps()
        {
            var seen = new HashSet<(int, int)>();
            var swamps = new List<(int, int)[]>();

            (int, int)[]? GetSwamp((int, int) startingPoint)
            {
                if (seen!.Contains(startingPoint))
                    return null;
                var list = new List<(int, int)>();

                void Go((int, int) p)
                {
                    seen!.Add(p);
                    if (GetTileAt(p.Item1, p.Item2) is SwampTile)
                    {
                        list!.Add(p);
                        foreach (var neighbour in Neighbours(p.Item1, p.Item2))
                            if (!seen.Contains(neighbour.XY))
                                Go(neighbour.XY);
                    }
                }
                Go(startingPoint);

                if (list.Count == 0)
                    return null;

                return list.ToArray();
            }
            
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
            {
                var s = GetSwamp((i, j));
                if (s != null)
                    swamps.Add(s);
            }

            if (swamps.Count != Parameters.SwampCount)
                return false;

            if (swamps.Any(_ => _.Length != Parameters.SwampSize))
                return false;

            return true;
        }

        private bool ValidatePortals()
        {
            var portals = new List<(int, int)>();
            
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
                if (GetTileAt(i, j) is PortalTile)
                    portals.Add((i, j));

            if (portals.Count != Parameters.PortalCount)
                return false;

            // Fill PortalPositions with correct data
            if (!portals.Select(_ => ((PortalTile) GetTileAt(new Point(_))).PortalNumber)
                .OrderBy(_ => _)
                .SequenceEqual(Enumerable.Range(0, Parameters.PortalCount)))
                return false;

            for (var i = 0; i < PortalPositions.Length; i++)
                if (GetTileAt(PortalPositions[i]) is PortalTile t)
                {
                    if (t.PortalNumber != i)
                        return false;
                }
                else
                    return false;

            return true;
        }

        private bool ValidateHomes()
        {
            var homes = new List<(int, int)>();
            
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
                if (GetTileAt(i, j) is HomeTile)
                    homes.Add((i, j));

            if (homes.Count != Parameters.PlayerCount)
                return false;

            // Fill PortalPositions with correct data
            if (!homes.Select(_ => ((HomeTile) GetTileAt(new Point(_))).PlayerIndex)
                .OrderBy(_ => _)
                .SequenceEqual(Enumerable.Range(0, Parameters.PlayerCount)))
                return false;

            for (var i = 0; i < HomePositions.Length; i++)
                if (GetTileAt(HomePositions[i]) is HomeTile t)
                {
                    if (t.PlayerIndex != i)
                        return false;
                }
                else
                    return false;

            return true;
        }

        private bool ValidateWalls()
        {
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
                foreach (var d in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    var wall = GetWallAt(i, j, d);

                    if (!Parameters.WrapAround &&
                        (i == 0 && d == Direction.Left || i == Width - 1 && d == Direction.Right ||
                         j == 0 && d == Direction.Up || j == Height - 1 && d == Direction.Down))
                    {
                        var grate = j == Height - 1 && d == Direction.Down && GetTileAt(i, j) is WaterTile &&
                                    Neighbours(i, j).Select(GetTileAt).Count(_ => _ is WaterTile) == 1;

                        if (grate)
                        {
                            if (wall != WallType.Grate)
                                return false;
                        }
                        else if (wall != WallType.Unbreakable)
                            return false;
                    }
                    else if (wall == WallType.Unbreakable || wall == WallType.Grate)
                        return false;
                }

            for (var i = 0; i < Width + 1; i++)
            for (var j = 0; j < Height; j++)
            {
                var wall = GetVerticalWall(i, j);
                if (wall != WallType.None)
                {
                    var p1 = new Point(i - 1, j).WrapAroundIfNeeded(this);
                    var p2 = new Point(i, j).WrapAroundIfNeeded(this);
                    if (IsPointInside(p1) && IsPointInside(p2))
                        // no walls between water
                        if (GetTileAt(p1) is WaterTile && GetTileAt(p2) is WaterTile)
                            return false;
                }
            }

            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height + 1; j++)
            {
                var wall = GetHorizontalWall(i, j);
                if (wall != WallType.None)
                {
                    var p1 = new Point(i, j - 1).WrapAroundIfNeeded(this);
                    var p2 = new Point(i, j).WrapAroundIfNeeded(this);
                    if (IsPointInside(p1) && IsPointInside(p2))
                        // no walls between water
                        if (GetTileAt(p1) is WaterTile && GetTileAt(p2) is WaterTile)
                            return false;
                }
            }

            return true;
        }

        private bool ValidateTreasure() => GetTileAt(TreasurePosition) is FieldTile;

        #endregion

        public void LoadString(string fieldString)
        {
            var parts = fieldString.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                throw new ArgumentException();
            var fieldData = parts[0].AsSpan();
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
            {
                var t = Tile.ReadString(ref fieldData);
                if (t is HomeTile home) 
                    HomePositions[home.PlayerIndex] = new Point(i, j);
                if (t is PortalTile portal) 
                    PortalPositions[portal.PortalNumber] = new Point(i, j);
                
                Tiles[i, j] = t;
            }

            var verticalWallsData = parts[1].AsSpan();
            for (var i = 0; i < Width + 1; i++)
            for (var j = 0; j < Height; j++)
            {
                VerticalWalls[i, j] = verticalWallsData[0] switch
                {
                    'N' => WallType.None,
                    'B' => WallType.Breakable,
                    'U' => WallType.Unbreakable,
                    'G' => WallType.Grate,
                    _ => throw new ArgumentException()
                };
                verticalWallsData = verticalWallsData[1..];
            }

            var horizontalWallsData = parts[2].AsSpan();
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height + 1; j++)
            {
                HorizontalWalls[i, j] = horizontalWallsData[0] switch
                {
                    'N' => WallType.None,
                    'B' => WallType.Breakable,
                    'U' => WallType.Unbreakable,
                    'G' => WallType.Grate,
                    _ => throw new ArgumentException()
                };
                horizontalWallsData = horizontalWallsData[1..];
            }

            var treasurePositionInts = parts[3].Split(' ').Select(int.Parse).ToArray();
            var treasurePosition = new Point(treasurePositionInts[0], treasurePositionInts[1]);
            CheckPointInside(treasurePosition, nameof(treasurePosition));
            SetTreasurePosition(treasurePosition);
        }
        
        public GameField Build()
        {
            var v = Validate();
            if (v != null)
                throw new Exception($"Field validation failed: {v}");
            
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