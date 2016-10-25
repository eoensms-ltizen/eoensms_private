using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Unit
{
    public GameObject m_prefab;
    public GameObject m_attackPrefab;

    public int m_id;
    public string m_name;
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

    public float m_minDisToReachTarg;
}
