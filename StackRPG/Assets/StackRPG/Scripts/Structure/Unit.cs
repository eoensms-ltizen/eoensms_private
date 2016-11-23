using UnityEngine;
using System.Collections;
using System;


public enum AttackType : int
{
    Normal = 0,
    Oscillatory,
    Explosive,
}

public enum ArmorType : int
{
    Small = 0,
    Medium,
    Large,
}

[Serializable]
public class Unit
{
    public GameObject m_prefab;    
    public GameObject m_attackPrefab;
    public GameObject m_deadPrefab;
    public GameObject m_levelUpPrefab;

    public int m_id;
    public string m_name;
    public string m_history;
    public int m_openPrice;
    public int m_makePrice;
    public int[] m_upgradeCost;
    public int[] m_hp;
    public int[] m_attackDamage;
    public float m_attackForce;
    public float m_moveSpeed;
    public float m_attackRange;
    public float m_attackCoolTime;
    public float m_attackHoldTime;

    public AttackType m_attackType;
    public ArmorType m_armorType;

    public float m_minDisToReachTarg;
}
