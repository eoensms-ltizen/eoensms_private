using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public Button m_startButton;
    public InputField m_nicknameField;

    void Awake()
    {
        m_startButton.onClick.RemoveAllListeners();
        m_startButton.onClick.AddListener(OnClickMakeUser);
    }


    IEnumerator Start()
    {
        SoundManager.Instance.PlayBGM("login");

        bool isAlreadyLogin = false;

        Notice.Instance.CenterAppear("[Anip] Attack 땅!", NoticeEffect.Typing, 1.0f);
        yield return new WaitForSeconds(2.0f);

        isAlreadyLogin = MUserManager.Instance.ShowLoadDialog();
        while (MUserManager.Instance.m_userData == null) yield return null;

        Notice.Instance.CenterDisappear("[Anip] Attack 땅!", NoticeEffect.Typing, 2.0f);
        yield return new WaitForSeconds(1.0f);

        if (isAlreadyLogin == true) StartCoroutine(StartGameAnimation());
        else
        {
            m_nicknameField.gameObject.SetActive(true);
            m_startButton.gameObject.SetActive(true);
        }
    }

    public void OnClickMakeUser()
    {
        string nickname = m_nicknameField.text.Trim();
        if (nickname.Length == 0)
        {
            m_nicknameField.text = "";
            SetNickName("");
            StartCoroutine(Notice.Instance.Bottom("이름이 비었습니다.", NoticeEffect.Fade, 3, 1));
            return;
        }
        else { SetNickName(nickname); }

        StartCoroutine(StartGameAnimation());
    }

    void SetNickName(string name)
    {
        MUserManager.Instance.SetNickname(name);
    }

    IEnumerator StartGameAnimation()
    {
        m_startButton.gameObject.SetActive(false);
        m_nicknameField.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Notice.Instance.Center(MUserManager.Instance.GetNickname(), NoticeEffect.Typing, 3, 1));
        yield return StartCoroutine(Notice.Instance.Center("시작", NoticeEffect.Typing, 3, 1));
        yield return new WaitForSeconds(0.5f);

        MLoading.Instance.LoadScene("MainMenu");
    }
}
