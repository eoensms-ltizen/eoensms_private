using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace stackRPG
{
    public class PlayUI : MonoBehaviour
    {
        private User m_user;

        public Text m_stage;
        public Text m_gold;
        public Button m_startButton;

        public List<Unit> m_unitTable = new List<Unit>();
        
        public GameObject m_makeUnitPanel;
        private List<RectTransform> m_makeUnitPanels = new List<RectTransform>();
        public ScrollRect m_scrollRect;
        void Awake()
        {
            m_user = null;
        }
        void OnEnable()
        {
            MGameManager.Instance.m_changeUserEvent += Init;
        }

        void OnDisable()
        {
            MGameManager.Instance.m_changeUserEvent -= Init;
        }

        public void Init(User user)
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
            m_user.m_changeStageEvent -= DrawStage;
        }

        private void AddUserStateChageEvent()
        {
            if (m_user == null) return;

            m_user.m_changeGoldEvent += DrawGold;
            m_user.m_changeStageEvent += DrawStage;

            DrawGold();
            DrawStage();
        }

        void DrawGold()
        {
            m_gold.text = string.Format("Gold {0}", m_user.m_gold);
        }

        void DrawStage()
        {
            m_stage.text = string.Format("Gold {0}", m_user.m_stageNumber);
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
            for(int i = 0; i< m_unitTable.Count;++i)
            {
                RectTransform rectTransform = Instantiate(m_makeUnitPanel).GetComponent<RectTransform>();
                rectTransform.SetParent(m_scrollRect.content);
                rectTransform.localScale = Vector3.one;

                MakeUnitBar makeUnitBar = rectTransform.GetComponent<MakeUnitBar>();
                makeUnitBar.SetUnit(m_unitTable[i]);

                m_makeUnitPanels.Add(rectTransform);
            }
        }

        public void GameOver()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void StartStage()
        {
            MGameManager.Instance.StartStage();
            //! UI 제거, 
            //! 전투 AI 작동
            //! 전투 종료(전장에 하나의 팀만 남았을경우 종료된다)
        }
    }
}

