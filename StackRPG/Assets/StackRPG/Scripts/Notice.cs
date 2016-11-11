using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public enum NoticeEffect
{
    None,
    Typing,    
    Fade,
    Flicker,
}
public class Notice : Singleton<Notice> {

    public Text m_center;
    public Text m_bottom;
    private Coroutine m_centerCoroutine;
    private Coroutine m_bottomCoroutine;
    
    void Awake()
    {
        m_center.text = "";
        m_bottom.text = "";

        RectTransform rectTransform = GetComponent<RectTransform>();
        Canvas canvas = FindObjectOfType<Canvas>();
        if(canvas != null)
        {
            rectTransform.SetParent(FindObjectOfType<Canvas>().GetComponent<RectTransform>());
            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
        }
        else Debug.LogError("Not Found Canvas"); 

        

        
    }

    public void ClearCenter()
    {
        if (m_centerCoroutine != null) StopCoroutine(m_centerCoroutine);
        m_centerCoroutine = null;
        m_center.text = "";
        m_center.color = Color.white;
    }

    public void ClearBottom()
    {
        if (m_bottomCoroutine != null) StopCoroutine(m_bottomCoroutine);
        m_bottomCoroutine = null;
        m_bottom.text = "";
        m_bottom.color = Color.white;
    }

    public IEnumerator Center(string text, NoticeEffect effect, float speed, float duration)
    {
        ClearCenter();
        yield return m_centerCoroutine = StartCoroutine(Appear(m_center, text, effect, speed));
        yield return m_centerCoroutine = StartCoroutine(Showing(m_center, text, effect, speed, duration));
        yield return m_centerCoroutine = StartCoroutine(Disappear(m_center, text, effect, speed));
    }

    public void CenterAppear(string text, NoticeEffect effect, float speed = 1)
    {
        ClearCenter();
        m_centerCoroutine = StartCoroutine(Appear(m_center, text, effect, speed));
    }
    public void CenterDisappear(string text, NoticeEffect effect, float speed = 1)
    {
        ClearCenter();
        m_centerCoroutine = StartCoroutine(Disappear(m_center, text, effect, speed));
    }

    public IEnumerator Bottom(string text, NoticeEffect effect, float speed, float duration)
    {
        ClearBottom();
        yield return m_bottomCoroutine = StartCoroutine(Appear(m_bottom, text, effect, speed));
        yield return m_bottomCoroutine = StartCoroutine(Showing(m_bottom, text, effect ,speed, duration));
        yield return m_bottomCoroutine = StartCoroutine(Disappear(m_bottom, text, effect, speed));
    }

    public void BottomAppear(string text, NoticeEffect effect, float speed = 1)
    {
        ClearBottom();
        m_bottomCoroutine = StartCoroutine(Appear(m_bottom, text, effect, speed));
    }
    public void BottomDisappear(string text, NoticeEffect effect, float speed = 1)
    {
        ClearBottom();
        m_bottomCoroutine = StartCoroutine(Disappear(m_bottom, text, effect, speed));
    }

    IEnumerator Show(Text textObject, string text, NoticeEffect appear, float appearSpeed, NoticeEffect showing, float showingSpeed, float duration, NoticeEffect disAppear, float disappearSpeed)
    {
        yield return StartCoroutine(Appear(textObject, text, appear, appearSpeed));
        yield return StartCoroutine(Showing(textObject, text, showing, showingSpeed, duration));
        yield return StartCoroutine(Disappear(textObject, text, disAppear, disappearSpeed));
    }

