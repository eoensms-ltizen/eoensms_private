using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class MMainMenu : MonoBehaviour
{
    public Camera m_camera;

    public Button m_startButton;

    public Button m_cashButton;
    private Text m_cashText;

    public UnitScrollView m_unitScrollView;
    public UserAbilityScrollView m_userAbilityScrollView;

    void Awake()
    {
        m_unitScrollView.InitScrollView(MUserManager.Instance.m_userData);

        m_userAbilityScrollView.InitScrollView(MUserManager.Instance.m_userData);

        m_startButton.onClick.RemoveAllListeners();
        m_startButton.onClick.AddListener(StartGame);
        
        m_cashText = m_cashButton.GetComponentInChildren<Text>();
        OnChangeCash();
        m_cashButton.onClick.RemoveAllListeners();
        m_cashButton.onClick.AddListener(() => { MUserManager.Instance.AddCash(100000); });
    }

    void OnEnable()
    {
        MUserManager.Instance.m_changeCashEvent += OnChangeCash;
    }
    void OnDisable()
    {
        if (MUserManager.m_isAlive == true) MUserManager.Instance.m_changeCashEvent -= OnChangeCash;
    }

    void OnChangeCash()
    {
        m_cashText.text = string.Format("{0:n0}", MUserManager.Instance.m_userData.m_user.m_cash);
    }

    void Start()
    {  
        StartCoroutine(MLoading.Instance.ReverseAnimation(m_camera));
    }

    public void StartGame()
    {
        StartCoroutine(StartGameAnimation());
    }

    void SetNickName(string name)
    {
        MUserManager.Instance.SetNickname(name);
    }

    IEnumerator StartGameAnimation()
    {
        m_cashButton.gameObject.SetActive(false);
        m_startButton.gameObject.SetActive(false);        
        m_unitScrollView.gameObject.SetActive(false);
        m_userAbilityScrollView.gameObject.SetActive(false);
        
        yield return StartCoroutine(Notice.Instance.Center("전투에 참여합니다.", NoticeEffect.Typing, 3, 1));
        MLoading.Instance.LoadScene("MPlay");
    }
}
