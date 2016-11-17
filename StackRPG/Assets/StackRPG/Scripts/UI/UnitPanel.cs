using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using stackRPG;
using CreativeSpore;


public enum UnitPanelState
{
    None,
    OwnerUnit,
    EnemyUnit,
}

public class UnitPanel : MonoBehaviour
{
    public UnitPanelState m_state { get; private set; }

    private MUser m_user;
    private List<Unit> m_units = new List<Unit>();
    private int m_currentIndex;
    public CharAnimationControllerUI m_charAnimationControllerUI;
    private UnitLevelTable m_unitLevelTable;


    public Image m_unitInfoBackground;
    public Text m_unitInfoText;

    public Button m_upgradeButton;
    public Text m_upgradecostText;

    public Button m_makeButton;
    public Text m_makeCostText;

    public Button m_up;
    public Button m_down;

    void Awake()
    {
        m_up.onClick.RemoveAllListeners();
        m_up.onClick.AddListener(Next);

        m_down.onClick.RemoveAllListeners();
        m_down.onClick.AddListener(Before);
    }

    public void Init(MUser user)
    {
        m_user = user;

        m_units.Clear();
        for (int i = 0;i< MUnitManager.Instance.m_unitDatas.Count;++i)
        {
            m_units.Add(MUnitManager.Instance.m_unitDatas[i].m_unitData);
        }

        m_currentIndex = 0;

        SetUnit(m_currentIndex);
    }

    void SetUnit(int index)
    {
        m_state = UnitPanelState.OwnerUnit;
        DrawImage(m_units[index].m_prefab.GetComponent<CharAnimationController>());
        DrawInfo(m_user.GetUnitLevelTable(m_units[index].m_id), m_user.m_startingPosition.m_color);
    }

    //! 게임상에서 클릭해서 드로잉할경우
    public void SetUnit(MUnit munit)
    {
        m_state = UnitPanelState.EnemyUnit;
        DrawImage(munit.GetComponent<CharAnimationController>());
        DrawState(munit.m_unit, munit.m_level,munit.m_teamColor);
    }

    public void UnSetUnit()
    {
        SetUnit(m_currentIndex);
    }

    void DrawInfo(UnitLevelTable unitLevelTable, Color color)
    {
        color.a = 0.3f;
        m_unitInfoBackground.color = color;

        Unit unit = MUnitManager.Instance.GetUnit(unitLevelTable.m_id);
        int level = unitLevelTable.m_level;

        if (level < unit.m_upgradeCost.Length - 1)
        {
            if (unitLevelTable.m_isOpend == true) m_unitInfoText.text = string.Format("Name : {0}[{1}] \nHP : {2} -> {3}\nATK : {4} -> {5}\nSPD : {6}", unit.m_name, level, unit.m_hp[level], unit.m_hp[level + 1], unit.m_attackDamage[level], unit.m_attackDamage[level + 1], unit.m_moveSpeed);
            else m_unitInfoText.text = string.Format("Name : {0}[{1}] \nHP : {2}\nATK : {3}\nSPD : {4}", unit.m_name, level, unit.m_hp[level], unit.m_attackDamage[level], unit.m_moveSpeed);

            m_upgradeButton.gameObject.SetActive(unitLevelTable.m_isOpend);
            m_upgradecostText.text = string.Format("Upgrade\n$ {0}", unit.m_upgradeCost[level]);
            m_upgradeButton.onClick.RemoveAllListeners();
            m_upgradeButton.onClick.AddListener(() => { MGameManager.Instance.UpgradeUnit(m_user.m_id, unit.m_id); SetUnit(m_currentIndex); });
        }
        else
        {
            m_unitInfoText.text = string.Format("Name : {0}[{1}] \nHP : {2}\nATK : {3}\nSPD : {4}", unit.m_name, level, unit.m_hp[level], unit.m_attackDamage[level], unit.m_moveSpeed);
            m_upgradeButton.gameObject.SetActive(false);
        }

        m_up.gameObject.SetActive(true);
        m_down.gameObject.SetActive(true);
        m_makeButton.gameObject.SetActive(true);
        m_makeCostText.text = unitLevelTable.m_isOpend ? string.Format("Make\n$ {0}", unit.m_makePrice) : string.Format("Open\n$ {0}", unit.m_openPrice);
        m_makeButton.onClick.RemoveAllListeners();
        m_makeButton.onClick.AddListener(() =>
        {
            if (unitLevelTable.m_isOpend) MGameManager.Instance.MakeUnit(m_user.m_id, unit.m_id, new Point2D(m_user.m_startingPosition.m_positions[m_user.m_makePointIndex]));
            else { MGameManager.Instance.OpenUnit(m_user.m_id, unit.m_id); SetUnit(m_currentIndex); }
        });
    }

    void DrawState(Unit unit, int level, Color color)
    {
        color.a = 0.3f;
        m_unitInfoBackground.color = color;
        m_unitInfoText.text = string.Format("Name : {0}[{1}] \nHP : {2}\nATK : {3}\nSPD : {4}", unit.m_name, level, unit.m_hp[level], unit.m_attackDamage[level], unit.m_moveSpeed);

        m_upgradeButton.gameObject.SetActive(false);
        m_makeButton.gameObject.SetActive(false);
        m_up.gameObject.SetActive(false);
        m_down.gameObject.SetActive(false);
    }

    void DrawImage(CharAnimationController charAnimationController)
    {
        m_charAnimationControllerUI.SpriteCharSet = charAnimationController.SpriteCharSet;
        m_charAnimationControllerUI.CharsetType = (CharAnimationControllerUI.eCharSetType)charAnimationController.CharsetType;
        m_charAnimationControllerUI.CreateSpriteFrames();
    }

    public void Next()
    {
        m_currentIndex++;
        if (m_currentIndex >= m_units.Count) m_currentIndex = 0;

        SetUnit(m_currentIndex);
    }

    public void Before()
    {
        m_currentIndex--;
        if (m_currentIndex < 0) m_currentIndex = m_units.Count - 1;

        SetUnit(m_currentIndex);
    }
}
