using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace stackRPG
{
    public class MakeUnitBar : MonoBehaviour
    {
        public Image m_icon;
        public Button m_makeButton;
        public Text m_costText;
        public Text m_unitInfo;

        public Unit m_unit;

        public void SetUnit(Unit unit)
        {
            m_unit = unit;

            m_costText.text = string.Format("$ {0}", m_unit.m_price);
            m_unitInfo.text = string.Format("Name : {0}[{1}] \nHP : {2}\nATK : {3}\nSPD : {4}", m_unit.m_name, m_unit.m_level, m_unit.m_level, m_unit.m_level, m_unit.m_level);

            m_makeButton.onClick.RemoveAllListeners();
            m_makeButton.onClick.AddListener(() => { MGameManager.Instance.BuyCharcter(m_unit); });
        }
    }
}

