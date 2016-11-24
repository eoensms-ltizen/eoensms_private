using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class User
{
    public string m_id;
    public string m_nickName;
    public int m_cash;
    public List<int> m_haveUnit;

    public List<AbilityData> m_haveAbility;

    public User()
    {

    }

    public User(User user)
    {
        m_id = user.m_id;
        m_nickName = user.m_nickName;
        m_cash = user.m_cash;

        m_haveUnit = new List<int>();
        for (int i= 0;i<user.m_haveUnit.Count;++i)
        {
            m_haveUnit.Add(user.m_haveUnit[i]);
        }

        m_haveAbility = new List<AbilityData>();
        for(int i =0;i<user.m_haveAbility.Count;++i)
        {
            m_haveAbility.Add(user.m_haveAbility[i]);
        }
    }
}
