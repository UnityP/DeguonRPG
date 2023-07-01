using UnityEngine;

namespace DungeonRPG.RoguelikeGeneratorPro
{
    public class DungeonTile
    {
        public Vector2Int Position { get; private set; }
        public ETileType ETileType { get; private set; }
        
        public DungeonTile(int x, int y, ETileType type)
        {
            Position = new Vector2Int(x, y);
            ETileType = type;
        }

        public void SetType(ETileType type)
        {
            ETileType = type;
        }
        
    }
}