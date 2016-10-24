using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace stackRPG
{
    public class MUnitManager : Singleton<MUnitManager>
    {
        public List<GameObject> m_unitTable = new List<GameObject>();

        private Dictionary<string, MUnit> m_unitPrefab = new Dictionary<string, MUnit>();
        void Awake()
        {
            //! 정리
            for (int i = 0; i < m_unitTable.Count; ++i)
            {
                MUnit unit = m_unitTable[i].GetComponent<MUnit>();
                m_unitPrefab.Add(unit.m_id, unit);
            }
        }

        public MUnit GetUnit(Unit unit)
        {   
            MUnit munit = ((GameObject)Instantiate(m_unitPrefab[unit.m_characterId].gameObject, Vector3.zero, Quaternion.identity)).GetComponent<MUnit>();            
            return munit;
        }
    }
}

