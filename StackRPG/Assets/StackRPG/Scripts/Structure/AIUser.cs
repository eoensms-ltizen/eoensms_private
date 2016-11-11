using UnityEngine;
using System;
using System.Collections.Generic;
using stackRPG;

[Serializable]
public class AIUser
{
    public float m_useGold_opend;
    public float m_useGold_upgrade;
    public float m_useGold_make;

    public List<LastUserAction> m_lastAction;
}
