using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Treasure.GameLogic.Tiles;

namespace Treasure.GameLogic.State.Field
{
    public class GameFieldGenerator
    {
        private const int AttemptCount = 100;
        private const double WallGenerationChance = 0.1;
        
        private readonly GameFieldParams _parameters;
        private readonly Random _rnd;
        private GameFieldBuilder _candidateField;


        private int Width => _parameters.FieldWidth;
        private int Height => _parameters.FieldHeight;
        
        private Point? _lastRiverTilePos;
        
        public GameFieldGenerator(GameFieldParams parameters, Random rnd)
        {
            _parameters = parameters;
            _rnd = rnd;
        }

        public GameField Generate()
        {
            for (var i = 0; i < AttemptCount; i++)
            {
                _candidateField = new GameFieldBuilder(_parameters);
                var acceptable = GenerateField() &&
                                 GenerateRiver() &&
                                 GenerateSwamps() &&
                                 GeneratePortals() &&
                                 GenerateHomes() &&
                                 GenerateWalls() &&
                                 GenerateTreasure() &&
                                 _candidateField.CheckDefinitelyPassable();
                
                if (acceptable)
                    return _candidateField.Build();
            }

            throw new FieldGenerationException("Exceeded number of generation attempts");
        }

        private bool GenerateField()
        {
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
                _candidateField.Tiles[i, j] = new FieldTile();

            return true;
        }

