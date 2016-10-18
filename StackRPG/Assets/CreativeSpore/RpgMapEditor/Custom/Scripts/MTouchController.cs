using UnityEngine;
using System.Collections;
using CreativeSpore;

public class MTouchController : MonoBehaviour
{
    public Transform m_target;
    private MCharBasicController m_mcharBasicController;

    private bool m_isMouseDown = false;
    public float m_sendsitivity = 3;
    private Vector2 m_range;

    void Awake()
    {   
        m_mcharBasicController = m_target.GetComponent<MCharBasicController>();
        if(m_mcharBasicController == null)
        {
            CommonLog.Instance.ShowLog("MCharBasicController is null!");
        }
    }

    void Update()
    {
        if (m_mcharBasicController == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            m_isMouseDown = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_isMouseDown = false;
        }

        if (m_isMouseDown == true)
        {
            Vector2 range = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);

            if (range.x > 1) range.x = 1;
            else if (range.x < -1) range.x = -1;

            if (range.y > 1) range.y = 1;
            else if (range.y < -1) range.y = -1;

            m_range += range * m_sendsitivity * Time.deltaTime;

            if (m_range.x > 1) m_range.x = 1;
            else if (m_range.x < -1) m_range.x = -1;

            if (m_range.y > 1) m_range.y = 1;
            else if (m_range.y < -1) m_range.y = -1;
        }

        m_mcharBasicController.DoUpdateMovement(m_range.x, m_range.y);
    }
}