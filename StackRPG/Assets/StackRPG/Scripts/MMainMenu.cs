using UnityEngine;
public class MMainMenu : MonoBehaviour
{
    public Camera m_camera;
    void Start()
    {
        StartCoroutine(MLoading.Instance.ReverseAnimation(m_camera));
    }
    public void StartGame()
    {
        MLoading.Instance.LoadScene("MPlay");
    }
}
