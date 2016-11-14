using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    IEnumerator Start()
    {
        Notice.Instance.CenterAppear("[Anip] Attack 땅!", NoticeEffect.Typing, 1.0f);
        yield return new WaitForSeconds(2.0f);
        Notice.Instance.CenterDisappear("[Anip] Attack 땅!", NoticeEffect.Typing, 2.0f);
        yield return new WaitForSeconds(1.0f);

        SceneManager.LoadScene("MainMenu");
    }
}
