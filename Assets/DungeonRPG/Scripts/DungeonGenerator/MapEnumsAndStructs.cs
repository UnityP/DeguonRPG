using System;


namespace DungeonRPG.RoguelikeGeneratorPro
{
    
    public enum ETileType
    {
        Empty, Floor, Wall, Detail
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
    
    [Flags]
    public enum EDirectionPattern
    {
        None = 0,
        Top = 1 << 0,
        Left = 1 << 1,
        Bottom = 1 << 2,
        Right = 1 << 3,
        TopLeft = 1 << 4,
        TopRight = 1 << 5,
        BottomLeft = 1 << 6,
        BottomRight = 1 << 7
    }



}