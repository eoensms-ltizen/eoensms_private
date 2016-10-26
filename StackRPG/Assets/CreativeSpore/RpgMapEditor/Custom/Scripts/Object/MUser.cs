using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CreativeSpore.RpgMapEditor;

namespace stackRPG
{
    public enum UserState
    {
        WaitTurn,
        MakeUnit,
        Ready,
        Play,
        Dead,
    }

    [Serializable]
    public class UnitLevelTable
    {
        public int m_id;
        public bool m_isOpend;
        public int m_level;

        public UnitLevelTable(int id, bool isOpend, int level)
        {
            m_id = id;
            m_isOpend = isOpend;
            m_level = level;
        }
    }

    public enum UserAction
    {
        Open,
        Upgrade,
        Make,        
    }

    public enum LastUserAction
    {
        Save,
        Upgrade,
        Make,
    }

    public class MUser
    {
        public MUser(User user)
        {
            m_userAI = user.m_userAI == null ? null : user.m_userAI.m_aI;
            m_id = user.m_id;
            m_teamId = user.m_teamId;
            m_gold = user.m_gold;
            m_startPoint = user.m_startPoint;

             m_aliveUnits = new List<MUnit>();
        }

        public AIUser m_userAI;
        public UserState m_state;

        public string m_id;
        public int m_gold;

        public int m_teamId;
        public Vector3 m_startPoint;
        public List<UnitLevelTable> m_haveUnit = new List<UnitLevelTable>();
        public List<MUnit> m_aliveUnits { get; private set; }
        

        public Action m_changeGoldEvent;

        int[] m_nearPositionCount = { 0, 1, 5, 13, 25, 41, 66, 107, 173, 280, 453 };

        void ChangeState(UserState state)
        {   
            m_state = state;
        }

        public void Ready()
        {
            ChangeState(UserState.Ready);
        }

        public void Play()
        {
            ChangeState(UserState.Play);
        }

        public void MakeUnit()
        {
            ChangeState(UserState.MakeUnit);
        }

        public void WaitTurn()
        {
            ChangeState(UserState.WaitTurn);
        }

        public IEnumerator Process()
        {
            if (m_state == UserState.Dead) yield break;
            MakeUnit();

            if(m_userAI!=null)
            {
                yield return MGameManager.Instance.StartCoroutine(MAIUser.Progress(m_userAI, this));
                Ready();
            }
            else
            {
                while (m_state != UserState.Ready) yield return null;
            }

            //while (m_state != UserState.Ready)
            //{
            //    if(m_state == UserState.MakeUnit)
            //    {
            //        if(m_userAI != null)
            //        {
            //            Action action;
            //            if (action != null) { action(); yield return null; }
            //            else Ready();
            //        }
            //    }
            //    yield return null;
            //}
        }

        bool MakeRandomUnit()
        {   
            //! 열만한 케릭터가 있느냐?

            //! 생산 가능한 유닛이 있느냐?
            if (m_haveUnit.Count == 0) return false;

            List<Unit> units;
            GetAllCanMakeUnitIndex(out units);
            if (units.Count == 0) return false;

            MGameManager.Instance.MakeUnit(units[MSettings.Random(0, units.Count)]);
            return true;
        }

        

        void GetAllCanMakeUnitIndex(out List<Unit> ids)
        {
            ids = new List<Unit>();

            for (int i = 0; i < m_haveUnit.Count; ++i)
            {
                Unit unit = MUnitManager.Instance.GetUnit(m_haveUnit[i].m_id);
                if (m_gold >= unit.m_makePrice) ids.Add(unit);
            }
        }

        public void SetGold(int gold)
        {
            m_gold = gold;
            if (m_changeGoldEvent != null) m_changeGoldEvent();
        }

        public void AttackGround(Vector3 position)
        {
            List<Vector2> canMovePositions;
            GetCanMovePosition(position, m_aliveUnits.Count, out canMovePositions);
            for (int i = 0; i < m_aliveUnits.Count; ++i) m_aliveUnits[i].CommandAttackGround(canMovePositions[i]);
        }

