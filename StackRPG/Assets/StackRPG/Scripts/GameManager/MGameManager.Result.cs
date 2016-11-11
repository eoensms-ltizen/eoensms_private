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

        Notice.Instance.CenterAppear("Game Over", NoticeEffect.Fade, 0.5f);
        Notice.Instance.BottomAppear("Last Stage : " + SingleGameManager.Instance.m_stageNumber, NoticeEffect.Typing, 0.5f);

        yield return new WaitForSeconds(3.0f);
        MLoading.Instance.LoadScene("MainMenu");
    }

    IEnumerator Finish()
    {
        MGameCamera.Instance.Finish();

        Notice.Instance.CenterAppear("Victory", NoticeEffect.Typing, 0.1f);
        yield return new WaitForSeconds(5.0f);
        Notice.Instance.CenterDisappear("Victory", NoticeEffect.Typing, 1.0f);
        yield return new WaitForSeconds(2.0f);
        Notice.Instance.CenterAppear("------------ Made by ------------\n             우짜까따쯔빠", NoticeEffect.Typing, 1.0f);
        yield return new WaitForSeconds(5.0f);
        Notice.Instance.BottomAppear("Click To Main", NoticeEffect.Typing, 0.5f);
        
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
