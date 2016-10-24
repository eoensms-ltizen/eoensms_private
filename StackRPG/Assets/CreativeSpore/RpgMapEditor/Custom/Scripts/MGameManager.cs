using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace stackRPG
{
    public enum GameState
    {
        WaitReady,
        PlayStage,
    }

    public class MGameManager : Singleton<MGameManager>
    {
        public int m_stageNumber;
        public int m_selectUserIndex;
        public User m_user { get; private set; }

        public Action<User> m_changeUserEvent;

        public List<User> m_userList = new List<User>();
        public Dictionary<Guid, MUnit> m_units = new Dictionary<Guid, MUnit>();
        public LinkedList<MUnit> m_unitLinkedList = new LinkedList<MUnit>();

        void Awake()
        {
            m_stageNumber = 1;
            m_selectUserIndex = -1;
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                m_selectUserIndex++;
                if (m_selectUserIndex >= m_userList.Count) m_selectUserIndex = 0;
                
                SetUser(m_userList[m_selectUserIndex]);
            }
        }

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

        public void SetUser(User user)
        {
            m_user = user;
            if (m_changeUserEvent != null) m_changeUserEvent(m_user);
        }

        public void BuyCharcter(Unit unit)
        {
            if (UseGold(unit.m_price) == false) return;

            MUnit munit = MUnitManager.Instance.GetUnit(unit);
            munit.m_id = unit.m_characterId;
            munit.m_level = unit.m_level;
            munit.m_teamId = m_user.m_teamId;
            munit.transform.position = m_user.m_startPoint;

            m_user.AddMUnit(munit);
        }       

        private bool SetStage(int stageNumber)
        {
            if (m_user == null) return false;
            if (m_user.m_stageNumber == stageNumber) return false;

            m_user.SetStageNumber(stageNumber);
            m_stageNumber = stageNumber;
            return true;
        }
        private bool UseGold(int value)
        {
            if (m_user == null) return false;
            if (m_user.m_gold < value) return false;

            m_user.SetGold(m_user.m_gold - value);
            return true;
        }

        public void StartStage()
        {
            for(int i = 0;i<m_userList.Count;++i)
            {
                int targetIndex = i + 1;
                if (targetIndex >= m_userList.Count) targetIndex = 0;
                
                m_userList[i].AttackGround(m_userList[targetIndex].m_startPoint);
            }
        }
    }
}
