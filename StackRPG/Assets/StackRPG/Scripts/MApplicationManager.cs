using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MApplicationManager : Singleton<MApplicationManager>
{   
    public static float width = 1280;
    public static float height = 720;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        Screen.SetResolution((int)width, (int)height, true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "MainMenu") Application.Quit();
            else SceneManager.LoadScene("MainMenu"); 
        }
    }
}
