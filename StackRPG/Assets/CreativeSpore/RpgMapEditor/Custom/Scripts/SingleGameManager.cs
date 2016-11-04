using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SingleGameManager : Singleton<SingleGameManager>
{
    // SIngleLevelDesign 데이터로, 스테이지 고르고, 적유닛들 생성해줌, 돈지급
    public int m_stageNumber = -1;
    public List<MapData> m_maps;
    public List<int> m_rewordGold;
    public AIUserData m_enemy;
    public float m_goldPercentage;

    public Map m_currentMap { get { return m_maps[m_stageNumber].m_map; } }   
    public AIUser m_currentEnemy { get { return m_enemy == null ? null : m_enemy.m_aI; } }
    public int m_currentRewordGold { get { return m_rewordGold[m_stageNumber]; } }
    public int m_enemyGold
    {
        get
        {
            int gold = 0;
            for (int i = 0; i <= m_stageNumber; ++i)
            {
                gold += m_rewordGold[i];
            }
            return (int)(gold * m_goldPercentage);
        }
    }

    public bool NextStage()
    {   
        if (m_stageNumber + 1 >= m_maps.Count) return false;        
        if (m_stageNumber + 1 >= m_rewordGold.Count) return false;
        
        m_stageNumber += 1;
        return true;
    }
}

