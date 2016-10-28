using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;

namespace stackRPG
{
    public enum GameState
    {
        Init,
        StartStage,
        ChangeTurn,
        WaitReady,
        Play,
        Result,
        Finish,
    }

    //! 이녀석은 받은 맵, 유저로 게임을 진행시키는 놈이다.

    public class MGameManager : Singleton<MGameManager>
    {   
        public MUser m_owner;

        public GameState m_state;
        public int m_stageNumber;
        public int m_selectUserIndex;
        public MUser m_currentUser { get; private set; }

        public Action<MUser> m_changeUserEvent;
        public Action<GameState> m_changeGameState;
        public Action<int> m_changeStageNumber;

        public List<MUser> m_userList = new List<MUser>();        

        public bool m_isReadyManager { get; private set; }

        void Awake()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            ChangeGameState(GameState.Init);
            m_stageNumber = -1;
            m_selectUserIndex = -1;
            m_isReadyManager = true;

            //! 유저데이터 로드한다.

            LoadOwnerData();

            yield return null;
            ChangeGameState(GameState.StartStage);
        }

        public IEnumerator WaitPrecess()
        {
            while (m_isReadyManager == false) yield return null;
        }

        private void LoadOwnerData()
        {
            m_userList.Clear();
            m_owner = new MUser((ResourcesManager.Load("Owner") as UserData).m_user);               
            m_userList.Add(m_owner);
        }

