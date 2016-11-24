using UnityEngine;
using UnityEngine.UI;

public class UnitInfoWindow : Singleton<UnitInfoWindow>
{
    public float m_imageSize = 2.0f;

    public CharAnimationControllerUI m_charAnimationControllerUI;
    public Text m_name;
    public Text m_hisStory;
    public Text m_stat_name;
    public Text m_stat_value;

    public Text m_openPrice_name;
    public Text m_openPrice_value;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        RectTransform rectTransform = GetComponent<RectTransform>();
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            rectTransform.SetParent(FindObjectOfType<Canvas>().GetComponent<RectTransform>());
            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
        }
        else Debug.LogError("Not Found Canvas");
    }

    public void SetUnit(Unit unit)
    {
        m_charAnimationControllerUI.SetUnit(unit);
        m_charAnimationControllerUI.TargetImage.SetNativeSize();
        m_charAnimationControllerUI.TargetImage.rectTransform.sizeDelta *= m_imageSize;

        m_name.text = unit.m_name;
        m_hisStory.text = unit.m_history;

        //m_stat_name.text = "";
        int level = 0;
        m_stat_value.text = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}", unit.m_hp[level], unit.m_moveSpeed, unit.m_attackDamage[level], unit.m_attackCoolTime, unit.m_attackRange, unit.m_attackType, unit.m_armorType);

        //m_openPrice_name.text = "";
        m_openPrice_value.text = string.Format("{0:n0}",unit.m_openPrice);

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
