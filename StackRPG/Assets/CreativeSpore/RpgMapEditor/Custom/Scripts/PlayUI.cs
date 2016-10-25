using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace stackRPG
{
    public class PlayUI : MonoBehaviour
    {
        private MUser m_user;

        public GameObject m_state;
        private Text m_stage;
        private Text m_gold;
        private Button m_startStage;

        public GameObject m_readyPanel;
        private GameObject m_makeUnitPanel;
        private List<RectTransform> m_makeUnitPanels = new List<RectTransform>();
        private ScrollRect m_scrollRect;
        void Awake()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            yield return StartCoroutine(MGameManager.Instance.WaitPrecess());

            if(m_state != null)
            {
                m_stage = m_state.transform.FindChild("Stage").GetComponentInChildren<Text>();
                m_gold = m_state.transform.FindChild("Gold").GetComponentInChildren<Text>();
                m_startStage = m_state.transform.FindChild("StartStage").GetComponent<Button>();
            }

            if(m_readyPanel != null)
            {
                m_makeUnitPanel = ResourcesManager.Load("MakeUnitBar") as GameObject;
                m_scrollRect = m_readyPanel.transform.FindChild("Scroll View").GetComponent<ScrollRect>();
            }

            m_user = null;
            MGameManager.Instance.m_changeUserEvent += SetUser;
            MGameManager.Instance.m_changeGameState += OnChangeGameState;
            MGameManager.Instance.m_changeStageNumber += DrawStage;
        }

        void OnChangeGameState(GameState state)
        {
            switch(state)
            {
                case GameState.Init:                                    
                case GameState.Play:
                case GameState.ChangeTurn:
                    {
                        m_state.gameObject.SetActive(false);
                        m_readyPanel.gameObject.SetActive(false);
                    }
                    break;
                case GameState.WaitReady:
                    {
                        m_state.gameObject.SetActive(true);
                        m_readyPanel.gameObject.SetActive(true);
                    }
                    break;
            }
        }
        public void SetUser(MUser user)
        {
            RemoveUserStateChageEvent();
            ClearScrollView();

            m_user = user;

            AddUserStateChageEvent();
            InitScrollView();
        }

        private void RemoveUserStateChageEvent()
        {
            if (m_user == null) return;

            m_user.m_changeGoldEvent -= DrawGold;
        }

        private void AddUserStateChageEvent()
        {
            if (m_user == null) return;

            m_user.m_changeGoldEvent += DrawGold;

            DrawGold();
        }

        void DrawGold()
        {
            m_gold.text = string.Format("Gold\n{0}", m_user !=null? m_user.m_gold : 0);
        }

        void DrawStage(int stageNumber)
        {
            m_stage.text = string.Format("Stage\n{0}", stageNumber);
        }
        private void ClearScrollView()
        {
            while(m_makeUnitPanels.Count > 0)
            {
                RectTransform transform = m_makeUnitPanels[0];
                m_makeUnitPanels.RemoveAt(0);
                Destroy(transform.gameObject);
            }
        }

        private void InitScrollView()
        {
            if (m_user == null) return;
            
            for (int i = 0; i< MUnitManager.Instance.m_unitDatas.Count;++i)
            {
                Unit unit = MUnitManager.Instance.m_unitDatas[i].m_unitData;
                RectTransform rectTransform = Instantiate(m_makeUnitPanel).GetComponent<RectTransform>();
                rectTransform.SetParent(m_scrollRect.content);
                rectTransform.localScale = Vector3.one;

                MakeUnitBar makeUnitBar = rectTransform.GetComponent<MakeUnitBar>();
                makeUnitBar.SetUnit(unit);

                m_makeUnitPanels.Add(rectTransform);
            }
        }

        public void GameOver()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void StartStage()
        {
            if (m_user == null) return;
            m_user.Ready();
        }

        public void SwitchAuto()
        {
            if (m_user == null) return;
            m_user.m_isAuto = !m_user.m_isAuto;
        }
    }
}

