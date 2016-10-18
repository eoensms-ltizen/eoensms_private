using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace stackRPG
{

    public class MGameManager : Singleton<MGameManager>
    {
        public Dictionary<Guid, MUnit> m_units = new Dictionary<Guid, MUnit>();
        public LinkedList<MUnit> m_unitLinkedList = new LinkedList<MUnit>();

        public Guid AddUnit(MUnit unit)
        {
            Guid guid = Guid.NewGuid();
            m_units.Add(guid, unit);
            m_unitLinkedList.AddLast(unit);
            return guid;
        }

        public void RemoveUnit(Guid guid)
        {
            m_unitLinkedList.Remove(m_units[guid]);
            m_units.Remove(guid);
        }
    }
}
