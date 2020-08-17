using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Collections.Extensions;
using Treasure.GameLogic.Message;

namespace Treasure.GameLogic.State.Field
{
    public abstract class AbstractGameField
    {
        protected AbstractGameField(GameFieldParams parameters)
        {
            Parameters = parameters;
        }

        public abstract Point TreasurePosition { get; }
        public GameFieldParams Parameters { get; }
        public int Width => Parameters.FieldWidth;
        public int Height => Parameters.FieldHeight;
        
        public bool IsPointInside(Point p)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;
        }

        public void CheckPointInside(Point p, string paramName)
        {
            if (!IsPointInside(p))
                throw new ArgumentException("does not belong to the field.", paramName);
        }

        public abstract Tile GetTileAt(int x, int y);
        public abstract WallType GetHorizontalWall(int wallX, int wallY);
        public abstract WallType GetVerticalWall(int wallX, int wallY);
        public abstract Point GetPortal(int index);
        public abstract Point GetHome(int index);

        public Tile GetTileAt(Point p) => GetTileAt(p.X, p.Y);
        
        public WallType GetWallAt(Point p, Direction direction)
        {
            var (x, y) = p.XY;
            return GetWallAt(x, y, direction);
        }

        public WallType GetWallAt(in int x, in int y, Direction direction)
        {
            return direction switch
            {
                Direction.Up => GetHorizontalWall(x, y),
                Direction.Left => GetVerticalWall(x, y),
                Direction.Down => GetHorizontalWall(x, y + 1),
                Direction.Right => GetVerticalWall(x + 1, y),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        public (WallType top, WallType left, WallType bottom, WallType right) GetWallsAt(Point p)
        {
            CheckPointInside(p, nameof(p));

            return (
                GetWallAt(p, Direction.Up), 
                GetWallAt(p, Direction.Left), 
                GetWallAt(p, Direction.Down),
                GetWallAt(p, Direction.Right)
            );
        }
        
        
        public (ImmutableArray<Point> path, Point newTreasurePosition, ImmutableArray<PlayerMessageElement> message) 
            ApplyMove(Point fromPosition, Direction direction)
        {
            var visitor = new FieldVisitor(this, fromPosition);
            var wall = GetWallAt(fromPosition, direction);
            if (wall != WallType.None)
            {
                if (wall == WallType.Grate)
                    visitor.Grate();
                else
                    visitor.Wall();
                return visitor.Finish();
            }

            var newPosition = (fromPosition + direction).WrapAroundIfNeeded(this);
            
            var t = GetTileAt(newPosition);
            t.ApplyMove(this, fromPosition, direction, visitor);

            return visitor.Finish();
        }

        public bool CheckDefinitelyPassable()
        {
            // first compute weakly-connected component where the treasure lies
            // then check that all homes belong to it
            // then check that it's actually a strongly-connected component (by counting from how many nodes one can get to treasure)
            // this makes sure that it's impossible to get stuck somewhere on the map

            var visitedNodes = new HashSet<Point>();
            // key - edge destination, value - edge source
            var edgesInverse = new MultiValueDictionary<Point, Point>();

            void AddNode(Point p)
            {
                visitedNodes.Add(p);
                foreach (var nei in Enum.GetValues(typeof(Direction)).Cast<Direction>()
                    .Select(d => ApplyMove(p, d).path[^1]).Where(_ => _ != p).Distinct())
                {
                    edgesInverse.Add(nei, p);
                    if (!visitedNodes.Contains(nei))
                        AddNode(nei);
                }
            }

            int CountAccessibleFrom(Point p)
            {
                var i = 1;

                visitedNodes.Add(p);
                
                if (edgesInverse.TryGetValue(p, out var sources))
                    foreach (var source in sources)
                        if (!visitedNodes.Contains(source))
                            i += CountAccessibleFrom(source);

                return i;
            }
            
            AddNode(TreasurePosition);

            // check that homes are in weakly-connected component
            if (!Enumerable.Range(0, Parameters.PlayerCount).Select(GetHome).All(_ => visitedNodes.Contains(_)))
                return false;

            var weaklyConnectedComponentSize = visitedNodes.Count;
            visitedNodes.Clear();

            // count from how many nodes in the component we can get to the treasure by going backwards
            if (CountAccessibleFrom(TreasurePosition) != weaklyConnectedComponentSize)
                return false;

            return true;
        }
    }
}