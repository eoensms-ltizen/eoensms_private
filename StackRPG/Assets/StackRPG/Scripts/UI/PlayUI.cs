using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

namespace stackRPG
{
    public enum MakeUX
    {
        ListView,
        NonListView,
    }

    public class PlayUI : MonoBehaviour
    {
        public static PlayUI Instance { get; private set; }

        private MUser m_user;

        public GameObject m_state;
        private Text m_stage;
        private Text m_gold;
        private Button m_readyButton;
        private Button m_switchButton;

        private Button m_skip;

        public GameObject m_readyPanel;
        private GameObject m_makeUnitBar;
        private List<RectTransform> m_makeUnitPanels = new List<RectTransform>();
        private ScrollRect m_scrollRect;

        public GameObject m_cameraFocus;
        private RectTransform _cameraFocus;
        private GameObject _userButton;
        private Dictionary<string, Button> m_focusToggles = new Dictionary<string, Button>();

        public GameObject m_unitPositionScrollbar;
        private int m_unitpositionIndex;
        private Scrollbar m_scrollbar;
        private Button m_beforeButton;
        private Button m_nextButton;

        public GameObject m_freeMove;
        private Toggle m_freeMovetoggle;

        public GameObject m_unitPanel;
        private UnitPanel _unitPanel;

        public MakeUX m_makeUX;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else if(Instance != this)
            {
                Destroy(transform.gameObject);
            }
        }

