using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;
using stackRPG;

public partial class MGameManager : Singleton<MGameManager>
{
    
    IEnumerator ClearStage()
    {
        if (SingleGameManager.Instance.IsLastStage() == true) { ChangeGameState(GameState.Finish); yield break; }

        StartCoroutine(Notice.Instance.Center("Clear", NoticeEffect.Typing, 1, 0.5f));
        //! 모든 유닛 제거
        for (int i = 0; i < m_userList.Count; ++i)
        {
            MUser user = m_userList[i];

            float time = 1.0f / user.m_aliveUnits.Count;
            for (int j = 0; j < user.m_aliveUnits.Count; ++j)
            {
                MUnit unit = user.m_aliveUnits[j];
                unit.Dead();
                --j;
                yield return new WaitForSeconds(time);
            }
            //! 유저가아닌 녀석은 없앤다.
            if (user.m_id != m_owner.m_id)
            {
                m_userList.RemoveAt(i);
                i--;
            }
        }

        yield return StartCoroutine(MLoading.Instance.StandardAnimation(m_camera));

        ChangeGameState(GameState.LoadMap);
    }
}
