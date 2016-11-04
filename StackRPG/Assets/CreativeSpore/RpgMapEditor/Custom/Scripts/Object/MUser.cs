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
            m_user = user;
        }

        public void Init(int teamIndex, StartingPoint startingPoint, Vector2 attackPoint)
        {
            m_teamIndex = teamIndex;
            m_startingPosition = startingPoint;
            m_attackPoint = attackPoint;

            m_state = UserState.WaitTurn;
            m_aliveUnits = new List<MUnit>();
        }

        public User m_user;
        public AIUser m_userAI;
        public UserState m_state;

        public string m_id { get { return m_user.m_id; } }
        public string m_nickName { get { return m_user.m_nickName; } }
        public int m_gold;

        public int m_teamIndex;        
        public StartingPoint m_startingPosition;
        public Vector3 m_attackPoint;

        public List<UnitLevelTable> m_haveUnit = new List<UnitLevelTable>();
        public List<MUnit> m_aliveUnits { get; private set; }

        public Vector2 GetSpawnPoint()
        {
            return m_startingPosition.m_positions[m_aliveUnits.Count];
        }

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

        public void SetUnitPosition(StartingPoint spawnPosition)
        {
            m_startingPosition = spawnPosition;
        }

        public IEnumerator Process()
        {
            if (m_state == UserState.Dead) yield break;
            MakeUnit();
            if (m_userAI != null)
            {
                yield return new WaitForSeconds(1.0f);
                yield return MGameManager.Instance.StartCoroutine(MAIUser.Progress(m_userAI, this));
                Ready();
            }
            else
            {
                while (m_state != UserState.Ready) yield return null;
            }
        }
        

        public void SetGold(int gold)
        {
            m_gold = gold;
            if (m_changeGoldEvent != null) m_changeGoldEvent();
        }

        public bool UseGold(int gold)
        {
            if (m_gold < gold) return false;

            SetGold(m_gold - gold);
            return true;
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
            unit.m_changeStateDelegate += (munit) => 
            {   
                m_aliveUnits.Remove(munit);
            };
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

        public void Dead()
        {
            ChangeState(UserState.Dead);
        }

        public bool IsCanMakeUnit()
        {
            if (m_aliveUnits.Count >= m_startingPosition.m_positions.Count) return false;

            return true;
        }
    }
}
