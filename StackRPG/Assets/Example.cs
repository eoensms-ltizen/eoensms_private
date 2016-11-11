using UnityEngine;
using System.Collections;

public class Example : MonoBehaviour
{
    public Transform sunrise;
    public Transform sunset;
    public float journeyTime = 1.0F;
    private float startTime;
    public float centerScale = 1;

    public bool m_isReverse;
    public bool m_isLerp;
    void Start()
    {
        startTime = Time.time;
    }
    void Update()
    {
        

        if (m_isReverse == false)
        {
            Vector3 center = (sunrise.position + sunset.position) * 0.5F;
            center -= new Vector3(0, centerScale, 0);
            Vector3 riseRelCenter = sunrise.position - center;
            Vector3 setRelCenter = sunset.position - center;
            float fracComplete = (Time.time - startTime) / journeyTime;

            transform.position = m_isLerp ? Vector3.Lerp(riseRelCenter, setRelCenter, fracComplete) : Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
            transform.position += center;
            if(fracComplete >= 1)
            {
                m_isReverse = true;
                startTime += journeyTime * fracComplete;
            }
        }
        else
        {
            Vector3 center = (sunrise.position + sunset.position) * 0.5F;
            center -= new Vector3(0, -centerScale, 0);
            Vector3 riseRelCenter = sunrise.position - center;
            Vector3 setRelCenter = sunset.position - center;
            float fracComplete = (Time.time - startTime) / journeyTime;
            transform.position = m_isLerp ? Vector3.Lerp(setRelCenter, riseRelCenter, fracComplete) : Vector3.Slerp(setRelCenter, riseRelCenter, fracComplete);
            transform.position += center;
            if (fracComplete >= 1)
            {
                m_isReverse = false;
                startTime += journeyTime * fracComplete;
            }
        }
        
    }
}