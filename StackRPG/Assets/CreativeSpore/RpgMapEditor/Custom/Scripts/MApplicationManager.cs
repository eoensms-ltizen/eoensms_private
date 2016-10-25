using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MApplicationManager : Singleton<MApplicationManager>
{
    //! 어플리케이션 전반에 걸친 셋팅을 한다.
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Application.runInBackground = true;
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "MainMenu") Application.Quit();
            else SceneManager.LoadScene("MainMenu"); 
        }
    }
}
