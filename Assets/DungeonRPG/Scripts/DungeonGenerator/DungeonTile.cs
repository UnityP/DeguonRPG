using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonRPG.RoguelikeGeneratorPro
{
    public interface IEntity
    {
        string Name { get; set; }
        bool IsSolid { get; set; }
        bool BlocksSight { get; set; }
        // ... other common properties/methods of game entities
    }

    public class DungeonTile
    {
        public Vector2Int Position { get; private set; }
        public ETileType TileType { get; set; }

        public Dictionary<EEightDirection, DungeonTile> Neighbors { get; private set; }
        
        public bool IsExplored { get; set; }
        public bool IsVisible { get; set; }
        
        public IEntity Entity { get; set; }

        public bool IsWalkable => TileType != ETileType.Wall; 
        
        
        public DungeonTile(int x, int y, ETileType type)
        {
            Position = new Vector2Int(x, y);
            TileType = type;

            Neighbors = new Dictionary<EEightDirection, DungeonTile>();
            IsExplored = false;
            IsVisible = false;
        }

        public void SetNeighbor(EEightDirection direction, DungeonTile neighbor)
        {
            Neighbors[direction] = neighbor;
        }
        
        public DungeonTile GetNeighborTile(EEightDirection direction)
        {
            if (Neighbors.TryGetValue(direction, out DungeonTile neighbor))
            {
                return neighbor;
            }
            return null;  // Or throw an exception, or handle this case however you like
        }
        
        public Dictionary<EEightDirection, DungeonTile> GetNeighborTypeTiles(ETileType tileType)
        {
            Dictionary<EEightDirection, DungeonTile> neighborTiles = new Dictionary<EEightDirection, DungeonTile>();
            
            foreach (EEightDirection direction in Enum.GetValues(typeof(EEightDirection)))
            {
                DungeonTile neighbor = GetNeighborTile(direction);
                if (neighbor != null && neighbor.TileType == tileType)
                {
                    neighborTiles.Add(direction,neighbor);
                }
            }
            return neighborTiles;
        }
    }
}




/* 추가 될 기능이나 수정 될 기능들
 *  public delegate void TileChanged(DungeonTile tile);

        public event TileChanged OnExplored;
        public event TileChanged OnVisibilityChanged;
        public event TileChanged OnEntityEntered;
        public event TileChanged OnEntityExited;
        
        
         public void Enter(IEntity entity)
        {
            this.Entity = entity;
            OnEntityEntered?.Invoke(this);
        }

        public IEntity Leave()
        {
            IEntity entity = this.Entity;
            this.Entity = null;
            OnEntityExited?.Invoke(this);
            return entity;
        }
 */