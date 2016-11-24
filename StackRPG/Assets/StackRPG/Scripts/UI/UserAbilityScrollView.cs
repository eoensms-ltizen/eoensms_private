using UnityEngine;
using System.Collections.Generic;
using stackRPG;
using UnityEngine.UI;

public class UserAbilityScrollView : MonoBehaviour
{
    public GameObject m_prefab;
    public ScrollRect m_scrollRect;
    private List<UserAbilityBar> m_abilityBar = new List<UserAbilityBar>();
    

    public void InitScrollView(UserData userData)
    {
        ClearAbilityBar();

        if (userData == null) return;

        User user = userData.m_user;

        for(int i = 0;i<AbilityManager.Instance.m_userAility.Count;++i)
        {
            AbilityType abilityType = AbilityManager.Instance.m_userAility[i].m_userAbility.m_abilityType;

            AbilityData abilityData = user.m_haveAbility.Find(x => x.m_abilityType == abilityType);
            if (abilityData == null) abilityData = new AbilityData(abilityType, 0);

            UserAbilityBar makeUnitBar = GetUnitBar(i);

            makeUnitBar.SetAbility(abilityData);
            makeUnitBar.gameObject.SetActive(true);
        }
    }

    void ClearAbilityBar()
    {
        for(int i = 0;i<m_abilityBar.Count;++i)
        {   
            m_abilityBar[i].gameObject.SetActive(false);
        }
    }

    UserAbilityBar GetUnitBar(int index)
    {
        while(index >= m_abilityBar.Count)
        {
            RectTransform rectTransform = Instantiate(m_prefab).GetComponent<RectTransform>();
            rectTransform.SetParent(m_scrollRect.content);
            rectTransform.localScale = Vector3.one;
            m_abilityBar.Add(rectTransform.GetComponent<UserAbilityBar>());
        }
        
        return m_abilityBar[index];
    }
}
