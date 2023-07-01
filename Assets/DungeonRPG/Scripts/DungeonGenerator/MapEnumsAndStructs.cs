using UnityEngine;

namespace DungeonRPG.RoguelikeGeneratorPro
{
    
    public enum ETileType
    {
        empty, floor, wall, detail
    };

    public enum EOverlayType
    {
        empty, wallPattern, wallRandom, floorPattern, floorRandom
    };

    public enum EPatternType
    {
        perlinNoise, checker, wideChecker, lineLeft, lineRight
    };

    public enum levelRotation
    {
        XZ, XY , ZY
    };

    public enum EGenType
    {
        generateObj, generateTile, noGeneration
    };

    public struct pathMaker
    {
        public Vector2 direction; public Vector2 position;
    };
    
    public enum EEightDirection
    {
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight
    }
        

    
}