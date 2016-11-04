using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace stackRPG
{
    public class MUnitManager : Singleton<MUnitManager>
    {
        public List<UnitData> m_unitDatas = new List<UnitData>();

        public Dictionary<int, Unit> m_units { get; private set; }
        void Awake()
        {
            m_units = new Dictionary<int, Unit>();
            for (int i = 0; i < m_unitDatas.Count; ++i)
            {   
                m_units.Add(m_unitDatas[i].m_unitData.m_id, m_unitDatas[i].m_unitData);
            }
        }

        public MUnit GetMUnit(int id)
        {   
            MUnit munit = ((GameObject)Instantiate(m_units[id].m_prefab, Vector3.zero, Quaternion.identity)).GetComponent<MUnit>();            
            return munit;
        }

        public Unit GetUnit(int id)
        {
            if (m_units.ContainsKey(id) == false) return null;
            return m_units[id];
        }
        
    }
}

