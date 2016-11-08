using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MMainMenu : MonoBehaviour
{
    public void StartGame()
    {
        MLoading.Instance.LoadScene("MPlay");
    }
}
