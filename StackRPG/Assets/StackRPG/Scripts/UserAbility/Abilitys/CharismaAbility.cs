using UnityEngine;
using System.Collections;
using System;

public class CharismaAbility : MUserAbility
{
    private float m_persentValue;
    public CharismaAbility(float value)
    {
        m_abilityType = AbilityType.Charisma;
        m_persentValue = value;
    }

    public override void AttachAbility(MUser user)
    {
        m_muser = user;

        user.m_takeLevelGoldEvent += OnTakeLevelGold;
    }

    public override void DetachAbility(MUser user)
    {
        user.m_takeLevelGoldEvent -= OnTakeLevelGold;

        m_muser = null;
    }

    void OnTakeLevelGold(int gold)
    {
        int level = m_muser.GetAbilityData(m_abilityType).m_level;
        int addGold = (int)(gold * (m_persentValue * level));
        Notice.Instance.SystemLog(string.Format("{0}(lv{1}) : ${2}", AbilityManager.Instance.GetUserAbility(m_abilityType).m_name, level, addGold));
        m_muser.AddGold(addGold);
    }
}