        void Update()
        {
            //! 임시로 셀랙하는 녀석으로 사용한다.
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Ray2D ray = new Ray2D(wp, Vector2.zero);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("MakeSquare")) { SetUnitPosition(m_makeSquares.IndexOf(hit.transform.gameObject)); }
                    else if (hit.collider.CompareTag("Unit"))
                    {
                        SetMakePanel(MakeUX.NonListView);
                        MUnit munit = hit.transform.GetComponent<MUnit>();
                        _unitPanel.SetUnit(munit);                        
                    }
                }
            }
            
            if(Input.GetMouseButtonUp(0))
            {
                if (_unitPanel.m_state == UnitPanelState.EnemyUnit) _unitPanel.UnSetUnit();
            }
        }

        public void Init()
        {
            if(m_state != null)
            {
                m_stage = m_state.transform.FindChild("Stage").GetComponentInChildren<Text>();
                m_gold = m_state.transform.FindChild("Gold").GetComponentInChildren<Text>();

                m_readyButton = m_state.transform.FindChild("Ready").GetComponent<Button>();
                m_readyButton.onClick.RemoveAllListeners();
                m_readyButton.onClick.AddListener(() => { StartStage(); });

                m_switchButton = m_state.transform.FindChild("Switch").GetComponent<Button>();
                m_switchButton.onClick.RemoveAllListeners();
                m_switchButton.onClick.AddListener(() => { SwitchMakePanel(); });

                m_skip = m_state.transform.FindChild("Skip").GetComponent<Button>();
                m_skip.onClick.RemoveAllListeners();
                m_skip.onClick.AddListener(() => { Skip(); });
            }

            if(m_readyPanel != null)
            {
                m_makeUnitBar = ResourcesManager.Load("MakeUnitBar") as GameObject;
                m_scrollRect = m_readyPanel.transform.FindChild("Scroll View").GetComponent<ScrollRect>();
            }

            if(m_cameraFocus != null)
            {
                _cameraFocus = m_cameraFocus.GetComponent<RectTransform>();
                _userButton = ResourcesManager.Load("CameraFocusButton") as GameObject;
            }

            if(m_unitPositionScrollbar != null)
            {
                m_scrollbar = m_unitPositionScrollbar.GetComponentInChildren<Scrollbar>();
                m_scrollbar.onValueChanged.RemoveAllListeners();
                m_scrollbar.onValueChanged.AddListener(OnChangeUnitposition);

                m_nextButton = m_unitPositionScrollbar.transform.FindChild("NextButton").GetComponent<Button>();
                m_nextButton.onClick.RemoveAllListeners();
                m_nextButton.onClick.AddListener(OnNextUnitPosition);

                m_beforeButton = m_unitPositionScrollbar.transform.FindChild("BeforeButton").GetComponent<Button>();
                m_beforeButton.onClick.RemoveAllListeners();
                m_beforeButton.onClick.AddListener(OnBeforeUnitPosition);
            }

            if(m_freeMove != null)
            {
                m_freeMovetoggle = m_freeMove.GetComponentInChildren<Toggle>();
                m_freeMovetoggle.onValueChanged.RemoveAllListeners();
                m_freeMovetoggle.onValueChanged.AddListener((value)=>{ MGameManager.Instance.SetFreeMove(value); });
            }

            if(m_unitPanel!=null)
            {
                _unitPanel = m_unitPanel.GetComponent<UnitPanel>();                
            }

            m_user = null;

            ShowFocusButton(false);
            ShowMakeUnitPanel(false);
            ShowSkipButton(false);
            ShowUnitPositionPanel(false);
        }

        public void ShowUnitPositionPanel(bool value)
        {
            m_unitPositionScrollbar.SetActive(value);
        }

        public void ShowSkipButton(bool value)
        {
            m_skip.gameObject.SetActive(value);
        }

        public void ShowMakeUnitPanel(bool value)
        {
            m_makeUXActive = value;
            
            switch(m_makeUX)
            {
                case MakeUX.ListView:
                    {
                        m_readyPanel.SetActive(value);
                        if (value == true) InitScrollView();
                    }
                    break;
                case MakeUX.NonListView:
                    {
                        m_unitPanel.SetActive(value);
                        if (value == true) InitUnitPanel();
                    }
                    break;
            }

            //! 부가적인 녀석들
            m_switchButton.gameObject.SetActive(value);
            m_readyButton.gameObject.SetActive(value);
        }

        bool m_makeUXActive = false;
        public void SwitchMakePanel()
        {
            MakeUX makeUX = m_makeUX;
            switch(m_makeUX)
            {
                case MakeUX.ListView: makeUX = MakeUX.NonListView; break;
                case MakeUX.NonListView: makeUX = MakeUX.ListView; break;
            }

            if (m_makeUXActive == false) { m_makeUX = makeUX; return; }
            else
            {
                ShowMakeUnitPanel(false);
                m_makeUX = makeUX;
                ShowMakeUnitPanel(true);
            }
        }

        public void SetMakePanel(MakeUX makeUX)
        {
            if (m_makeUX == makeUX) return;

            SwitchMakePanel();
        }

        public void ShowFreeMoveToggle(bool value)
        {
            m_freeMove.SetActive(value);
        }

        public void ShowFocusButton(bool value)
        {
            if (value == true)
            {
                int userCount = MGameManager.Instance.m_userList.Count;
                //_cameraFocus.sizeDelta = new Vector2(100, 150 * userCount);
                foreach (KeyValuePair<string, Button> obj in m_focusToggles)
                {
                    Destroy(obj.Value.gameObject);
                }
                m_focusToggles.Clear();                
                for (int i = 0; i < userCount; ++i)
                {
                    MUser user = MGameManager.Instance.m_userList[i];

                    //! 기본 설정
                    RectTransform rectTransform = Instantiate(_userButton).GetComponent<RectTransform>();
                    rectTransform.SetParent(_cameraFocus);
                    rectTransform.localScale = Vector3.one;

                    Button button = rectTransform.GetComponent<Button>();

                    //! 이벤트
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => { MGameCamera.Instance.OnFocusUserStartingPosition(user);});

                    //! 디자인
                    //button.image.color = user.m_startingPosition.m_color;
                    Text text = button.GetComponentInChildren<Text>();
                    text.text = user.m_nickName;
                    text.color = user.m_startingPosition.m_color;

                    m_focusToggles.Add(user.m_id, button);
                }
            }

            m_cameraFocus.SetActive(value);
        }

        public void OnChangeUser(MUser user)
        {
            RemoveUserStateChageEvent();
            
            m_user = user;

            AddUserStateChageEvent();

            InitMarkUnit();
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

        public void DrawStage(int stageNumber)
        {
            m_stage.text = string.Format("Stage\n{0}", stageNumber);
        }
        private void InitMarkUnit()
        {
            if (m_user == null) return;

            m_unitpositionIndex = 0;
            m_scrollbar.value = 0;
            m_scrollbar.numberOfSteps = m_user.m_startingPosition.m_positions.Count;
            OnChangeUnitposition(0);
        }

        private void InitUnitPanel()
        {
            if(m_user == null) return;

            _unitPanel.Init(m_user);

        }
        private void InitScrollView()
        {
            while (m_makeUnitPanels.Count > 0)
            {
                RectTransform transform = m_makeUnitPanels[0];
                m_makeUnitPanels.RemoveAt(0);
                Destroy(transform.gameObject);
            }

            if (m_user == null) return;
            
            for (int i = 0; i< m_user.m_haveUnit.Count;++i)
            {
                Unit unit = MUnitManager.Instance.GetUnit(m_user.m_haveUnit[i].m_id);
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
            m_user.Ready();
            m_readyPanel.SetActive(false);
        }

        public void Skip()
        {
            MGameManager.Instance.m_currentUser.Skip();
            m_skip.gameObject.SetActive(false);
        }

        public void SetUnitPosition(int index)
        {
            m_scrollbar.value = index / ((float)m_scrollbar.numberOfSteps - 1);
        }
        public void OnChangeUnitposition(float value)
        {
            m_unitpositionIndex = Convert.ToInt32(value / (1.0f / (float)(m_scrollbar.numberOfSteps - 1)));            
                        
            MGameManager.Instance.MarkIndex(m_user, m_unitpositionIndex);
        }
        public void OnNextUnitPosition()
        {   
            m_scrollbar.value += 1.0f / ((float)m_scrollbar.numberOfSteps - 1);
        }

        public void OnBeforeUnitPosition()
        {
            m_scrollbar.value -= 1.0f / ((float)m_scrollbar.numberOfSteps - 1);
        }

        GameObject m_markSquare = null;
        public void MarkSquare(Color color, Vector3 position)
        {
            if (m_markSquare == null) m_markSquare = Instantiate(ResourcesManager.Load("MarkSquare")) as GameObject;

            m_markSquare.GetComponent<SpriteRenderer>().color = color;
            m_markSquare.transform.position = position;
            m_markSquare.SetActive(true);
        }
        public void RemoveMarkSquare()
        {
            if (m_markSquare != null) m_markSquare.SetActive(false);
        }

        List<GameObject> m_makeSquares = new List<GameObject>();

        public void MakeSquare(List<Vector3> positions, Color color)
        {
            RemoveMakeSquare();
            
            for (int i = 0; i < positions.Count; ++i)
            {
                while (i >= m_makeSquares.Count)
                {
                    GameObject obj = Instantiate(ResourcesManager.Load("MakeSquare")) as GameObject;
                    m_makeSquares.Add(obj);
                }

                m_makeSquares[i].SetActive(true);
                m_makeSquares[i].GetComponent<SpriteRenderer>().color = color;                
                m_makeSquares[i].transform.position = positions[i]; 
            }
        }

        public void RemoveMakeSquare()
        {
            for (int i = 0; i < m_makeSquares.Count; ++i)
            {
                m_makeSquares[i].SetActive(false);
            }
        }
    }
}

