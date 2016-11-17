using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using stackRPG;

public class UIUnitInfoTooltip : Singleton<UIUnitInfoTooltip>
{
    private MUnit m_munit;

    private Canvas myCanvas;
    private RectTransform rectTransform;
    private RectTransform m_iconRectTransform;
    private Vector3 m_left = new Vector3(-150, 0, 0);
    private Vector3 m_right = new Vector3(150, 0, 0);

    public CharAnimationControllerUI m_charAnimationControllerUI;
    public Image m_background;
    public Image m_icon;
    public Text m_info;

    void Awake()
    {
        myCanvas = FindObjectOfType<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        m_iconRectTransform = m_icon.GetComponent<RectTransform>();

        rectTransform.SetParent(myCanvas.transform);
        rectTransform.localScale = Vector3.one;
    }

    public static void Show(MUnit munit) { Instance._Show(munit); }
    public void _Show(MUnit munit)
    {
        m_munit = munit;
        Unit unit = munit.m_unit;
        int level = munit.m_level;

        m_background.color = munit.transform.FindChild("Sprite").FindChild("TeamCircle").GetComponent<SpriteRenderer>().color;

        m_charAnimationControllerUI.SetUnit(unit);
        m_charAnimationControllerUI.TargetImage.SetNativeSize();
        RectTransform rectTransform = m_charAnimationControllerUI.GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = new Vector3(rectTransform.sizeDelta.x * 0.5f + 5, 0, 0);
        m_charAnimationControllerUI.transform.localScale = new Vector3(2, 2, 1);

        m_info.text = string.Format("Name : {0}[{1}] \nHP : {2}\nATK : {3}\nSPD : {4}", unit.m_name, level, unit.m_hp[level], unit.m_attackDamage[level], unit.m_moveSpeed);
        UpdatePos();
        
        gameObject.SetActive(true);
    }
    public static void Hide() { Instance._Hide(); }
    public void _Hide()
    {
        m_munit = null;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0)) Hide();
        UpdatePos();
    }


    void UpdatePos()
    {
        if (m_munit == null) return;
        
        Vector3 position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_munit.transform.position);

        if (position.x < Screen.width * 0.5f) { position.x += rectTransform.sizeDelta.x * 0.25f; m_iconRectTransform.localPosition = m_right; m_info.alignment = TextAnchor.MiddleRight; }
        else if (position.x > Screen.width * 0.5f) { position.x -= rectTransform.sizeDelta.x * 0.25f; m_iconRectTransform.localPosition = m_left; m_info.alignment = TextAnchor.MiddleLeft; }
        
        if (position.y < Screen.height * 0.5f) { position.y += rectTransform.sizeDelta.y * 0.25f; }
        else if (position.y > Screen.height * 0.5f) { position.y -= rectTransform.sizeDelta.y * 0.25f; }

        rectTransform.position = position;
    }
}