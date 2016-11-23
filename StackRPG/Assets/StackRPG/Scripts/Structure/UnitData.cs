using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class ValunceTable
{
    public int length;
    public int firstValue;
    public float modulusValue;
}

public class UnitData : ScriptableObject
{
    public Unit m_unitData;

    public ValunceTable m_upgradeCostValunceTable;
    public ValunceTable m_hpValunceTable;
    public ValunceTable m_damageValunceTable;
}
