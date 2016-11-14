using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;
using stackRPG;

public partial class MGameManager : Singleton<MGameManager>
{
    IEnumerator StartStage()
    {
        MGameCamera.Instance.FullScreenAndCenter();
        yield return null;

        PlayUI.Instance.DrawStage(SingleGameManager.Instance.m_stageNumber + 1);

        yield return StartCoroutine(MLoading.Instance.ReverseAnimation(m_camera));

        yield return StartCoroutine(MGameCamera.Instance.MapTour());

        
        EnterNewEnemy();

        UserLineUp();

        AllocateRandomStartingPoint();
        
        PlayUI.Instance.ShowFocusButton(true);

        //! 게임시작
        ChangeGameState(GameState.WaitReady);
    }

    private void EnterNewEnemy()
    {
        MUser enemy = new MUser(SingleGameManager.Instance.m_currentUser);
        enemy.SetGold(SingleGameManager.Instance.m_enemyGold);
        enemy.m_userAI = SingleGameManager.Instance.m_currentAI;
        m_userList.Add(enemy);
    }

    /// <summary>
    /// 유저 순서정렬 및 초기화 : 마지막턴으로 보낸다.
    /// </summary>
    private void UserLineUp()
    {
        //! 유저 순서정렬 및 초기화
        m_owner.WaitTurn();
        m_userList.Remove(m_owner);        
        m_userList.Add(m_owner);
    }

    /// <summary>
    /// 렌덤으로 스타팅포인트를 발급한다.
    /// </summary>
    private void AllocateRandomStartingPoint()
    {
        Map map = SingleGameManager.Instance.m_currentMap;
        List<int> usedStartingPointIndex = new List<int>();
        for (int i = 0; i < map.m_makeUnitPositions.Count; ++i) usedStartingPointIndex.Add(i);
        MSettings.Shuffle(usedStartingPointIndex);
        for (int i = 0; i < m_userList.Count; ++i)
        {
            int index = usedStartingPointIndex[i];
            m_userList[i].Init(index, map.m_makeUnitPositions[index]);
        }
    }
}
