using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainUnitBar : MonoBehaviour
{
    public Button m_background;
    public CharAnimationControllerUI m_charAnimationControllerUI;
    public Text m_unitInfo;
    public Button m_buyButton;
    public Text m_buyCostText;

    public Color m_haveColor = Color.white;
    public Color m_nonHaveColor = new Color(0, 0, 0, 0.8f);

    public void SetUnit(int unitId, bool isHave)
    {
        Unit unit = MUnitManager.Instance.GetUnit(unitId);

        m_charAnimationControllerUI.SetUnit(unit);
        m_charAnimationControllerUI.TargetImage.SetNativeSize();
        RectTransform rectTransform = m_charAnimationControllerUI.GetComponent<RectTransform>();
        //rectTransform.anchoredPosition3D = new Vector3(rectTransform.sizeDelta.x * 0.5f + 5, 0, 0);
        m_charAnimationControllerUI.transform.localScale = new Vector3(2, 2, 1);
        m_charAnimationControllerUI.IsAnimated = isHave;
        m_charAnimationControllerUI.TargetImage.color = isHave ? m_haveColor : m_nonHaveColor;

        m_unitInfo.text = string.Format("{0}", unit.m_name);
        //m_unitInfo.text = string.Format("Name : {0}[{1}] \nHP : {2}\nATK : {3}\nSPD : {4}", m_unit.m_name, level, m_unit.m_hp[level], m_unit.m_attackDamage[level], m_unit.m_moveSpeed);

        m_buyCostText.text = isHave ? string.Format("Sell\n$ {0}", unit.m_openPrice) : string.Format("Buy\n$ {0}", unit.m_openPrice);
        m_buyButton.onClick.RemoveAllListeners();
        m_buyButton.onClick.AddListener(() =>
        {
            if (isHave)
            {
                MUserManager.Instance.RemoveUnit(unitId);
                MUserManager.Instance.AddCash(unit.m_openPrice);
                SetUnit(unitId, !isHave);
            }
            else
            {
                if (MUserManager.Instance.UseCash(unit.m_openPrice) == true)
                {
                    MUserManager.Instance.AddUnit(unitId); SetUnit(unitId, !isHave);
                }
            }
        });

        m_background.onClick.RemoveAllListeners();
        m_background.onClick.AddListener(() => { UnitInfoWindow.Instance.SetUnit(unit); });
    }
}

