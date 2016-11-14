using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NicknamePanel : Singleton<Notice>
{   
    private string m_nickname;
    public Text m_text;
    public Button m_button;
    private TouchScreenKeyboard m_keyboard;

    public InputField m_inputField;

    void Awake()
    {
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

    void Start()
    {
        SetNickname(MSettings.GetNickname());

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

        m_text.gameObject.SetActive(true);
        m_inputField.gameObject.SetActive(false);

        m_button.onClick.AddListener(() =>
        {   
            m_keyboard = TouchScreenKeyboard.Open(m_nickname, TouchScreenKeyboardType.Default);
            StartCoroutine(CheckInputDone());
        });

#else
        m_button.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(m_inputField.gameObject, null);
        });

        m_text.gameObject.SetActive(false);
        m_inputField.gameObject.SetActive(true);
        m_inputField.onEndEdit.AddListener((text) => { SetNickname(text); });
#endif
    }

    void SetNickname(string nickname)
    {   
        m_nickname = nickname.Trim();
        if (m_nickname.Length == 0) m_nickname = MSettings.GetRandomNickname();

        MSettings.SetNickname(m_nickname);

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
        m_text.text = m_nickname;
#else
        m_inputField.text = m_nickname;
#endif
    }

    IEnumerator CheckInputDone()
    {
        while (m_keyboard != null && !m_keyboard.done && m_keyboard.active)
        {
            yield return null;
        }

        SetNickname(m_keyboard.text);
    }
}
