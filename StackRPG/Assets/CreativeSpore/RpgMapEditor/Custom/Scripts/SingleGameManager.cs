using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using stackRPG;

public class SingleGameManager : Singleton<SingleGameManager>
{
    // SIngleLevelDesign 데이터로, 스테이지 고르고, 적유닛들 생성해줌, 돈지급
    public int m_stageNumber = -1;
    public List<MapData> m_maps;
    public List<int> m_rewordGold;
    public AIUserData m_AI;
    public List<UserData> m_userTable;
    public float m_goldPercentage;

    public Map m_currentMap { get { return m_maps[m_stageNumber].m_map; } }   
    public AIUser m_currentAI { get { return m_AI == null ? null : m_AI.m_aI; } }
    public int m_currentRewordGold { get { return m_rewordGold[m_stageNumber]; } }
    public User m_currentUser { get { return m_userTable[m_stageNumber].m_user; } }
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
        if (m_stageNumber + 1 >= m_userTable.Count) return false;

        m_stageNumber += 1;
        return true;
    }
}

