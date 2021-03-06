﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;
using stackRPG;

public enum GameState
{
    Init,
    LoadMap, //! 맵(stage)을 불러온다.(구지 StartStage와 나눈이유는, CameraSetting때문이다.)
    StartStage, //! 게임시작에 필요한 준비함                
    WaitReady, //! 현재 유저가 레디하기를 기다림
    Play, //! 자동전투씬
    ClearStage, //! 스테이지 클리어
    Result, //! 게임오버
    Finish, //! 모든스테이지 클리어
}

public partial class MGameManager : Singleton<MGameManager>
{
    public float m_delayAttackGround = 2.0f;
    public bool m_isCanControllUser;

    public MUser m_owner;

    public Camera m_camera;
    public GameObject m_autoTileMapPrefab;
    public GameObject m_playUIPrefab;

    public GameState m_state;    
    public int m_selectUserIndex;
    public MUser m_currentUser { get; private set; }
    
    public Action<GameState> m_changeGameState;    

    public List<MUser> m_userList = new List<MUser>();

    public bool m_isReadyManager { get; private set; }

    void Awake()
    {
        StartCoroutine(Init());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            MLoading.Instance.LoadScene("MPlay");
        }
    }

    private void ChangeGameState(GameState state)
    {
        m_state = state;
        if (m_changeGameState != null) m_changeGameState(m_state);

        switch (m_state)
        {
            case GameState.LoadMap:
                {
                    StartCoroutine(LoadMap());
                }
                break;
            case GameState.StartStage:
                {
                    StartCoroutine(StartStage());
                }
                break;
            case GameState.WaitReady:
                {
                    StartCoroutine(WaitReady());
                }
                break;
            case GameState.Play:
                {
                    StartCoroutine(Play());
                }
                break;
            case GameState.ClearStage:
                {
                    StartCoroutine(ClearStage());
                }
                break;
            case GameState.Result:
                {
                    StartCoroutine(Result());
                }
                break;
            case GameState.Finish:
                {
                    StartCoroutine(Finish());
                }
                break;
        }
    }

    public MUser GetUser(string id)
    {
        for (int i = 0; i < m_userList.Count; ++i)
        {
            if (m_userList[i].m_id == id) return m_userList[i];
        }
        return null;
    }       
}
