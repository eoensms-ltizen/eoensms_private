using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CommonLog : Singleton<CommonLog>
{
    private bool isShowWindow = false;
    static string myLog = "";
    private string output;
    private string stack;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width * 0.25f, Screen.height * 0.9f, Screen.width * 0.5f, Screen.height * 0.1f), output))
        {
            isShowWindow = !isShowWindow;
        }

        if (isShowWindow == true)
        {
            myLog = GUI.TextArea(new Rect(10, 10, Screen.width - 10, Screen.height - 10), myLog);
        }
    }

    public void ShowLog(string text)
    {
        MSettings.Log(text);
    }
}
