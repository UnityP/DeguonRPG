using System.Collections.Generic;
using UnityEngine;

namespace DungeonRPG.RoguelikeGeneratorPro
{
    public interface IPathMaker
    {
        Vector2Int Direction { get; }
        Vector2Int Position { get; }
        List<Vector2Int> PathHistory { get; }
        void Turn();
        void Move();
        void UndoMove();
        bool ShouldBeDestroyed();
    }
    
    public class PathMaker : IPathMaker
    {
        private static readonly Vector2Int[] Directions = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        private readonly RoguelikeGeneratorPro m_generatorPro;
    
        private Vector2Int m_direction;
        
        private Vector2Int m_position;

        private List<Vector2Int> m_pathHistory;
        
        public Vector2Int Direction
        {
            get => m_direction;
            private set
            {
                if (System.Array.IndexOf(Directions, value) == -1) 
                {
                    Debug.LogError("잘못된 방향:" + value);
                    return;
                }
                m_direction = value;
            }
        }
        
        public Vector2Int Position
        {
            get => m_position;
            private set
            {
                value.x = Mathf.Clamp(value.x, 1, m_generatorPro.levelSizeCut.x - 2);
                value.y = Mathf.Clamp(value.y, 1, m_generatorPro.levelSizeCut.y - 2);
                m_position = value;
            }
        }
    
        public List<Vector2Int> PathHistory => m_pathHistory;

        public PathMaker()
        {
        }
        
        public PathMaker(PathMaker other)
        {
            m_position = other.m_position;
            m_direction = other.m_direction;
            m_generatorPro = other.m_generatorPro;
            m_pathHistory = new List<Vector2Int>();
        }
        
        public void Turn()
        {
            Direction = TurnDirection(Direction);
        }
        
        private Vector2Int TurnDirection(Vector2Int pathMakerDirection)
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