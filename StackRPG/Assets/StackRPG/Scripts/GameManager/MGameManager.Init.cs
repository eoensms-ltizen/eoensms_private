using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;
using stackRPG;


public partial class MGameManager : Singleton<MGameManager>
{    
    IEnumerator Init()
    {
        yield return null;
        ChangeGameState(GameState.Init);        
        m_selectUserIndex = -1;
        m_isReadyManager = true;

        //! Camera
        InitCamera();

        //! AutoTileMap 생성
        InitAutoTileMap();

        //! 유저데이터 로드한다.
        InitOwner();

        //! 유아이인잇
        InitUI();

        ChangeGameState(GameState.LoadMap);
    }

    private void InitCamera()
    {
        MGameCamera.Instance.Init();
    }

    private void InitAutoTileMap()
    {
        AutoTileMap autoTileMap = Instantiate(m_autoTileMapPrefab).GetComponent<AutoTileMap>();
        autoTileMap.ViewCamera = MGameCamera.Instance.m_camera;
    }

    private void InitOwner()
    {
        m_userList.Clear();
        User user = (ResourcesManager.Load("Owner") as UserData).m_user;
        user.m_nickName = MSettings.GetNickname();
        m_owner = new MUser(user);

        m_userList.Add(m_owner);
    }

    private void InitUI()
    {
        RectTransform rectTransform = (Instantiate(m_playUIPrefab) as GameObject).GetComponent<RectTransform>();
        rectTransform.SetParent(FindObjectOfType<Canvas>().GetComponent<RectTransform>());
        rectTransform.localScale = Vector3.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;
        PlayUI.Instance.Init();
    }
}
