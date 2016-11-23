using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(T)) as T;

                if (_instance == null)
                {   
                    GameObject obj = (GameObject)Instantiate(ResourcesManager.Load(typeof(T).ToString()));
                    obj.name = typeof(T).ToString();
                    _instance = FindObjectOfType(typeof(T)) as T;
                }
            }
            return _instance;
        }
    }

    //! 임시로 한다.
    public static bool m_isAlive { get { return _instance == null ? false : true; } }

    public void Blink()
    {

    }
}

