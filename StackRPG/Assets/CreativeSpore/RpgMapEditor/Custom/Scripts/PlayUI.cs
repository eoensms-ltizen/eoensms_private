using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace stackRPG
{
    public class PlayUI : MonoBehaviour
    {
        public static PlayUI Instance { get; private set; }

        private MUser m_user;

        public GameObject m_state;
        private Text m_stage;
        private Text m_gold;
        private Button m_startStage;

        public GameObject m_readyPanel;
        private GameObject m_makeUnitBar;
        private List<RectTransform> m_makeUnitPanels = new List<RectTransform>();
        private ScrollRect m_scrollRect;

        public GameObject m_cameraFocus;
        private RectTransform _cameraFocus;
        private GameObject _userToggle;
        private Dictionary<string, Toggle> m_focusToggles = new Dictionary<string, Toggle>();
        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                Init();
            }
            else if(Instance != this)
            {
                Destroy(transform.gameObject);
            }
        }
        void Init()
        {
            if(m_state != null)
            {
                m_stage = m_state.transform.FindChild("Stage").GetComponentInChildren<Text>();
                m_gold = m_state.transform.FindChild("Gold").GetComponentInChildren<Text>();
                m_startStage = m_state.transform.FindChild("StartStage").GetComponent<Button>();
            }

            if(m_readyPanel != null)
            {
                m_makeUnitBar = ResourcesManager.Load("MakeUnitBar") as GameObject;
                m_scrollRect = m_readyPanel.transform.FindChild("Scroll View").GetComponent<ScrollRect>();
            }

            if(m_cameraFocus != null)
            {
                _cameraFocus = m_cameraFocus.GetComponent<RectTransform>();
                _userToggle = ResourcesManager.Load("CameraFocusToggle") as GameObject;
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
                case GameState.LoadMap:
                case GameState.StartStage:
                case GameState.Result:
                case GameState.Finish:
                case GameState.ClearStage:
                case GameState.Play:
                    {
                        m_startStage.gameObject.SetActive(false);
                        m_readyPanel.gameObject.SetActive(false);
                    }
                    break;
                case GameState.WaitReady:
                    {
                        //! 유저들 데이터는 여기서 결정나 있다.

                        int userCount = MGameManager.Instance.m_userList.Count;
                        _cameraFocus.sizeDelta = new Vector2(100, 150 * userCount);

                        
                        foreach(KeyValuePair<string, Toggle> value in m_focusToggles)
                        {   
                            Destroy(value.Value.gameObject);
                        }
                        m_focusToggles.Clear();
                        MGameCamera.Instance.ClearCameraFocus();
                        for (int i = 0; i < userCount; ++i)
                        {   
                            MUser user = MGameManager.Instance.m_userList[i];
                            RectTransform rectTransform = Instantiate(_userToggle).GetComponent<RectTransform>();
                            rectTransform.SetParent(_cameraFocus);
                            rectTransform.localScale = Vector3.one;
                            Toggle toggle = rectTransform.GetComponent<Toggle>();
                            toggle.onValueChanged.RemoveAllListeners();
                            toggle.onValueChanged.AddListener((value) => { MGameCamera.Instance.SetCameraFocus(user.m_id, value); });
                            m_focusToggles.Add(user.m_id, toggle);
                        }

                        m_startStage.gameObject.SetActive(true);
                        m_readyPanel.gameObject.SetActive(true);
                    }
                    break;
            }
        }

        public void SetCameraFocus(string id, bool value)
        {
            m_focusToggles[id].isOn = value;
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
                RectTransform rectTransform = Instantiate(m_makeUnitBar).GetComponent<RectTransform>();
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
    }
}

