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
        LoadMap, //! 맵(stage)을 불러온다.(구지 StartStage와 나눈이유는, CameraSetting때문이다.)
        StartStage, //! 게임시작에 필요한 준비함                
        WaitReady, //! 현재 유저가 레디하기를 기다림
        Play, //! 자동전투씬
        ClearStage, //! 스테이지 클리어
        Result, //! 게임오버
        Finish, //! 모든스테이지 클리어
    }

    //! 이녀석은 받은 맵, 유저로 게임을 진행시키는 놈이다.

    public class MGameManager : Singleton<MGameManager>
    {
        public bool m_isAutoTarget;

        public MUser m_owner;

        public Camera m_camera;
        public GameObject m_autoTileMapPrefab;
        public GameObject m_gameCameraPrefab;
        public GameObject m_playUIPrefab;

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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                MLoading.Instance.LoadScene("MPlay");
            }
        }

        IEnumerator Init()
        {
            yield return null;
            ChangeGameState(GameState.Init);
            m_stageNumber = -1;
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

        public IEnumerator WaitPrecess()
        {
            while (m_isReadyManager == false) yield return null;
        }

        private void InitCamera()
        {
            //! 첫 위치 조절?
            MGameCamera gameCamera = Instantiate(m_gameCameraPrefab).GetComponent<MGameCamera>();
        }

        private void InitAutoTileMap()
        {
            //! 맵생성
            AutoTileMap autoTileMap = Instantiate(m_autoTileMapPrefab).GetComponent<AutoTileMap>();
            autoTileMap.ViewCamera = MGameCamera.Instance.m_camera;
        }

        private void InitOwner()
        {
            m_userList.Clear();
            m_owner = new MUser((ResourcesManager.Load("Owner") as UserData).m_user);               
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

        private void ChangeGameState(GameState state)
        {
            m_state = state;
            if (m_changeGameState != null) m_changeGameState(m_state);

            switch(m_state)
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
        IEnumerator LoadMap()
        {
            if (SingleGameManager.Instance.NextStage() == false) { ChangeGameState(GameState.Finish); yield break; }

            Map map = SingleGameManager.Instance.m_currentMap;
            AutoTileMap.Instance.Tileset = map.m_autoTileset;
            AutoTileMap.Instance.MapData = map.m_autoTileMapData;

            ChangeGameState(GameState.StartStage);
        }

        IEnumerator StartStage()
        {
            yield return null;
            MGameCamera.Instance.FullScreenAndCenter();
            yield return null;
            yield return StartCoroutine(MLoading.Instance.ReverseAnimation(m_camera));

            yield return StartCoroutine(MGameCamera.Instance.MapTour());

            //! 적진입
            MUser enemy = new MUser(SingleGameManager.Instance.m_currentUser);
            enemy.SetGold(SingleGameManager.Instance.m_enemyGold);            
            enemy.m_userAI = SingleGameManager.Instance.m_currentAI;
            m_userList.Add(enemy);
            
            yield return StartCoroutine(Notice.Instance.Center("[" + enemy.m_nickName + "] 출현!! ", NoticeEffect.Fade, 5, 0));
            yield return StartCoroutine(Notice.Instance.Center("[" + enemy.m_nickName + "] 출현!! ", NoticeEffect.Fade, 5, 0));
            yield return StartCoroutine(Notice.Instance.Center("[" + enemy.m_nickName + "] 출현!! ", NoticeEffect.Fade, 5, 0));

            //! 보상주기            
            m_owner.SetGold(m_owner.m_gold + SingleGameManager.Instance.m_currentRewordGold);
            yield return StartCoroutine(Notice.Instance.Center("$ " + SingleGameManager.Instance.m_currentRewordGold + " 지급", NoticeEffect.Typing, 1, 1));

            //! 유저 순서정렬 및 초기화
            m_userList.Remove(m_owner);
            m_owner.WaitTurn();
            m_userList.Add(m_owner);

            //! 유저 스타팅포인트 지정            
            Map map = SingleGameManager.Instance.m_currentMap;
            List<int> usedStartingPointIndex = new List<int>();
            for (int i = 0; i < map.m_makeUnitPositions.Count;++i) usedStartingPointIndex.Add(i);
            MSettings.Shuffle(usedStartingPointIndex);
            for (int i = 0; i<m_userList.Count; ++i)
            {
                int index = usedStartingPointIndex[i];
                m_userList[i].Init(index, map.m_makeUnitPositions[index], map.m_attackPoint);
            }

            //! 유저리스트 배치(Focus용)
            PlayUI.Instance.ShowFocusToggle(true);

            //! 게임시작
            ChangeGameState(GameState.WaitReady);
        }

        MUser GetWaitTurnUser()
        {
            for (int i = 0; i < m_userList.Count; ++i)
            {
                if (m_userList[i].m_state != UserState.WaitTurn) continue;                
                return m_userList[i];
            }
            return null;
        }

        IEnumerator WaitReady()
        {
            MUser user = GetWaitTurnUser();
            while (user != null)
            {                                
                SetUser(user);
                MakeSquare(user.m_startingPosition);
                yield return StartCoroutine(Notice.Instance.Center(user.m_nickName, NoticeEffect.Fade, 3, 1));

                //! 생성 패널 보이기
                PlayUI.Instance.ShowSkipButton(user.m_id != m_owner.m_id);
                PlayUI.Instance.ShowMakeUnitPanel(user.m_id == m_owner.m_id);

                yield return StartCoroutine(m_currentUser.Process());
                MakeSquare(null);
                user = GetWaitTurnUser();
            }

            //! 생성 패널 감추기
            PlayUI.Instance.ShowSkipButton(false);
            PlayUI.Instance.ShowMakeUnitPanel(false);

            ChangeGameState(GameState.Play);
        }

        public void SetUser(MUser user)
        {
            m_currentUser = user;
            if (m_changeUserEvent != null) m_changeUserEvent(m_currentUser);
        }        

        public void MakeUnit(string userID, int unitID)
        {
            MUser user = GetUser(userID);
            if (user.IsCanMakeUnit() == false) return;

            Unit unit = MUnitManager.Instance.GetUnit(unitID);
            if (user.UseGold(unit.m_makePrice) == false) return;

            MUnit munit = MUnitManager.Instance.GetMUnit(unitID);            
            munit.m_level = user.GetUnitLevel(unit.m_id);
            munit.m_teamId = user.m_teamIndex;
            munit.transform.FindChild("Sprite").FindChild("TeamCircle").GetComponent<SpriteRenderer>().color = user.m_startingPosition.m_color;
            Vector2 pos = user.GetSpawnPoint();
            munit.transform.position =  RpgMapHelper.GetTileCenterPosition((int)pos.x, (int)pos.y);
            munit.Init(unit);

            user.MakeUnit(munit);

            MGameCamera.Instance.SetTarget(munit.transform.position);
        }

        public bool UpgradeUnit(string userID, int unitID)
        {
            MUser user = GetUser(userID);
            int level = user.GetUnitLevel(unitID);
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
       
        private bool IsCanMakeUnit()
        {
            if (m_currentUser == null) return false;
            return m_currentUser.IsCanMakeUnit();
        }
        
        IEnumerator ClearStage()
        {
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
        IEnumerator Play()
        {
            for (int i = 0; i < m_userList.Count; ++i)
            {
                m_userList[i].Play();
            }

            yield return new WaitForSeconds(1.0f);

            //! 적으로 돌격

            if (m_isAutoTarget) StartCoroutine(AutoAttackGround(1.0f));
            else
            {
                for (int i = 0; i < m_userList.Count; ++i)
                {
                    int targetIndex = i + 1;
                    if (targetIndex >= m_userList.Count) targetIndex = 0;
                    Vector2 pos = m_userList[targetIndex].m_attackPoint;
                    m_userList[i].AttackGround(RpgMapHelper.GetTileCenterPosition((int)pos.x, (int)pos.y));
                }
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

            if (m_owner.m_state != UserState.Dead) ChangeGameState(GameState.ClearStage);
            else ChangeGameState(GameState.Result);
        }

        IEnumerator AutoAttackGround(float delay)
        {
            while(m_state == GameState.Play)
            {
                AutoAttackAllUser();
                yield return new WaitForSeconds(delay);
            }
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

        private void AutoAttackAllUser()
        {   
            for (int i = 0; i < m_userList.Count; ++i)
            {
                MUser user = m_userList[i];
                if (user.m_state != UserState.Play) continue;

                int targetIndex = i;
                MUser targetUser = null;
                while(targetUser == null)
                {
                    targetIndex += 1;
                    if (targetIndex >= m_userList.Count) targetIndex = 0;
                    targetUser = m_userList[targetIndex];
                    //! 자기 밖에 안남았으니 끝낸다.
                    if (targetUser == user) return;
                    if (targetUser.m_state != UserState.Play) { targetUser = null; continue; }
                }

                Debug.Log(user.m_nickName + " 이 " + targetUser.m_nickName + " 에게 공격합니다.");
                Vector3 attackPosition = Vector3.zero;
                if (GetUserUnitCenterPosition(targetUser.m_id, ref attackPosition) == true) user.AttackGround(attackPosition);
            }
        }

        public bool GetUserUnitCenterPosition(string userID, ref Vector3 position)
        {
            MUser user = GetUser(userID);
            if (user == null || user.m_aliveUnits.Count == 0) return false;

            //! 중심 위치 선정
            Vector3 totalPosition = Vector3.zero;
            for(int i = 0;i< user.m_aliveUnits.Count;++i)
            {
                totalPosition += user.m_aliveUnits[i].transform.position;
            }
            position = totalPosition / user.m_aliveUnits.Count;

            //! 중심에서 가장 가까운 유닛의 위치를 타겟으로삼는다.
            int unitIndex = -1;
            float sqrMagnitude = 0;            
            for (int i = 0; i < user.m_aliveUnits.Count; ++i)
            {
                bool isChoose = false;
                float tempMagnitue = Vector3.SqrMagnitude(totalPosition - user.m_aliveUnits[i].transform.position);

                if (unitIndex == -1) isChoose = true;
                else if (sqrMagnitude > tempMagnitue) isChoose = true;
                    
                if(isChoose == true)
                {
                    unitIndex = i;
                    sqrMagnitude = tempMagnitue;
                }
            }
            return true;
        }


        List<GameObject> m_makeSquares = new List<GameObject>();        
        public void MakeSquare(StartingPoint startingPoint)
        {
            if(startingPoint == null)
            {
                for (int i = 0; i < m_makeSquares.Count; ++i)
                {
                    m_makeSquares[i].SetActive(false);
                }

                return;
            }

            Color color = startingPoint.m_color;
            color.a = 0.5f;
            for (int i = 0;i<startingPoint.m_positions.Count;++i)
            {
                while (i >= m_makeSquares.Count)
                {
                    GameObject obj = Instantiate(ResourcesManager.Load("MakeSquare")) as GameObject;
                    m_makeSquares.Add(obj);
                }
                m_makeSquares[i].SetActive(true);
                m_makeSquares[i].GetComponent<SpriteRenderer>().color = color;
                Vector2 position = startingPoint.m_positions[i];
                m_makeSquares[i].transform.position = RpgMapHelper.GetTileCenterPosition((int)position.x, (int)position.y);
            }
        }
    }
}
