using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourcesManager : MonoBehaviour {

    public ResourcesManager() { m_instance = this; }
    static ResourcesManager m_instance;
    public static ResourcesManager Instance { get { return m_instance; } }

    private static Dictionary<int, Object> m_objectPool = new Dictionary<int, Object>();
    private static Dictionary<int, Sprite> m_spritePool = new Dictionary<int, Sprite>();

    public static Object Load(string path)
    {
        int hash = path.GetHashCode();
        if (m_objectPool.ContainsKey(hash) == false) { m_objectPool.Add(hash, Resources.Load(path)); }
        return m_objectPool[hash];
    }

    public static Sprite LoadSprite(string path)
    {
        int hash = path.GetHashCode();
        if (m_spritePool.ContainsKey(hash) == false) { m_spritePool.Add(hash, Resources.Load<Sprite>(path)); }
        return m_spritePool[hash];
    }      
}
