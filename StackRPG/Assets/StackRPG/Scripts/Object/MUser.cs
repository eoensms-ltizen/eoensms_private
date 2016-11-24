using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

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

        for (int i = 0; i < user.m_haveUnit.Count; ++i)
        {
            UnitLevelTable unitLevelTable = new UnitLevelTable(user.m_haveUnit[i], false, 0);
            m_haveUnit.Add(unitLevelTable);
        }

        AbilityManager.Instance.AttachAility(this);
    }

    public AbilityData GetAbilityData(AbilityType abilityType)
    {
        AbilityData abilityData = null;
        for (int i = 0; i < m_user.m_haveAbility.Count; ++i)
        {
            abilityData = m_user.m_haveAbility[i];
            if (abilityData.m_abilityType == abilityType) return abilityData;
        }

        abilityData = new AbilityData(abilityType, 0);
        m_user.m_haveAbility.Add(abilityData);

        return abilityData;
    }

    public void Init(int teamIndex, StartingPoint startingPoint)
    {
        m_teamIndex = teamIndex;
        m_startingPosition = startingPoint;

        m_state = UserState.WaitTurn;
        m_aliveUnits = new List<MUnit>();

        //! 초기화
        m_makeUnitState = new Dictionary<Point2D, MUnit>();
        for (int i = 0; i < startingPoint.m_positions.Count; ++i)
        {
            m_makeUnitState.Add(new Point2D(startingPoint.m_positions[i]), null);
        }
    }

    public User m_user;
    public AIUser m_userAI;
    public UserState m_state;

    public string m_id { get { return m_user.m_id; } }
    public string m_nickName { get { return m_user.m_nickName; } }
    public int m_gold;

    //! 유닛 생성관련된 데이터
    public int m_teamIndex;
    public StartingPoint m_startingPosition;
    public Dictionary<Point2D, MUnit> m_makeUnitState = new Dictionary<Point2D, MUnit>();
    public int m_makePointIndex;

    public List<UnitLevelTable> m_haveUnit = new List<UnitLevelTable>();
    public List<MUnit> m_aliveUnits { get; private set; }

    public bool m_isSkip { get; private set; }

    public Point2D GetSpawnPoint()
    {
        return new Point2D(m_startingPosition.m_positions[m_aliveUnits.Count]);
    }

    public UnityAction m_changeGoldEvent;

    int[] m_nearPositionCount = { 0, 1, 5, 13, 25, 41, 66, 107, 173, 280, 453 };

    void ChangeState(UserState state)
    {
        m_state = state;
    }
    public void Dead()
    {
        ChangeState(UserState.Dead);
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

    public void Skip()
    {
        m_isSkip = true;
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
            yield return MGameManager.Instance.StartCoroutine(MAIUser.Progress(m_userAI, this));
            if (m_isSkip == true)
            {
                m_isSkip = false;
                yield return new WaitForSeconds(1.0f);
            }
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

    public UnityAction<int> m_takeLevelGoldEvent;
    public void TakeLevelGold(int gold)
    {
        AddGold(gold);
        if (m_takeLevelGoldEvent != null) m_takeLevelGoldEvent(gold);
    }

    public void AddGold(int gold)
    {
        SetGold(m_gold + gold);
    }

    public bool UseGold(int gold)
    {
        if (m_gold < gold) return false;

        SetGold(m_gold - gold);
        return true;
    }

    public void AttackGround(Vector3 position)
    {
        MHelper.AttackGround(m_aliveUnits, position);
        //List<Vector2> canMovePositions;
        //MHelper.GetCanMovePosition(position, m_aliveUnits.Count, out canMovePositions);
        //for (int i = 0; i < m_aliveUnits.Count; ++i) m_aliveUnits[i].CommandAttackGround(canMovePositions[i]);
    }

    public void MoveGround(Vector3 position)
    {
        MHelper.MoveGround(m_aliveUnits, position);
        //List<Vector2> canMovePositions;
        //MHelper.GetCanMovePosition(position, m_aliveUnits.Count, out canMovePositions);
        //for (int i = 0; i < m_aliveUnits.Count; ++i) m_aliveUnits[i].CommandMoveGround(canMovePositions[i]);
    }

    public void MakeUnit(MUnit unit, Point2D point)
    {
        unit.m_changeStateDelegate += (munit) =>
        {
            if (munit.m_state == State.Dead) m_aliveUnits.Remove(munit);
        };
        m_aliveUnits.Add(unit);

        m_makeUnitState[point] = unit;
    }

    public void OpenUnit(int id)
    {
        for (int i = 0; i < m_haveUnit.Count; ++i)
        {
            if (m_haveUnit[i].m_id == id)
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
            if (m_haveUnit[i].m_id == id) { m_haveUnit[i].m_level += 1; break; }
        }

        for (int i = 0; i < m_aliveUnits.Count; ++i)
        {
            if (m_aliveUnits[i].m_id == id) m_aliveUnits[i].LevelUp();
        }
    }
    public int GetUnitLevel(int id)
    {
        for (int i = 0; i < m_haveUnit.Count; ++i)
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

    public void SetMakePointIndex(int index)
    {
        m_makePointIndex = index;
    }



    public bool IsCanMakeUnit()
    {
        if (m_aliveUnits.Count >= m_startingPosition.m_positions.Count) return false;

        return true;
    }

    public bool IsEmptyPoint(Point2D point)
    {
        if (m_makeUnitState.ContainsKey(point) == false) return false;

        if (m_makeUnitState[point] != null) return false;

        return true;
    }
}
