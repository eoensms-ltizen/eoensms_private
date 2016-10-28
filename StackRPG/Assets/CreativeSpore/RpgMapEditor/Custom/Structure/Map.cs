﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using stackRPG;
using System;
using CreativeSpore.RpgMapEditor;

[Serializable]
public class StartingPoint
{
    public bool m_visible = true;
    public Color m_color = Color.green;
    public List<Vector2> m_positions = new List<Vector2>();
}



[Serializable]
public class Map
{
    public string m_name = "New Map";
    /// <summary>
    /// 맵데이터
    /// </summary>
    public AutoTileMapData m_autoTileMapData;
    public AutoTileset m_autoTileset;

    /// <summary>
    /// 유저당 생산유닛갯수, 스타팅포인트, 어택포인트
    /// </summary>
    public int m_canMakeUnitCount = 10;
    public Vector2 m_attackPoint;
    public List<StartingPoint> m_makeUnitPositions = new List<StartingPoint>();
}
