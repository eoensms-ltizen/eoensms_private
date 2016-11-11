using UnityEngine;

public class AppManager
{
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        Debug.Log("After scene is loaded and game is running");

        MApplicationManager.Instance.Blink();
    }

    [RuntimeInitializeOnLoadMethod]
    static void OnSecondRuntimeMethodLoad()
    {
        Debug.Log("SecondMethod After scene is loaded and game is running.");
    }
}
