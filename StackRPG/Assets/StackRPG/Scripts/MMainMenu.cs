using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class MMainMenu : MonoBehaviour
{
    public Camera m_camera;

    public Button m_startButton;
    public InputField m_nicknameField;
    void Start()
    {
        string nickname = MSettings.GetNickname().Trim();
        if(nickname.Length > 0) { m_nicknameField.text = nickname; }
        StartCoroutine(MLoading.Instance.ReverseAnimation(m_camera));
    }
    public void StartGame()
    {
        string nickname = m_nicknameField.text.Trim();
        if (nickname.Length == 0)
        {
            m_nicknameField.text = "";
            MSettings.SetNickname("");
            StartCoroutine(Notice.Instance.Bottom("이름이 비었습니다.", NoticeEffect.Fade, 3, 1));
            return;
        }
        else { MSettings.SetNickname(nickname); }

        StartCoroutine(StartGameAnimation());
    }

    IEnumerator StartGameAnimation()
    {
        m_startButton.gameObject.SetActive(false);
        m_nicknameField.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Notice.Instance.Center(m_nicknameField.text, NoticeEffect.Typing, 3, 1));
        yield return StartCoroutine(Notice.Instance.Center("시작", NoticeEffect.Typing, 3, 1));
        yield return new WaitForSeconds(0.5f);

        MLoading.Instance.LoadScene("MPlay");
    }
}
