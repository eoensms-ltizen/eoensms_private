using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class User
{
    public string m_id;
    public string m_nickName;

    public User(string id, string nickName)
    {
        m_id = id;
        m_nickName = nickName;
    }
}