    IEnumerator Fade(Text textObject, string text, bool isReverse, float speed)
    {   
        Color color = textObject.color;
        if(isReverse == true)
        {
            float alpha = 1;
            textObject.text = text;
            while (alpha > 0)
            {
                alpha -= speed * Time.deltaTime;
                if (alpha < 0) alpha = 0;
                color.a = alpha;
                textObject.color = color;
                yield return null;
            }
        }
        else
        {
            float alpha = 0;
            textObject.text = text;
            while (alpha < 1)
            {
                alpha += speed * Time.deltaTime;
                if (alpha > 1) alpha = 1;
                color.a = alpha;
                textObject.color = color;
                yield return null;
            }
        }
        
    }
    IEnumerator Typing(Text textObject, string text, bool isReverse, float speed)
    {
        WaitForSeconds tickTime = new WaitForSeconds(0.025f / speed);
        WaitForSeconds reverseTickTime = new WaitForSeconds(0.01f / speed);
        bool isDevide = false;
        if (isReverse == true)
        {
            if (isDevide == false)
            {
                //! 분해안함
                for (int i = text.Length - 1; i >= 0; --i)
                {
                    textObject.text = text.Remove(i);
                    
                    yield return tickTime;
                }
            }
            else
            {
                //! 분해함
                string temp = "";
                for (int i = text.Length - 1; i >= 0; --i)
                {
                    temp = text.Remove(i);

                    char c = text[i];
                    if (c >= 'ㄱ' && c <= 'ㅎ')
                    {
                        textObject.text = temp + c;
                        
                        yield return reverseTickTime;
                    }
                    //완성된 문자를 입력했을때 검색패턴 쓰기
                    else if (c >= '가')
                    {
                        //받침이 있는지 검사
                        int magic = ((c - '가') % 588);
                        HANGUL_INFO info = HangulJaso.DevideJaso(c);
                        //받침이 없을때.
                        if (magic == 0)
                        {
                            string subTemp = info.chars[0].ToString();
                            textObject.text = temp + subTemp;
                            
                            yield return reverseTickTime;
                        }

                        //받침이 있을때
                        else
                        {
                            string subTemp = HangulJaso.MergeJaso(info.chars[0].ToString(), info.chars[1].ToString(), "").ToString();
                            textObject.text = temp + subTemp;
                            
                            yield return reverseTickTime;
                            subTemp = info.chars[0].ToString();
                            textObject.text = temp + subTemp;
                            
                            yield return reverseTickTime;
                        }
                    }
                    //영어를 입력했을때
                    else if (c >= 'A' && c <= 'z')
                    {

                    }
                    //숫자를 입력했을때.
                    else if (c >= '0' && c <= '9')
                    {

                    }
                    else
                    {

                    }

                    textObject.text = temp;
                    
                    yield return reverseTickTime;
                }            
            }
        }
        else
        {
            textObject.text = "";
            string log = "";
            for (int i = 0; i < text.Length; ++i)
            {
                char c = text[i];
                if (c >= 'ㄱ' && c <= 'ㅎ')
                {
                    textObject.text = log + c;
                    yield return tickTime;
                }
                //완성된 문자를 입력했을때 검색패턴 쓰기
                else if (c >= '가')
                {
                    //받침이 있는지 검사
                    int magic = ((c - '가') % 588);
                    HANGUL_INFO info = HangulJaso.DevideJaso(c);
                    //받침이 없을때.
                    if (magic == 0)
                    {
                        string temp = info.chars[0].ToString();
                        textObject.text = log + temp;
                        yield return tickTime;
                        temp = HangulJaso.MergeJaso(info.chars[0].ToString(), info.chars[1].ToString(), "").ToString();
                        textObject.text = log + temp;
                        yield return tickTime;
                    }

                    //받침이 있을때
                    else
                    {
                        string temp = info.chars[0].ToString();
                        textObject.text = log + temp;
                        yield return tickTime;
                        temp = HangulJaso.MergeJaso(info.chars[0].ToString(), info.chars[1].ToString(), "").ToString();
                        textObject.text = log + temp;
                        yield return tickTime;
                        temp = HangulJaso.MergeJaso(info.chars[0].ToString(), info.chars[1].ToString(), info.chars[2].ToString()).ToString();
                        textObject.text = log + temp;
                        yield return tickTime;
                    }
                }
                //영어를 입력했을때
                else if (c >= 'A' && c <= 'z')
                {
                    textObject.text = log + c;
                    yield return tickTime;
                }
                //숫자를 입력했을때.
                else if (c >= '0' && c <= '9')
                {
                    textObject.text = log + c;
                    yield return tickTime;
                }
                else
                {
                    textObject.text = log + c;
                    yield return tickTime;
                }
                log += c;
            }
        }
    }

    public IEnumerator Appear(Text textObject, string text, NoticeEffect effect,float speed)
    {   
        switch(effect)
        {   
            case NoticeEffect.Typing:
                yield return StartCoroutine(Typing(textObject, text, false, speed));
                break;
            case NoticeEffect.Fade:
                yield return StartCoroutine(Fade(textObject, text, false, speed));
                break;
            default:
                textObject.text = text;
                yield return null;
                break;

        }
    }

    public IEnumerator Disappear(Text textObject, string text, NoticeEffect effect,float speed)
    {
        switch (effect)
        {   
            case NoticeEffect.Typing:
                yield return StartCoroutine(Typing(textObject, text, true, speed));
                break;
            case NoticeEffect.Fade:
                yield return StartCoroutine(Fade(textObject, text, true, speed));
                break;
            default:
                textObject.text = text;
                yield return null;
                break;
        }
    }

    IEnumerator Showing(Text textObject, string text, NoticeEffect effect, float speed, float duration)
    {
        switch (effect)
        {   
            default:
                textObject.text = text;
                yield return new WaitForSeconds(duration);
                break;
        }
    }

    public void Log(string text)
    {
        m_bottomCoroutine = StartCoroutine(Bottom(text, NoticeEffect.None, 1, 2));
    }
}

