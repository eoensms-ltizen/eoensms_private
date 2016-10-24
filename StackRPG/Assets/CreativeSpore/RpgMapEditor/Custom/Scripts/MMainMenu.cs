using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MMainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("MPlay");
    }
}