        private void ChangeGameState(GameState state)
        {
            m_state = state;
            if (m_changeGameState != null) m_changeGameState(m_state);

            switch(m_state)
            {
                case GameState.StartStage:
                    {
                        StartCoroutine(StartStage());
                    }
                    break;                
                case GameState.ChangeTurn:
                    {
                        StartCoroutine(ChangeTurn());
                    }
                    break;
                case GameState.WaitReady:
                    {
                        //! 유저 한명씩 턴을 보급해준다.

                        //! 모든 유저가 레디를 마치면 자동 시작!

                        StartCoroutine(MakeProcess());
                    }
                    break;
                case GameState.Play:
                    {
                        StartCoroutine(Play());
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

        IEnumerator StartStage()
        {
            if (SingleGameManager.Instance.NextStage() == false) { ChangeGameState(GameState.Finish); yield break; }

            Map map = SingleGameManager.Instance.m_currentMap;

            //! 모든 유닛 제거
            for (int i = 0; i < m_userList.Count; ++i)
            {
                m_userList[i].RemoveAllUnit();

                //! 유저가아닌 녀석은 없앤다.
                if (m_userList[i].m_id != m_owner.m_id)
                {
                    m_userList.RemoveAt(i);
                    i--;
                }
            }

            yield return new WaitForSeconds(0.5f);

            //! 맵생성
            AutoTileMap autoTileMap = FindObjectOfType<AutoTileMap>();
            if (autoTileMap == null) Debug.LogError("Not Found Object! [AutoTileMap]");

            autoTileMap.Tileset = map.m_autoTileset;
            autoTileMap.MapData = map.m_autoTileMapData;

            yield return new WaitForSeconds(0.5f);
            

            //! 보상주기            
            m_owner.SetGold(m_owner.m_gold + SingleGameManager.Instance.m_currentRewordGold);
            Debug.Log("보급품을 받았습니다. : $ " + SingleGameManager.Instance.m_currentRewordGold);

            yield return new WaitForSeconds(0.5f);

            //! 적진입
            MUser enemy = new MUser(new User("enemy ID"," enemy NickName"));
            enemy.SetGold(SingleGameManager.Instance.m_enemyGold);            
            enemy.m_userAI = SingleGameManager.Instance.m_currentEnemy;
            m_userList.Add(enemy);

            yield return new WaitForSeconds(0.5f);

            //! 유저 순서정렬 및 초기화
            m_userList.Remove(m_owner);
            m_owner.WaitTurn();
            m_userList.Add(m_owner);

            //! 유저에게 스타팅 포인트 지급
            //! 셔플
            List<int> usedStartingPointIndex = new List<int>();
            for (int i = 0; i < map.m_makeUnitPositions.Count;++i) usedStartingPointIndex.Add(i);
            MSettings.Shuffle(usedStartingPointIndex);
            //! 지급
            for (int i = 0; i<m_userList.Count; ++i)
            {
                int index = usedStartingPointIndex[i];
                m_userList[i].Init(index, map.m_makeUnitPositions[index], map.m_attackPoint);
            }

            //! 게임시작
            ChangeGameState(GameState.ChangeTurn);
        }
        IEnumerator ChangeTurn()
        {
            if (m_userList.Count == 0) { Debug.Log("Can not Start Game (User Count : 0)"); yield break; }

            for (int i = 0; i < m_userList.Count; ++i)
            {
                if (m_userList[i].m_state != UserState.WaitTurn) continue;

                yield return new WaitForSeconds(0.5f);
                SetUser(m_userList[i]);
                ChangeGameState(GameState.WaitReady);
                yield break;
            }

            ChangeGameState(GameState.Play);
        }

        IEnumerator MakeProcess()
        {
            yield return StartCoroutine(m_currentUser.Process());
            ChangeGameState(GameState.ChangeTurn);
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                m_selectUserIndex++;
                if (m_selectUserIndex >= m_userList.Count) m_selectUserIndex = 0;
                
                SetUser(m_userList[m_selectUserIndex]);
            }
        }

        public void SetUser(MUser user)
        {
            m_currentUser = user;
            if (m_changeUserEvent != null) m_changeUserEvent(m_currentUser);
        }

        int unitCount = 0;

        

        public void MakeUnit(Unit unit)
        {
            if (IsCanMakeUnit() == false) return;
            if (UseGold(unit.m_makePrice) == false) return;

            MUnit munit = MUnitManager.Instance.GetUnit(unit);            
            munit.m_level = m_currentUser.GetUnitLevel(unit.m_id);
            munit.m_teamId = m_currentUser.m_teamIndex;
            Vector2 pos = m_currentUser.GetSpawnPoint();
            munit.transform.position =  RpgMapHelper.GetTileCenterPosition((int)pos.x, (int)pos.y);
            munit.Init(unit);

            m_currentUser.MakeUnit(munit);
        }

        public bool UpgradeUnit(Unit unit)
        {
            int level = m_currentUser.GetUnitLevel(unit.m_id);
            if (UseGold(unit.m_upgradeCost[level]) == false) return false;

            m_currentUser.UpgradeUnit(unit.m_id);
            return true;
        }
        public bool OpenUnit(Unit unit)
        {
            if (UseGold(unit.m_openPrice) == false) return false;

            m_currentUser.OpenUnit(unit.m_id);
            return true;
        }   

        private void SetStage(int stageNumber)
        {   
            m_stageNumber = stageNumber;

            if (m_changeStageNumber != null) m_changeStageNumber(stageNumber);
        }

        public MUser GetUser(string id)
        {
            for (int i = 0; i < m_userList.Count; ++i)
            {
                if (m_userList[i].m_id == id) return m_userList[i];
            }
            return null;
        }

        private bool UseGold(int value)
        {
            if (m_currentUser == null) return false;
            if (m_currentUser.m_gold < value) return false;

            m_currentUser.SetGold(m_currentUser.m_gold - value);
            return true;
        }

        private bool IsCanMakeUnit()
        {
            if (m_currentUser == null) return false;
            return m_currentUser.IsCanMakeUnit();
        }
        
        IEnumerator Play()
        {
            for (int i = 0; i < m_userList.Count; ++i)
            {
                m_userList[i].Play();
            }

            //! 자기 위치에 해쳐모여
            //for (int i = 0; i < m_userList.Count; ++i)
            //{
            //    m_userList[i].MoveGround(m_userList[i].m_startPoint);
            //}

            yield return new WaitForSeconds(1.0f);

            //! 적으로 돌격
            for (int i = 0; i < m_userList.Count; ++i)
            {
                int targetIndex = i + 1;
                if (targetIndex >= m_userList.Count) targetIndex = 0;
                Vector2 pos = m_userList[targetIndex].m_attackPoint;
                m_userList[i].AttackGround(RpgMapHelper.GetTileCenterPosition((int)pos.x, (int)pos.y));
            }

            //! 유저의 유닛카운트가 0이면, 유저를 죽인다.
            float playLimitTime = 60.0f;
            while (playLimitTime > 0 && m_userList.Count > 1)
            {
                playLimitTime -= Time.deltaTime;

                for (int i = 0; i < m_userList.Count; ++i)
                {
                    MUser user = m_userList[i];
                    if (user.m_aliveUnits.Count == 0)
                    {
                        Debug.Log("user Die : " + user.m_id);
                        user.Dead();
                        m_userList.Remove(user);
                        i--;
                    }
                    yield return null;
                }
            }

            if (m_owner.m_state != UserState.Dead) ChangeGameState(GameState.StartStage);
            else ChangeGameState(GameState.Result);
        }

        IEnumerator Result()
        {
            Debug.Log("You Record : " + m_stageNumber);
            yield return new WaitForSeconds(2.0f);
            SceneManager.LoadScene("MainMenu");
        }

        IEnumerator Finish()
        {
            Debug.Log("Victory!!");
            yield return new WaitForSeconds(2.0f);
            SceneManager.LoadScene("MainMenu");
        }
    }
}
