using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class AnimationController : MonoBehaviour
{

    //! Editor를 위한 변수
    [HideInInspector]
    public string m_folderPath = "Assets/StackRPG/Gfx/Rpg Maker VX-Ace/";
    [HideInInspector]
    public string m_fileName;
    [HideInInspector]
    public int m_startIndex;
    [HideInInspector]
    public int m_endIndex;

    public bool m_IsReverse;
    public bool m_IsPingPong;


    public enum eAnimType
    {
        LOOP,
        ONESHOOT
    }

    public List<Sprite> SpriteFrames = new List<Sprite>();

    public eAnimType AnimType = eAnimType.LOOP;
    public bool IsAnimated = true;
    public float AnimSpeed = 0.2f;
    public bool IsDestroyedOnAnimEnd = false;
    public float CurrentFrame
    {
        get { return m_currAnimFrame; }
    }

    private float m_currAnimFrame = 0f;
    private float m_totalAnimFrame = 0f;


    public SpriteRenderer m_spriteRenderer;

    void Update()
    {
        if (IsAnimated)
        {
            m_currAnimFrame += AnimSpeed * Time.deltaTime;
            m_totalAnimFrame += AnimSpeed * Time.deltaTime;

            if (AnimType == eAnimType.LOOP)
            {
                if(m_IsPingPong == false)
                {
                    while (m_currAnimFrame >= SpriteFrames.Count)
                        m_currAnimFrame -= SpriteFrames.Count;
                }
                else
                {
                    float loopFrame = SpriteFrames.Count * 2 - 1;

                    while (m_totalAnimFrame >= loopFrame)
                        m_totalAnimFrame -= loopFrame;

                    if (m_totalAnimFrame < loopFrame * 0.5f)
                        m_currAnimFrame = m_totalAnimFrame;
                    else m_currAnimFrame = SpriteFrames.Count - 1 - (m_totalAnimFrame - loopFrame * 0.5f);
                }
            }
            else if (AnimType == eAnimType.ONESHOOT)
            {
                if (m_IsPingPong == false)
                {
                    if (m_currAnimFrame >= SpriteFrames.Count)
                    {
                        if (IsDestroyedOnAnimEnd)
                        {
                            Destroy(transform.gameObject);
                            return;
                        }
                        else
                        {
                            m_currAnimFrame = 0f;
                            IsAnimated = false;
                        }
                    }
                }
                else
                {
                    float loopFrame = SpriteFrames.Count * 2 - 1;

                    if (m_totalAnimFrame >= loopFrame)
                    {
                        if (IsDestroyedOnAnimEnd)
                        {
                            Destroy(transform.gameObject);
                            return;
                        }
                        else
                        {
                            m_currAnimFrame = 0f;
                            IsAnimated = false;
                        }
                    }
                    else
                    {
                        if (m_totalAnimFrame < loopFrame * 0.5f)
                            m_currAnimFrame = m_totalAnimFrame;
                        else m_currAnimFrame = SpriteFrames.Count - 1 - (m_totalAnimFrame - loopFrame * 0.5f);
                    }
                }
            }
        }
        else
        {
            m_currAnimFrame = 0.9f;
        }
        ShowSprite(m_IsReverse ? SpriteFrames.Count - 1 - (int)m_currAnimFrame : (int)m_currAnimFrame);
    }

    public void ShowSprite(int index)
    {
        m_spriteRenderer.sprite = SpriteFrames[index];
    }
}
