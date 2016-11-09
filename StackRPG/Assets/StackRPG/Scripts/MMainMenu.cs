using UnityEngine;
public class MMainMenu : MonoBehaviour
{
    public void StartGame()
    {
        MLoading.Instance.LoadScene("MPlay");
    }
}
