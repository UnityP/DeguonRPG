/*using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRPG.RoguelikeGeneratorPro
{
    public class TileManager : MonoBehaviour
    {
        private DungeonTile[,] _tiles;
        
        private static readonly Dictionary<EEightDirection, Vector2Int> _offsetsByDirection = new Dictionary<EEightDirection, Vector2Int>
        {
            { EEightDirection.Left, new Vector2Int(-1, 0) },
            { EEightDirection.Right, new Vector2Int(1, 0) },
            { EEightDirection.Top, new Vector2Int(0, 1) },
            { EEightDirection.Bottom, new Vector2Int(0, -1) },
            { EEightDirection.TopLeft, new Vector2Int(-1, 1) },
            { EEightDirection.BottomLeft, new Vector2Int(-1, -1) },
            { EEightDirection.TopRight, new Vector2Int(1, 1) },
            { EEightDirection.BottomRight, new Vector2Int(1, -1) },
        };
        
        public int GetLength(int dimension)
        {
            return _tiles.GetLength(dimension);
        }
        
        public void InitializeTileArray(int width, int height)
        {
            _tiles = new DungeonTile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _tiles[x, y] = new DungeonTile(x, y, ETileType.Empty);
                }
            }
        }
        
        public void ClearTileArray()
        {
            _tiles = null;
        }
        
        public DungeonTile GetTile(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _tiles.GetLength(0) && y < _tiles.GetLength(1))
            {
                return _tiles[x, y];
            }
            else
            {
                throw new ArgumentOutOfRangeException($"좌표({x}, {y})에 타일이 없습니다.");
            }
        }
        
        public void SetTile(int x, int y,ETileType newTileType)
        {
            if (x >= 0 && y >= 0 && x < _tiles.GetLength(0) && y < _tiles.GetLength(1))
            {
                _tiles[x, y].SetType(newTileType);
            }
        }
        
        public void SetTile(DungeonTile tile,ETileType newTileType)
        {
            if (tile != null)
            {
                SetTile(tile.Position.x, tile.Position.y, newTileType);
            }
        }

        public bool CheckTileType(int x, int y, ETileType type)
        {
            DungeonTile tile = GetTile(x, y);
            return tile != null && tile.TileType == type;
        }
        
        public DungeonTile GetNeighborTile(DungeonTile tile, EEightDirection direction)
        {
            Vector2Int offset = GetOffsetFromDirection(direction);
            return GetTile(tile.Position.x + offset.x, tile.Position.y + offset.y);
        }

        public bool CheckNeighborTileType(DungeonTile tile, EEightDirection direction, ETileType type)
        {
            DungeonTile neighborTile = GetNeighborTile(tile, direction);
            if (neighborTile != null && neighborTile.TileType == type) { return true; }
            return false;
        }
        
        public void SetNeighborTile(DungeonTile tile, EEightDirection direction, ETileType type)
        {
            DungeonTile neighborTile = GetNeighborTile(tile, direction);
            if (neighborTile != null) { neighborTile.SetType(type); }
        }
        
        public bool AllSurroundingTilesNotEmpty(int x, int y)
        {
            foreach (EEightDirection direction in Enum.GetValues(typeof(EEightDirection)))
            {
                Vector2Int offset = GetOffsetFromDirection(direction);
                if (CheckTileType(x + offset.x, y + offset.y, ETileType.Empty))
                {
                    return false;
                }
            }
            return true;
        }
        
        private Vector2Int GetOffsetFromDirection(EEightDirection direction)
        {
            return _offsetsByDirection.TryGetValue(direction, out var value) ? value : Vector2Int.zero;
        }
        
        public void GenerateBlock(DungeonTile tile, float chunkSpawnChance, float chunkChance2x2)
        {
            if (UnityEngine.Random.Range(0f, 1f) < chunkSpawnChance)
            {
                int size = UnityEngine.Random.Range(0f, 1f) < chunkChance2x2 ? 2 : 3;

                for (int offsetX = 0; offsetX < size; offsetX++)
                {
                    for (int offsetY = 0; offsetY < size; offsetY++)
                    {
                        SetTile(tile.Position.x + offsetX, tile.Position.y + offsetY, ETileType.Floor);
                    }
                }
            }
        }
        
    }
}*/