        private bool GenerateRiver()
        {
            if (_parameters.WrapAround)
            {
                _lastRiverTilePos = null;
                for (var i = 0; i < AttemptCount; i++)
                {                    
                    var p = new Point(_rnd.Next(Width), Height - 1);
                    var river = new List<(Point p, Direction d)>();
                    var water = new bool[Width, Height];
                    var direction = Direction.Down;
                    while (p.Y >= 0)
                    {
                        water[p.X, p.Y] = true;
                        var dir = new List<Direction> { Direction.Up };
                        if (river.Count == 0 || (p + Direction.Left).WrapAround(_candidateField) != river[^1].p)
                            dir.Add(Direction.Left);
                        if (river.Count == 0 || (p + Direction.Right).WrapAround(_candidateField) != river[^1].p)
                            dir.Add(Direction.Right);


                        var selectedDirection = dir[_rnd.Next(dir.Count)];
                        
                        river.Add((p, direction));
                        direction = selectedDirection.Reverse();
                        p += selectedDirection;
                        p = new Point((p.X % Width + Width) % Width, p.Y);
                    }
                    if (river.First().p.X != river.Last().p.X)
                        continue;
                    foreach (var r in river)
                    {
                        var count = 0;
                        foreach (var d in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                        {
                            var rr = (r.p + d).WrapAround(_candidateField);
                            if (water[rr.X, rr.Y]) count++;
                        }
                        if (count != 2)
                            goto Cont;
                    }

                    foreach (var (r, d) in river) 
                        _candidateField[r] = new WaterTile(d);
                    return true;
                    
                    Cont: ;
                }
            }
            else
            {
                var p = new Point(_rnd.Next(Width), Height - 1);
                var direction = Direction.Down;
                _lastRiverTilePos = p;
                while (p.Y >= 0)
                {
                    var dir = new List<Direction> { Direction.Up };
                    if (p.X != 0 && _candidateField[p.X - 1, p.Y] is FieldTile && 
                        (p.Y == Height - 1 || !(_candidateField[p.X - 1, p.Y + 1] is WaterTile)))
                        dir.Add(Direction.Left);
                    if (p.X != Width - 1 && _candidateField[p.X + 1, p.Y] is FieldTile && 
                        (p.Y == Height - 1 || !(_candidateField[p.X + 1, p.Y + 1] is WaterTile)))
                        dir.Add(Direction.Right);
                    var selectedDirection = dir[_rnd.Next(dir.Count)];
                    _candidateField[p] = new WaterTile(direction);
                    direction = selectedDirection.Reverse();
                    p += selectedDirection;
                }
                return true;
            }
            return false;
        }
        
        
        private bool GenerateSwampBlock()
        {
            var swamp = new List<Point>();
            var candidates = new List<Point>();

            // helper functions follow

            bool HaveSwampsAround(Point p)
            {
                foreach (var d in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    var pp = p + d;
                    pp = pp.WrapAroundIfNeeded(_candidateField);
                    if (IsPointInside(pp) && _candidateField[pp] is SwampTile)
                        return true;
                }
                return false;
            }

            Point? FindPointWithoutSwampsAround()
            {
                for (var i = 0; i < AttemptCount; i++)
                {
                    var p = new Point(_rnd.Next(Width), _rnd.Next(Height));
                    if (_candidateField[p] is FieldTile && !HaveSwampsAround(p))
                        return p;
                }

                return null;
            }

            // finds such field neighbours, that does not have swamps around it
            void FindCandidates(Point p)
            {
                foreach (var d in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    var neigh = p + d;

                    neigh = neigh.WrapAroundIfNeeded(_candidateField);

                    if (!IsPointInside(neigh))
                        continue;

                    if (!(_candidateField[neigh] is FieldTile))
                        continue;

                    if (swamp.Contains(neigh) || candidates.Contains(neigh))
                        continue;

                    if (!HaveSwampsAround(neigh))
                        candidates.Add(neigh);
                }
            }

            bool TryGenerateBlock()
            {
                swamp.Clear();
                Point p;
                {
                    var pp = FindPointWithoutSwampsAround();
                    if (pp == null)
                        return false;
                    p = pp.Value;
                }
                swamp.Add(p);

                FindCandidates(p);

                for (var i = 1; i < _parameters.SwampSize; i++)
                {
                    if (candidates.Count == 0)
                        // No more candidates. Try again
                        return false;

                    // Select next candidate and set it to swamp
                    p = candidates[_rnd.Next(candidates.Count)];
                    candidates.Remove(p);
                    swamp.Add(p);

                    // Update candidate list
                    FindCandidates(p);
                }
                
                // Apply results
                foreach (var s in swamp)
                    _candidateField[s] = new SwampTile();
                return true;
            }
            
            for (var j = 0; j < AttemptCount; j++)
                if (TryGenerateBlock())
                    return true;
            return false;
        }

        private bool GenerateSwamps()
        {
            for (var k = 0; k < _parameters.SwampCount; k++)
                if (!GenerateSwampBlock())
                    return false;
            return true;
        }

        private bool GeneratePortals()
        {
            for (var i = 0; i < _parameters.PortalCount; i++)
            {
                var p = TryFindField();
                if (p == null)
                    return false;
                _candidateField[p.Value] = new PortalTile(i);
                _candidateField.PortalPositions[i] = p.Value;
            }
            return true;
        }

        private bool GenerateHomes()
        {
            var homes = new List<Point>();

            bool GenerateHome(int playerIndex)
            {
                for (var j = 0; j < AttemptCount; j++)
                {
                    var pp = TryFindField();
                    if (pp == null)
                        return false;
                    var p = pp.Value;
                    
                    if (homes.Any(h => Math.Min(Math.Abs(h.X - p.X), Width - Math.Abs(h.X - p.X)) 
                        + Math.Min(Math.Abs(h.Y - p.Y), Height - Math.Abs(h.Y - p.Y)) <= 2))
                        continue;

                    homes.Add(p);
                    _candidateField.HomePositions[playerIndex] = p;
                    Tile t = new HomeTile(playerIndex);
                    _candidateField[p] = t;
                    return true;
                }

                return false;
            }
            
            for (var i = 0; i < _parameters.PlayerCount; i++)
                if (!GenerateHome(i))
                    return false;
            return true;
        }
        
        private bool GenerateWalls()
        {
            bool ShouldCreateWallBetween(Point p1, Point p2)
            {
                var t1 = _candidateField[p1];
                var t2 = _candidateField[p2];
                    
                // won't create walls between swamps and water
                if (t1 is SwampTile && t2 is SwampTile || t1 is WaterTile && t2 is WaterTile)
                    return false;
                return _rnd.NextDouble() < _parameters.WallChance;
            }
            
            // TODO: generation of walls on the border if WrapAround mode is on
            for (var i = 0; i < Width; i++)
            for (var j = 0; j < Height; j++)
            {
                if (i > 0)
                {
                    // try create vertical wall
                    var type = WallType.None;
                    
                    var p1 = new Point(i - 1, j);
                    var p2 = p1 + Direction.Right;
                    if (ShouldCreateWallBetween(p1, p2))
                        type = WallType.Breakable;

                    _candidateField.VerticalWalls[i, j] = type;
                }

                if (j > 0)
                {
                    // try create horizontal wall
                    var type = WallType.None;
                    
                    var p1 = new Point(i, j - 1);
                    var p2 = p1 + Direction.Down;
                    if (ShouldCreateWallBetween(p1, p2))
                        type = WallType.Breakable;

                    _candidateField.HorizontalWalls[i, j] = type;
                }
            }
            
            if (!_parameters.WrapAround)
            {
                for (var i = 0; i < Width; i++)
                {
                    _candidateField.HorizontalWalls[i, 0] = WallType.Unbreakable;
                    _candidateField.HorizontalWalls[i, Height] = WallType.Unbreakable;
                }
                for (var j = 0; j < Height; j++)
                {
                    _candidateField.VerticalWalls[0, j] = WallType.Unbreakable;
                    _candidateField.VerticalWalls[Width, j] = WallType.Unbreakable;
                }

                Debug.Assert(_lastRiverTilePos != null, nameof(_lastRiverTilePos) + " != null");
                
                var (rx, ry) = _lastRiverTilePos.Value.XY;
                _candidateField.HorizontalWalls[rx, ry + 1] = WallType.Grate;
            }
            return true;
        }

        private bool GenerateTreasure()
        {
            var p = TryFindField();
            if (p == null)
                return false;

            _candidateField.SetTreasurePosition(p.Value);
            
            return true;
        }
        
        
        public Point? TryFindField()
        {
            for (var i = 0; i < AttemptCount; i++)
            {
                var p = new Point(_rnd.Next(Width), _rnd.Next(Height));
                if (_candidateField[p] is FieldTile)
                    return p;
            }

            return null;
        }
        
        public bool IsPointInside(Point p)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;
        }
    }

    class FieldGenerationException : Exception
    {
        public FieldGenerationException()
        {
        }

        protected FieldGenerationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public FieldGenerationException(string message) : base(message)
        {
        }

        public FieldGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}