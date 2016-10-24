using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct Unit
{
    public string m_characterId;
    public string m_name;
    public int m_price;
    public int m_level;

    public Unit(string characterId, string name, int price, int level)
    {
        m_characterId = characterId;
        m_name = name;        
        m_price = price;
        m_level = level;
    }
}
