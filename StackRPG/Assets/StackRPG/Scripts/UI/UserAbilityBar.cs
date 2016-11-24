using UnityEngine;
using UnityEngine.UI;

public class UserAbilityBar : MonoBehaviour {
    public Button m_background;
    public Image m_icon;
    public Text m_name;    
    public Button m_upgradebutton;
    public Text m_upgradeCostText;

    public Color m_haveColor = Color.white;
    public Color m_nonHaveColor = new Color(0, 0, 0, 0.8f);

    public void SetAbility(AbilityData abilityData)
    {
        UserAbility userAbility = AbilityManager.Instance.GetUserAbility(abilityData.m_abilityType);

        m_name.text = string.Format("{0}", userAbility.m_name);

        int level = abilityData.m_level;

        bool isMaxLevel = level >= userAbility.m_upgradeCost.Count - 1;

        if(isMaxLevel == true)
        {
            m_upgradebutton.enabled = false;
            m_upgradeCostText.text = string.Format("다배움");
        }
        else
        {
            m_upgradebutton.enabled = true;
            int cost = userAbility.m_upgradeCost[abilityData.m_level];
            m_upgradeCostText.text = string.Format("익히자\n{0:n0}", cost);
            m_upgradebutton.onClick.RemoveAllListeners();
            m_upgradebutton.onClick.AddListener(() =>
            {
                if (MUserManager.Instance.UseCash(cost) == true)
                {
                    abilityData.m_level += 1;
                    MUserManager.Instance.SetSkill(abilityData); SetAbility(abilityData);
                }
            });
        }
    }
}