        public void MoveGround(Vector3 position)
        {
            List<Vector2> canMovePositions;
            GetCanMovePosition(position, m_aliveUnits.Count, out canMovePositions);
            for (int i = 0; i < m_aliveUnits.Count; ++i) m_aliveUnits[i].CommandMoveGround(canMovePositions[i]);
        }

        void GetCanMovePosition(Vector2 center, int count, out List<Vector2> positions)
        {
            positions = new List<Vector2>();
            AutoTile centerTile = RpgMapHelper.GetAutoTileByPosition(center, 0);

            int index = 0;
            while (positions.Count < count)
            {
                Vector2 pos = center;

                if (GetNextPosition(centerTile, index++, ref pos) == false) continue;
                if (IsCanMovePosition(pos) == false) continue;

                positions.Add(pos);
            }
        }

        

        bool GetNextPosition(AutoTile centerTile, int index, ref Vector2 pos)
        {
            int distance = -1;
            int number = -1;
            for (int i = 0; i < m_nearPositionCount.Length; i++)
            {
                if (m_nearPositionCount[i] > index)
                {
                    distance = i - 1;
                    number = index - m_nearPositionCount[i - 1];
                    break;
                }
            }

            if (distance == -1) return false;


            int x = -distance + (number + 1) / 2;
            int y = distance - Mathf.Abs(x);
            if (number % 2 == 0) y *= -1;

            pos = RpgMapHelper.GetTileCenterPosition(x + centerTile.TileX, y + centerTile.TileY);

            return true;
        }

        bool IsCanMovePosition(Vector2 pos)
        {
            //! 해당위치가 가능한지 안한지는, 한타일을 9등분해서 판단한다. 젠장 즉, 옆에는 서있을수있다는거다. -_-
            if (AutoTileMap.Instance.GetAutotileCollisionAtPosition(pos) == eTileCollisionType.PASSABLE) return true;
            return false;
        }

        public void MakeUnit(MUnit unit)
        {
            unit.m_changeStateDelegate += (munit) => { m_aliveUnits.Remove(munit); };
            m_aliveUnits.Add(unit);
        }

        public void OpenUnit(int id)
        {   
            for (int i = 0; i < m_haveUnit.Count; ++i)
            {
                if (m_haveUnit[i].m_id == id )
                {
                    if (m_haveUnit[i].m_isOpend) { Debug.Log("OpenUnit : " + id + " already opend unit!"); return; }
                    else { m_haveUnit[i].m_isOpend = true; return; }
                }
            }
            m_haveUnit.Add(new UnitLevelTable(id, true, 0));
        }

        public void UpgradeUnit(int id)
        {
            for (int i = 0; i < m_haveUnit.Count; ++i)
            {
                if (m_haveUnit[i].m_id == id) { m_haveUnit[i].m_level += 1; return; }
            }
            Debug.Log("UpgradeUnit : " + id + " not opend unit!");
        }
        public int GetUnitLevel(int id)
        {   
            for(int i = 0;i<m_haveUnit.Count;++i)
            {
                if (m_haveUnit[i].m_id == id) return m_haveUnit[i].m_level;
            }
            return 0;
        }

        public UnitLevelTable GetUnitLevelTable(int id)
        {
            for (int i = 0; i < m_haveUnit.Count; ++i)
            {
                if (m_haveUnit[i].m_id == id) { return m_haveUnit[i]; }
            }
            UnitLevelTable unitLevelTable = new UnitLevelTable(id, false, 0);
            m_haveUnit.Add(unitLevelTable);
            return unitLevelTable;
        }

        public void RemoveAllUnit()
        {   
            for(int i = 0;i< m_aliveUnits.Count;++i)
            {
                m_aliveUnits[i].Dead();
            }
        }

        public void Dead()
        {
            ChangeState(UserState.Dead);
        }
    }
}
