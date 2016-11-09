using UnityEngine;
using System.Collections;

public class CommonInfo : Singleton<CommonInfo>
{
   //private bool isShowWindow = false;
   //static string myLog = "";
   //private string output;
   //private string stack;
   //void Awake()
   //{
   //    DontDestroyOnLoad(gameObject);
   //}
   //
   //public void Log(string logString, string stackTrace, LogType type)
   //{
   //    output = logString;
   //    stack = stackTrace;
   //    myLog = output + "\n" + myLog;
   //    if (myLog.Length > 5000)
   //    {
   //        myLog = myLog.Substring(0, 4000);
   //    }
   //}
   //
   //void OnGUI()
   //{
   //    
   //    if (GUI.Button(new Rect(10, 10, Screen.width * 0.1f, Screen.height * 0.1f), "Info"))
   //    {
   //        isShowWindow = !isShowWindow;
   //    }
   //
   //    if (isShowWindow == true)
   //    {
   //        myLog = GUI.TextArea(new Rect(10, 10, Screen.width - 10, Screen.height - 10), myLog);
   //    }
   //}
   //
   //public void ShowLog(string text)
   //{
   //    MSettings.Log(text);
   //}
}