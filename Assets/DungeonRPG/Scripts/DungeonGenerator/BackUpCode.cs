/*private static readonly Dictionary<EEightDirection, (int, int)> Directions = new Dictionary<EEightDirection, (int, int)>
        {
            { EEightDirection.Right, (1, 0) },
            { EEightDirection.TopRight, (1, 1) },
            { EEightDirection.Top, (0, 1) },
            { EEightDirection.TopLeft, (-1, 1) },
            { EEightDirection.Left, (-1, 0) },
            { EEightDirection.BottomLeft, (-1, -1) },
            { EEightDirection.Bottom, (0, -1) },
            { EEightDirection.BottomRight, (1, -1) }
        };
        

private Dictionary<EDirectionPattern, Tile> wallPatternToTileDictionary;
{ WallPattern.Top, floorTile_C1 },
{ WallPattern.Bottom, floorTile_C2 },
{ WallPattern.Left, floorTile_C3 },
{ WallPattern.Right, floorTile_C4 },
.....
 
        
private Dictionary<EDirectionPattern, Action> wallPatternActions = new Dictionary<EDirectionPattern, Action>()
{
    { EDirectionPattern.None, HandleNoWallsPattern },
    { EDirectionPattern.TopLeft, HandleTopLeftPattern },
    { EDirectionPattern.TopRight, HandleTopRightPattern },
    // Define other patterns and their corresponding actions
};


   private EDirectionPattern ConvertDirectionToPattern(EEightDirection direction)
        {
            // 주어진 방향 값에 해당하는 패턴 값을 반환합니다.
            switch (direction)
            {
                case EEightDirection.Top: return EDirectionPattern.Top;
                case EEightDirection.Left: return EDirectionPattern.Left;
                case EEightDirection.Bottom: return EDirectionPattern.Bottom;
                case EEightDirection.Right: return EDirectionPattern.Right;
                case EEightDirection.TopLeft: return EDirectionPattern.TopLeft;
                case EEightDirection.TopRight: return EDirectionPattern.TopRight;
                case EEightDirection.BottomLeft: return EDirectionPattern.BottomLeft;
                case EEightDirection.BottomRight: return EDirectionPattern.BottomRight;
                default: throw new ArgumentException("방향 값이 잘못되었습니다.", nameof(direction));
            }
        }
        
        private EDirectionPattern GetDirectionTilePattern(DungeonTile tile,ETileType tileType)
        {
            EDirectionPattern dirPattern = EDirectionPattern.None;
            
            foreach (EEightDirection direction in Enum.GetValues(typeof(EEightDirection)))
            {
                DungeonTile neighbor = tile.GetNeighbor(direction);
                if (neighbor != null && neighbor.TileType == tileType)
                {
                    dirPattern |= ConvertDirectionToPattern(direction);
                }
            }
            return dirPattern;
        }
        
        bool ContainsDirectionPatterns(EDirectionPattern dirPattern, params EDirectionPattern[] directions)
        {
            EDirectionPattern combinedDirections = directions.Aggregate(EDirectionPattern.None, (current, direction) => current | direction);
            return (dirPattern & combinedDirections) == combinedDirections;
        }
        
        bool NotContainsDirectionPatterns(EDirectionPattern dirPattern, params EDirectionPattern[] directions)
        {
            EDirectionPattern combinedDirections = directions.Aggregate(EDirectionPattern.None, (current, direction) => current | direction);
            return (dirPattern & combinedDirections) == EDirectionPattern.None;
        }

*/