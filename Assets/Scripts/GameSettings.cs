using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    [Header("Match 3 Settings")]

    public int BoardSizeX = 5;

    public int BoardSizeY = 5;

    public int MatchesMin = 3;

    public int LevelMoves = 16;

    public float LevelTime = 30f;

    public float TimeForHint = 5f;

    [Header("Mahjong Settings")]

    public int MahjongSizeX = 6;

    public int MahjongSizeY = 4;

    public int MahjongSizeBottom = 5;

    public int MahjongMatches = 3;

    public float MahjongTimeAttackDuration = 60f;
}
