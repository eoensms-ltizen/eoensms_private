using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CreativeSpore.RpgMapEditor;

namespace stackRPG
{
    [Serializable]
    public class User
    {
        public string m_id;
        public int m_gold;
        public int m_stageNumber;

        public int m_teamId;
        public Vector3 m_startPoint;
        public List<Unit> m_haveUnit;
        public List<MUnit> m_aliveUnit;
        public Dictionary<Unit, int> m_levelTable;

        public Action m_changeGoldEvent;
        public Action m_changeStageEvent;
        public void SetGold(int gold)
        {
            m_gold = gold;
            if (m_changeGoldEvent != null) m_changeGoldEvent();
        }

        public void SetStageNumber(int stage)
        {
            m_stageNumber = stage;
            if (m_changeStageEvent != null) m_changeStageEvent();
        }

        public void AttackGround(Vector3 position)
        {
            List<Vector2> canMovePositions;
            GetCanMovePosition(position, m_aliveUnit.Count, out canMovePositions);
            for (int i = 0; i < m_aliveUnit.Count; ++i) m_aliveUnit[i].CommandAttackGround(canMovePositions[i]);
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

        int[] m_nearPositionCount = { 0, 1, 5, 13, 25, 41, 66, 107 };

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

        public void AddMUnit(MUnit unit)
        {
            m_aliveUnit.Add(unit);
        }
    }
}
