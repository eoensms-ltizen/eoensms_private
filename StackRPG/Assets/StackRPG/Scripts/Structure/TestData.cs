using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class TestTeam
{
    public int m_teamID;
    public Point2D m_startPoint;
    public Color m_teamColor;
    public List<TestUnit> m_units;
}

[Serializable]
public class TestUnit
{
    public UnitData m_unitData;
    public int m_count;
    public int m_level;
}

public class TestData : ScriptableObject
{
    public List<TestTeam> m_teams;
}
