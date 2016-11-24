using UnityEngine;
using System;

[Serializable]
public class AbilityData
{
    public AbilityType m_abilityType;
    public int m_level; 
    
    public AbilityData()
    {

    }

    public AbilityData(AbilityType abilityType, int level)
    {
        m_abilityType = abilityType;
        m_level = level;
    }
}
