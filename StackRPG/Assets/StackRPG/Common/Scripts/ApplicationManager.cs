using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ApplicationManager : Singleton<ApplicationManager>
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Application.runInBackground = true;
        Application.targetFrameRate = 60;
    }
	
	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
	}

    public enum Scene
    {
        TitleScene,
        LobbyScene,
        GameScene,
        QuickLobbyScene,
        QuickGameScene,
    }

    public Scene m_currentScene;

    private void ChangeScene(Scene scene)
    {
        m_currentScene = scene;
        SceneManager.LoadScene(scene.ToString());
    }

    public void EndGame()
    {
        ChangeScene(Scene.QuickLobbyScene);
    }
    public void StartGame()
    {
        ChangeScene(Scene.GameScene);
    }

    
}
