using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;
using stackRPG;


public partial class MGameManager : Singleton<MGameManager>
{
    IEnumerator Result()
    {
        MGameCamera.Instance.GameOver();

        Notice.Instance.CenterAppear("전투에서 졌습니다.", NoticeEffect.Fade, 0.5f);
        Notice.Instance.BottomAppear("마지막 전투 : " + SingleGameManager.Instance.m_stageNumber, NoticeEffect.Typing, 0.5f);

        yield return new WaitForSeconds(3.0f);
        MLoading.Instance.LoadScene("MainMenu");
    }

    IEnumerator Finish()
    {
        MGameCamera.Instance.Finish();

        Notice.Instance.CenterAppear("모든 전투에서 승리하였습니다", NoticeEffect.Typing, 0.1f);
        yield return new WaitForSeconds(5.0f);
        Notice.Instance.CenterDisappear("모든 전투에서 승리하였습니다", NoticeEffect.Typing, 1.0f);
        yield return new WaitForSeconds(2.0f);
        Notice.Instance.CenterAppear("------------ 만든이 ------------\n             우짜까따쯔빠", NoticeEffect.Typing, 1.0f);
        yield return new WaitForSeconds(5.0f);
        Notice.Instance.BottomAppear("누르면 처음화면으로 이동합니다", NoticeEffect.Typing, 0.5f);
        
        while(true)
        {
            if(Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        }

        MLoading.Instance.LoadScene("MainMenu");
    }
}
