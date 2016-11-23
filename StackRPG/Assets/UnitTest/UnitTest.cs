using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using stackRPG;
using CreativeSpore.RpgMapEditor;

public class UnitTest : MonoBehaviour
{
    public float m_playSpeed = 1.0f;
    public float m_timeValue = 0.25f;
    public TestData m_testData;
    private List<TestTeam> m_teams = new List<TestTeam>();
    private Dictionary<int, List<MUnit>> m_aliveUnits = new Dictionary<int, List<MUnit>>();
    void Start()
    {
        Time.timeScale = m_playSpeed;

        MGameCamera.Instance.InitMapData();
        MGameCamera.Instance.SetTarget(transform.position);

        m_teams = m_testData.m_teams;

        for (int i = 0; i < m_teams.Count; ++i)
        {
            m_aliveUnits.Add(m_teams[i].m_teamID, InitTeam(m_teams[i]));
        }

        StartCoroutine(AttackGround(1.0f));

        List<Transform> totalUnit = new List<Transform>();
        foreach(KeyValuePair<int,List<MUnit>> value in m_aliveUnits)
        {
            foreach(MUnit munit in value.Value)
            {
                totalUnit.Add(munit.transform);
            }
        }
        MGameCamera.Instance.SetFollowGroup(totalUnit);
    }

    IEnumerator AttackGround(float delay)
    {
        while (true)
        {
            for (int i = 0; i < m_aliveUnits.Count; ++i)
            {
                int target = i + 1;
                if (target >= m_aliveUnits.Count) target = 0;

                List<MUnit> ownerunits = m_aliveUnits[m_teams[i].m_teamID];
                List<MUnit> targetUnits = m_aliveUnits[m_teams[target].m_teamID];

                Vector3 attackPosition = Vector3.zero;
                if (GetUserUnitCenterPosition(targetUnits, ref attackPosition) == true) MHelper.AttackGround(ownerunits, attackPosition);
            }
            yield return new WaitForSeconds(delay);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            m_playSpeed += m_timeValue;
            Time.timeScale = m_playSpeed;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            m_playSpeed -= m_timeValue;
            Time.timeScale = m_playSpeed;
        }
    }

    List<MUnit> InitTeam(TestTeam testTeam)
    {
        List<MUnit> teamUnits = new List<MUnit>();
        for (int i = 0; i < testTeam.m_units.Count; ++i)
        {
            TestUnit testUnit = testTeam.m_units[i];
            for (int j = 0; j < testUnit.m_count; ++j)
            {
                teamUnits.Add(MakeUnit(testUnit.m_unitData.m_unitData, testTeam.m_teamID, testUnit.m_level, testTeam.m_teamColor, testTeam.m_startPoint));
            }
        }

        List<Vector2> positions;
        MHelper.GetCanMovePosition(Point2D.GetVector(testTeam.m_startPoint), teamUnits.Count, out positions);
        for (int i = 0; i < teamUnits.Count; ++i)
        {
            teamUnits[i].transform.position = positions[i];
        }

        return teamUnits;
    }

    MUnit MakeUnit(Unit unit, int teamID, int level, Color color, Point2D point)
    {
        MUnit munit = MUnitManager.Instance.GetMUnit(unit.m_id);
        munit.m_level = level;
        munit.m_teamId = teamID;
        munit.m_teamColor = color;
        munit.transform.FindChild("Sprite").FindChild("TeamCircle").GetComponent<SpriteRenderer>().color = munit.m_teamColor;
        munit.transform.position = RpgMapHelper.GetTileCenterPosition(point.X, point.Y);
        munit.Init(unit);

        return munit;
    }

    public bool GetUserUnitCenterPosition(List<MUnit> units, ref Vector3 position)
    {
        //! 중심 위치 선정
        Vector3 totalPosition = Vector3.zero;
        for (int i = 0; i < units.Count; ++i)
        {
            if (units[i] == null) continue;
            totalPosition += units[i].transform.position;
        }
        position = totalPosition / units.Count;

        //! 중심에서 가장 가까운 유닛의 위치를 타겟으로삼는다.
        //! 그러지 않으면, 갈수없는곳을 타겟으로 삼을수도있다.
        int unitIndex = -1;
        float sqrMagnitude = 0;
        for (int i = 0; i < units.Count; ++i)
        {
            if (units[i] == null) continue;
            bool isChoose = false;
            float tempMagnitue = Vector3.SqrMagnitude(totalPosition - units[i].transform.position);

            if (unitIndex == -1) isChoose = true;
            else if (sqrMagnitude > tempMagnitue) isChoose = true;

            if (isChoose == true)
            {
                unitIndex = i;
                sqrMagnitude = tempMagnitue;
            }
        }
        return totalPosition != Vector3.zero ? true : false;
    }
}
