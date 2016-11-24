using UnityEngine;
using System.Collections.Generic;
using System;

public enum AbilityType
{
    Charisma,
    Lucky,
    Smart,
}

public enum ValueType
{
    FLOAT,
    INT,
    BOOL,
}

[Serializable]
public class UserAbility
{
    public AbilityType m_abilityType;
    public string m_name;
    public string m_info;    

    public List<int> m_upgradeCost;

    public ValueType m_valueType;

    public float m_valueFloat;
    public int m_valueInt;
    public bool m_valueBool;
}
