using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;
using stackRPG;

public partial class MGameManager : Singleton<MGameManager>
{
    /// <summary>
    /// WaitReady 연출 : 유저들이 돌아가면서, 턴을 처리하고, 모든 유저가 래디하면 게임시작시킨다.
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitReady()
    {
        while (true)
        {
            MUser muser = GetUserByState(UserState.WaitTurn);
            if (muser == null) break;

            m_currentUser = muser;
            
            PlayUI.Instance.OnChangeUser(muser);
            MarkIndex(muser, 0);
            MakeSquare(muser.m_startingPosition);
            MGameCamera.Instance.OnFocusUserStartingPosition(muser);

            yield return StartCoroutine(Notice.Instance.Center(muser.m_nickName, NoticeEffect.Fade, 3, 1));

            bool isOwner = muser.m_id == m_owner.m_id;

            if(isOwner)
            {   
                yield return StartCoroutine(Notice.Instance.Center("$ " + SingleGameManager.Instance.m_currentRewordGold + " 지급", NoticeEffect.Typing, 2, 1));
                m_owner.SetGold(m_owner.m_gold + SingleGameManager.Instance.m_currentRewordGold);
            }
            
            PlayUI.Instance.ShowSkipButton(!isOwner);
            PlayUI.Instance.ShowMakeUnitPanel(isOwner);
            PlayUI.Instance.ShowUnitPositionPanel(isOwner);
            MGameCamera.Instance.SetPivot(isOwner ? MGameCamera.m_pivotLeft : MGameCamera.m_pivotCenter);

            yield return StartCoroutine(m_currentUser.Process());

            PlayUI.Instance.ShowSkipButton(false);
            MakeSquare(null);
            PlayUI.Instance.RemoveMarkSquare();
        }
        
        PlayUI.Instance.ShowSkipButton(false);
        PlayUI.Instance.ShowMakeUnitPanel(false);
        PlayUI.Instance.ShowUnitPositionPanel(false);
        PlayUI.Instance.ShowFocusButton(false);
        MGameCamera.Instance.SetFreeMove(false);
        MGameCamera.Instance.SetPivot(MGameCamera.m_pivotCenter);

        ChangeGameState(GameState.Play);
    }

    MUser GetUserByState(UserState userState)
    {
        for (int i = 0; i < m_userList.Count; ++i)
        {
            if (m_userList[i].m_state != userState) continue;
            return m_userList[i];
        }
        return null;
    }

    public void MakeUnit(string userID, int unitID, Point2D point)
    {
        MUser user = GetUser(userID);
        if (user.IsCanMakeUnit() == false) return;

        if (user.IsEmptyPoint(point) == false) return;

        Unit unit = MUnitManager.Instance.GetUnit(unitID);
        if (user.UseGold(unit.m_makePrice) == false) return;

        MUnit munit = MUnitManager.Instance.GetMUnit(unitID);
        munit.m_level = user.GetUnitLevel(unit.m_id);
        munit.m_teamId = user.m_teamIndex;
        munit.m_teamColor = user.m_startingPosition.m_color;
        munit.transform.FindChild("Sprite").FindChild("TeamCircle").GetComponent<SpriteRenderer>().color = munit.m_teamColor;        
        munit.transform.position = RpgMapHelper.GetTileCenterPosition(point.X, point.Y);
        munit.Init(unit);

        user.MakeUnit(munit, point);

        PlayUI.Instance.OnNextUnitPosition();
        //MGameCamera.Instance.SetTarget(munit.transform.position);
    }

    public bool UpgradeUnit(string userID, int unitID)
    {
        MUser user = GetUser(userID);
        int level = user.GetUnitLevel(unitID);
        Unit unit = MUnitManager.Instance.m_units[unitID];
        if (level >= unit.m_upgradeCost.Length - 1) return false;
        if (user.UseGold(MUnitManager.Instance.m_units[unitID].m_upgradeCost[level]) == false) return false;

        user.UpgradeUnit(unitID);
        return true;
    }

    public bool OpenUnit(string userID, int unitID)
    {
        MUser user = GetUser(userID);
        if (user.UseGold(MUnitManager.Instance.GetUnit(unitID).m_openPrice) == false) return false;

        user.OpenUnit(unitID);
        return true;
    }

    public void MakeSquare(StartingPoint startingPoint)
    {
        if(startingPoint == null) { PlayUI.Instance.RemoveMakeSquare(); return; }

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < startingPoint.m_positions.Count; ++i)
        {
            Vector3 position = startingPoint.m_positions[i];
            positions.Add(RpgMapHelper.GetTileCenterPosition((int)position.x, (int)position.y));
        }

        PlayUI.Instance.MakeSquare(positions, startingPoint.m_color);
        
    }

    public void MarkIndex(MUser user, int index)
    {
        user.SetMakePointIndex(index);

        Vector2 tilePos = user.m_startingPosition.m_positions[index];
        Point2D point = new Point2D(tilePos);

        //! 카메라 포커싱
        MGameCamera.Instance.SetTarget(point.X, point.Y);

        //! 위치 표시
        PlayUI.Instance.MarkSquare(user.m_startingPosition.m_color, RpgMapHelper.GetTileCenterPosition(point.X, point.Y));
    }

    bool m_isForceFreeMove = false;
    public void SetFreeMove(bool value)
    {
        m_isForceFreeMove = value;
        MGameCamera.Instance.SetFreeMove(value);
    }
}
