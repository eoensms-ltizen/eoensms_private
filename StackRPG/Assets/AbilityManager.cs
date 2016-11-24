using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityManager : Singleton<AbilityManager>
{
    public List<UserAbilityData> m_userAility;
    private Dictionary<AbilityType, UserAbility> m_abilitys = new Dictionary<AbilityType, UserAbility>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        m_abilitys = new Dictionary<AbilityType, UserAbility>();
        for (int i = 0; i < m_userAility.Count; ++i)
        {
            UserAbility userAbility = m_userAility[i].m_userAbility;
            m_abilitys.Add(userAbility.m_abilityType, userAbility);
        }

        if (Instance != this)
        {
            Destroy(transform.gameObject);
        }
    }

    public void AttachAility(MUser muser)
    {
        for (int i = 0;i< m_userAility.Count;++i)
        {
            UserAbility userAbility = m_userAility[i].m_userAbility;

            switch (userAbility.m_abilityType)
            {
                case AbilityType.Charisma: new CharismaAbility(userAbility.m_valueFloat).AttachAbility(muser); break;
                case AbilityType.Lucky: new LuckyAbility(userAbility.m_valueFloat).AttachAbility(muser); break;
                case AbilityType.Smart: new SmartAbility(userAbility.m_valueFloat).AttachAbility(muser); break;
            }
        }
    }

    public void DitachAility(MUser muser, AbilityData abilityData)
    {
        //! 할일이 있나?
    }

    public UserAbility GetUserAbility(AbilityType abilityType)
    {
        return m_abilitys[abilityType];
    }
}
