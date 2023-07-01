using System.Collections.Generic;
using UnityEngine;

namespace DungeonRPG.RoguelikeGeneratorPro
{
    public interface IPathMaker
    {
        Vector2 Direction { get; }
        Vector2 Position { get; }
        List<Vector2> PathHistory { get; }
        void Turn();
        void Move();
        void UndoMove();
        bool ShouldBeDestroyed();
    }
    
    public class PathMaker :  IPathMaker
    {
        private static readonly Vector2[] Directions = new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        public RoguelikeGeneratorPro m_generatorPro;
    
        private Vector2 m_direction;
        
        private Vector2 m_position;

        private List<Vector2> m_pathHistory;

        public Vector2 Direction
        {
            get => m_direction;
            set
            {
                if (System.Array.IndexOf(Directions, value) == -1) 
                {
                    Debug.LogError("잘못된 방향:" + value);
                    return;
                }
                m_direction = value;
            }
        }
        
        public Vector2 Position
        {
            get => m_position;
            set
            {
                value.x = Mathf.Clamp(value.x, 1, m_generatorPro.levelSizeCut.x - 2);
                value.y = Mathf.Clamp(value.y, 1, m_generatorPro.levelSizeCut.y - 2);
                m_position = value;
            }
        }
    
        public List<Vector2> PathHistory => m_pathHistory;

        
        public PathMaker()
        {
            m_pathHistory = new List<Vector2>();
        }
        
        public PathMaker(Vector2 newPosition, Vector2 newDir, RoguelikeGeneratorPro roguelikeGeneratorPro)
        {
            m_position = newPosition;
            m_direction = newDir;
            m_generatorPro = roguelikeGeneratorPro;
            m_pathHistory = new List<Vector2>();
        }
        
        public PathMaker(PathMaker other)
        {
            m_position = other.m_position;
            m_direction = other.m_direction;
            m_generatorPro = other.m_generatorPro;
            m_pathHistory = new List<Vector2>();
        }

        public void Turn()
        {
            Direction = TurnDirection(Direction);
        }
        
        public Vector2 TurnDirection(Vector2 pathMakerDirection)
        {
            int randomValue = Random.Range(0, 100);
        
            int directionIndex = System.Array.IndexOf(Directions, pathMakerDirection);
        
            if (randomValue <= m_generatorPro.pathMakerRotatesLeft)
            {
                directionIndex = (directionIndex + 3) % 4; // Rotate left
            }
            else if (randomValue <= m_generatorPro.pathMakerRotatesLeft + m_generatorPro.pathMakerRotatesRight)
            {
                directionIndex = (directionIndex + 1) % 4; // Rotate right
            }
            else if (randomValue <= m_generatorPro.pathMakerRotatesLeft + m_generatorPro.pathMakerRotatesRight + m_generatorPro.pathMakerRotatesBackwords)
            {
                directionIndex = (directionIndex + 2) % 4; // Rotate backwards
            }
            else 
            {
                return pathMakerDirection; // No rotation
            }

            return Directions[directionIndex];
        }
        
        public void Move()
        {
            m_pathHistory.Add(Position);
            Position += Direction;
        }
        
        public void UndoMove()
        {
            if (m_pathHistory.Count > 0)
            {
                Position = m_pathHistory[m_pathHistory.Count - 1];
                m_pathHistory.RemoveAt(m_pathHistory.Count - 1);
            }
            else
            {
                Debug.LogError("이동을 취소할 수 없습니다. 경로 이동기록이 비어 있습니다.");
            }
        }
    
        public bool ShouldBeDestroyed()
        {
            return Random.Range(0, 100) < m_generatorPro.pathMakerDestructionChance;
        }

        public void GenerateBlock(int blockSize)
        {
            for (int x = 0; x < blockSize; x++)
            {
                for (int y = 0; y < blockSize; y++)
                {
                    var newX = (int)(Position.x + x);
                    var newY = (int)(Position.y + y);
                    if (newX > 0 && newX < m_generatorPro.tiles.GetLength(0) && newY > 0 && newY < m_generatorPro.tiles.GetLength(1))
                    {
                        m_generatorPro.tiles[newX, newY] = ETileType.floor; 
                    }
                }
            }
        }

        public void GenerateBlock2X2()
        {
            GenerateBlock(2);
        }

        public void GenerateBlock3X3()
        {
            GenerateBlock(3);
        }
        
    }
    
}

/*public class RandomPathMaker : IPathMaker
    {
       
    }
    
    public class StraightPathMaker : IPathMaker
    {
        
    }*/

/*
public void InitializeEvent(Vector2 position, Vector2 direction,RoguelikeGeneratorPro owner)
{
    // 방향이 미리 정의된 방향 중 하나인지 확인합니다.
    if (System.Array.IndexOf(Directions, direction) == -1) 
    {
        Debug.LogError("InitializeEvent에서 잘못된 방향:" + direction);
        return;
    }
    m_generatorPro = owner;
    Position = position;
    Direction = TurnPathMakers(direction);
}*/