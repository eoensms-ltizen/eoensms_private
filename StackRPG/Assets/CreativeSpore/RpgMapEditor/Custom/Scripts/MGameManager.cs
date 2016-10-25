using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace stackRPG
{
    public enum GameState
    {
        Init,
        StartStage,
        ChangeTurn,
        WaitReady,
        Play,
    }

    public class MGameManager : Singleton<MGameManager>
    {
        public string m_ownerID;
        public UserData m_baseUserData;
        public MUser m_owner;

        public GameState m_state;
        public int m_stageNumber;
        public int m_selectUserIndex;
        public MUser m_user { get; private set; }

        public Action<MUser> m_changeUserEvent;
        public Action<GameState> m_changeGameState;
        public Action<int> m_changeStageNumber;

        public List<MUser> m_userList = new List<MUser>();
        public Dictionary<Guid, MUnit> m_units = new Dictionary<Guid, MUnit>();
        public LinkedList<MUnit> m_unitLinkedList = new LinkedList<MUnit>();

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
            m_owner = new MUser(m_baseUserData.m_user);
            m_userList.Add(m_owner);
            m_owner.m_id = m_ownerID;
        }

        private void ChangeGameState(GameState state)
        {
            m_state = state;
            if (m_changeGameState != null) m_changeGameState(m_state);

            switch(m_state)
            {
                case GameState.StartStage:
                    {
                        StartCoroutine(StartStage(m_stageNumber + 1));
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
                        //! UI 제거

                        //! 전투 AI 작동

                        //! 전투 종료(전장에 하나의 팀만 남았을경우 종료된다)

                        StartCoroutine(Fight());
                    }
                    break;
            }
        }

        IEnumerator StartStage(int stageNumber)
        {
            SetStage(stageNumber);

            yield return new WaitForSeconds(0.5f);

            //! 맵, 유저 정리
            for (int i = 0; i < m_userList.Count; ++i)
            {
                m_userList[i].RemoveAllUnit();

                //! 유저가아닌 녀석은 없앤다.
                if (m_userList[i].m_id != m_ownerID)
                {
                    m_userList.RemoveAt(i);
                    i--;
                }
            }

            yield return new WaitForSeconds(0.5f);

            //! 다음스테이지 불러오기
            Stage stage = StageManager.Instance.m_stageDatas[stageNumber].m_stage;

            //! 보상주기
            MUser owner = GetUser(m_ownerID);
            if (owner != null) owner.SetGold(owner.m_gold + stage.m_gold);

            Debug.Log("보급품을 받았습니다. : $ " + stage.m_gold);

            yield return new WaitForSeconds(0.5f);

            //! 다음 유저 진입
            for (int i = 0; i < stage.m_enemys.Count; ++i)
            {
                m_userList.Add(new MUser(stage.m_enemys[i].m_user));
                Debug.Log("새로운 도전자가 진입하였습니다. : " + stage.m_enemys[i].m_user.m_id);
            }

            //! 유저 순서정렬 및 초기화
            m_userList.Remove(owner);
            owner.WaitTurn();
            m_userList.Add(owner);

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
            yield return StartCoroutine(m_user.Process());
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

        public Guid AddUnit(MUnit unit)
        {
            Guid guid = Guid.NewGuid();
            m_units.Add(guid, unit);
            m_unitLinkedList.AddLast(unit);
            return guid;
        }

        public void RemoveUnit(Guid guid)
        {
            m_unitLinkedList.Remove(m_units[guid]);
            m_units.Remove(guid);
        }     

        public void SetUser(MUser user)
        {
            m_user = user;
            if (m_changeUserEvent != null) m_changeUserEvent(m_user);
        }

        int unitCount = 0;

        

        public void MakeUnit(Unit unit)
        {
            if (UseGold(unit.m_makePrice) == false) return;

            MUnit munit = MUnitManager.Instance.GetUnit(unit);            
            munit.m_level = m_user.GetUnitLevel(unit.m_id);
            munit.m_teamId = m_user.m_teamId;
            munit.transform.position = m_user.m_startPoint;
            munit.Init(unit);
            m_user.MakeUnit(munit);
        }
        public bool UpgradeUnit(Unit unit)
        {
            int level = m_user.GetUnitLevel(unit.m_id);
            if (UseGold(unit.m_upgradeCost[level]) == false) return false;

            m_user.UpgradeUnit(unit.m_id);
            return true;
        }
        public bool OpenUnit(Unit unit)
        {
            if (UseGold(unit.m_openPrice) == false) return false;

            m_user.OpenUnit(unit.m_id);
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
            if (m_user == null) return false;
            if (m_user.m_gold < value) return false;

            m_user.SetGold(m_user.m_gold - value);
            return true;
        }
        
        IEnumerator Fight()
        {
            CommonLog.Instance.ShowLog("Total Unit : " + unitCount);

            //! 자기 위치에 해쳐모여
            for (int i = 0; i < m_userList.Count; ++i)
            {
                m_userList[i].MoveGround(m_userList[i].m_startPoint);
            }

            yield return new WaitForSeconds(1.0f);

            //! 적으로 돌격
            for (int i = 0; i < m_userList.Count; ++i)
            {
                int targetIndex = i + 1;
                if (targetIndex >= m_userList.Count) targetIndex = 0;

                m_userList[i].AttackGround(m_userList[targetIndex].m_startPoint);
            }
        }
    }
}
