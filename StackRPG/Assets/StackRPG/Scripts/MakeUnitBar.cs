﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace stackRPG
{
    public class MakeUnitBar : MonoBehaviour
    {
        public MUser m_user;
        public Unit m_unit;

        public Image m_icon;
        public Text m_unitInfo;
        public Button m_upgradeButton;
        public Text m_upgradecostText;
        public Button m_makeButton;
        public Text m_makeCostText;

        public void SetUnit(Unit unit)
        {
            m_unit = unit;
            m_user = MGameManager.Instance.m_currentUser;
            UnitLevelTable unitLevelTable = m_user.GetUnitLevelTable(unit.m_id);
            int level = unitLevelTable.m_level;

            m_unitInfo.text = string.Format("Name : {0}[{1}] \nHP : {2}\nATK : {3}\nSPD : {4}", m_unit.m_name, level, m_unit.m_hp[level], m_unit.m_attackDamage[level], m_unit.m_moveSpeed);

            m_upgradeButton.gameObject.SetActive(unitLevelTable.m_isOpend);
            m_upgradecostText.text = string.Format("Upgrade\n$ {0}", m_unit.m_upgradeCost[level]);
            m_upgradeButton.onClick.RemoveAllListeners();
            m_upgradeButton.onClick.AddListener(() => { MGameManager.Instance.UpgradeUnit(m_user.m_id, m_unit.m_id); SetUnit(m_unit); });

            m_makeCostText.text = unitLevelTable.m_isOpend ? string.Format("Make\n$ {0}", m_unit.m_makePrice) :  string.Format("Open\n$ {0}", m_unit.m_openPrice);
            m_makeButton.onClick.RemoveAllListeners();
            m_makeButton.onClick.AddListener(() => 
            {
                if (unitLevelTable.m_isOpend) MGameManager.Instance.MakeUnit(m_user.m_id, m_unit.m_id);
                else { MGameManager.Instance.OpenUnit(m_user.m_id, m_unit.m_id); SetUnit(m_unit); }
            });
        }
    }
}
