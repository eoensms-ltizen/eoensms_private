using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using CreativeSpore;

namespace stackRPG
{
    [RequireComponent(typeof(Text))]
    public class PlayerInfo : MonoBehaviour
    {
        public MUnit m_target;
        public Text m_text;
        private LinkedListNode<MUnit> m_unitLinkedListNode;
        private UnityAction m_changeActionDelegate = null;
        private UnityAction m_changeStateDelegate = null;

        private FollowObjectBehaviour m_followObjectBehaviour;



        void Start()
        {
            m_text = GetComponent<Text>();

            m_followObjectBehaviour = Camera.main.GetComponent<FollowObjectBehaviour>();

            m_changeActionDelegate = () => { Debug.Log(m_target.m_action.ToString()); };
            m_changeStateDelegate = () => { Debug.Log(m_target.m_state.ToString()); };
        }

        

        void SetTarget(MUnit unit)
        {
            //! 이벤트 해지
            if (m_target != null)
            {
                m_target.m_changeActionDelegate -= m_changeActionDelegate;
                m_target.m_changeStateDelegate -= m_changeStateDelegate;
            }

            m_target = unit;

            if (m_followObjectBehaviour != null)
            {
                if (m_target != null) m_followObjectBehaviour.SetTarget(m_target.transform);
                else m_followObjectBehaviour.SetTarget(null);
            }

            //! 이벤트 등록
            if (m_target != null)
            {
                m_target.m_changeActionDelegate += m_changeActionDelegate;
                m_target.m_changeStateDelegate += m_changeStateDelegate;
            }
        }

        void Update()
        {
            TargetControll();


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetTarget(null);
                m_unitLinkedListNode = null;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (m_unitLinkedListNode == null)
                {
                    if (m_target == null) { m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.First; }
                    else
                    {
                        m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.Find(m_target);

                        if (m_unitLinkedListNode.Next == null) { m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.First; }
                        else { m_unitLinkedListNode = m_unitLinkedListNode.Next; }
                    }
                }
                else
                {
                    if (m_target != null && m_target != m_unitLinkedListNode.Value) { m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.Find(m_target); }

                    if (m_unitLinkedListNode.Next == null) { m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.First; }
                    else { m_unitLinkedListNode = m_unitLinkedListNode.Next; }
                }

                SetTarget(m_unitLinkedListNode.Value);
            }

            if(Input.GetMouseButtonDown(1) == true)
            {
                Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Ray2D ray = new Ray2D(wp, Vector2.zero);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider == null) SetTarget(null);

                if (hit.collider != null && hit.collider.CompareTag("Unit") == true)
                {
                    SetTarget(hit.transform.GetComponent<MUnit>());
                }
            }

            if (m_target == null) m_text.text = "Click! Mouse rightButton\n Press Tab!";
            if (m_target != null)
            {
                Vector3 pos = m_target.transform.position;//Camera.main.WorldToScreenPoint(headSquare.m_transform.position + m_nickNamePosition);
                pos.z = 0;
                m_text.transform.position = pos;

                m_text.text = m_target.m_state.ToString() + "\n" + m_target.m_action.ToString();
            }
        }

        bool m_isTouching = false;
        float m_findRange;
        Vector3 m_rangeCenterPosition;
        void TargetControll()
        {
            if (m_target == null) return;

            if (m_isTouching == false && Input.GetMouseButtonDown(0))
            {
                //! 터치시작시, 범위 초기화
                m_isTouching = true;
                m_findRange = m_target.m_minFindRange;


            }
            else if (m_isTouching == true && Input.GetMouseButtonUp(0))
            {
                //! 터치를 종료하면, 이동명령한다.
                m_isTouching = false;
                m_target.SetMoveRange(m_rangeCenterPosition, m_findRange);
            }

            if (m_isTouching == true)
            {
                //! 터치중 범위를 늘린다.

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane hPlane = new Plane(Vector3.forward, Vector3.zero);
                float distance = 0;
                if (hPlane.Raycast(ray, out distance))
                {
                    m_rangeCenterPosition = ray.GetPoint(distance);
                }
                m_findRange += Time.deltaTime;
                if (m_findRange > m_target.m_maxFindRange) m_findRange = m_target.m_maxFindRange;
            }
        }

        void OnDrawGizmos()
        {
            if (m_target == null) return;
            Vector2 center = m_isTouching ? m_rangeCenterPosition : m_target.m_rangeCenterPosition;
            float range = m_isTouching ? m_findRange : m_target.m_findRange;
            Gizmos.color = new Color(1, 1, 1, 0.5f);
            Gizmos.DrawSphere(center, range);
        }
    }

}
