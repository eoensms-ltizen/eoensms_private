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

        [SerializeField]
        public List<MUnit> m_targets = new List<MUnit>();

        public Text m_text;
        private LinkedListNode<MUnit> m_unitLinkedListNode;
        private UnityAction m_changeActionDelegate = null;
        private UnityAction m_changeStateDelegate = null;

        private FollowObjectBehaviour m_followObjectBehaviour;


        public enum ClickAction
        {
            Select,
            Move,
            Attack,
            MapMove,
        }

        public ClickAction m_clickAction_left;
        public ClickAction m_clickAction_right;


        void Start()
        {
            m_text = GetComponent<Text>();

            m_followObjectBehaviour = Camera.main.GetComponent<FollowObjectBehaviour>();

            m_changeActionDelegate = () => 
            {
                //Debug.Log(m_target.m_action.ToString());
            };
            m_changeStateDelegate = () => 
            {
                //Debug.Log(m_target.m_state.ToString());
            };
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

        void UnSelect()
        {
            m_targets.Clear();
            m_clickAction_left = ClickAction.Select;
            m_clickAction_right = ClickAction.MapMove;
        }

        void Select(Collider2D[] cols)
        {
            m_targets.Clear();
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Unit") == true) m_targets.Add(col.GetComponent<MUnit>());
            }

            if (m_targets.Count == 0) m_clickAction_right = ClickAction.MapMove;
            else m_clickAction_right = ClickAction.Move;
        }

        void Update()
        {
            if (m_clickAction_left == ClickAction.Select)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    m_isTouching = true;
                    m_dragPoint_1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                
                if(m_isTouching == true)
                {
                    m_dragPoint_2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }

                if(Input.GetMouseButtonUp(0))
                {
                    m_isTouching = false;
                    Select(Physics2D.OverlapAreaAll(m_dragPoint_1, m_dragPoint_2, -1));
                }
            }

            if (m_clickAction_right == ClickAction.Move)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.SetAttackPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnSelect();
            }

            //TargetControll();
            //RangeSelect();
            //
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    SetTarget(null);
            //    m_targets.Clear();
            //    m_unitLinkedListNode = null;
            //}
            //
            //if (Input.GetKeyDown(KeyCode.Tab))
            //{
            //    if (m_unitLinkedListNode == null)
            //    {
            //        if (m_target == null) { m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.First; }
            //        else
            //        {
            //            m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.Find(m_target);
            //
            //            if (m_unitLinkedListNode.Next == null) { m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.First; }
            //            else { m_unitLinkedListNode = m_unitLinkedListNode.Next; }
            //        }
            //    }
            //    else
            //    {
            //        if (m_target != null && m_target != m_unitLinkedListNode.Value) { m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.Find(m_target); }
            //
            //        if (m_unitLinkedListNode.Next == null) { m_unitLinkedListNode = MGameManager.Instance.m_unitLinkedList.First; }
            //        else { m_unitLinkedListNode = m_unitLinkedListNode.Next; }
            //    }
            //
            //    SetTarget(m_unitLinkedListNode.Value);
            //}
            //
            //if(Input.GetMouseButtonDown(1) == true)
            //{
            //    Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //    Ray2D ray = new Ray2D(wp, Vector2.zero);
            //    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            //
            //    if (hit.collider == null) SetTarget(null);
            //
            //    if (hit.collider != null && hit.collider.CompareTag("Unit") == true)
            //    {
            //        SetTarget(hit.transform.GetComponent<MUnit>());
            //    }
            //}
            //
            //if (m_target == null) m_text.text = "Click! Mouse rightButton\n Press Tab!";
            //if (m_target != null)
            //{
            //    Vector3 pos = m_target.transform.position;//Camera.main.WorldToScreenPoint(headSquare.m_transform.position + m_nickNamePosition);
            //    pos.z = 0;
            //    m_text.transform.position = pos;
            //
            //    m_text.text = m_target.m_state.ToString() + "\n" + m_target.m_action.ToString();
            //}
        }

        public bool m_isTouching = false;
        float m_findRange;
        Vector3 m_rangeCenterPosition;

        Vector2 m_dragPoint_1;
        Vector2 m_dragPoint_2;


        float m_maxFindRange = 3;
        float m_minFindRange = 1;

        void RangeSelect()
        {
            if (m_target == null && m_targets.Count == 0)
            {
                //! 마우스 드레그 타게팅
                if (m_isTouching == false && Input.GetMouseButtonDown(0))
                {
                    m_targets.Clear();
                    m_isTouching = true;
                    m_findRange = m_minFindRange;
                }
                else if (m_isTouching == true && Input.GetMouseButtonUp(0))
                {
                    //! 터치를 종료하면, 찾는다
                    m_isTouching = false;

                    Collider2D[] cols = Physics2D.OverlapCircleAll(m_rangeCenterPosition, m_findRange, -1);

                    if(cols.Length == 1) SetTarget(cols[0].GetComponent<MUnit>());
                    else
                    {
                        foreach (Collider2D col in cols)
                        {
                            if (col.CompareTag("Unit") == true) m_targets.Add(col.GetComponent<MUnit>());
                        }
                    }
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
                    if (m_findRange > m_maxFindRange) m_findRange = m_maxFindRange;
                }
            }
        }

        void TargetControll()
        {
            if(m_targets.Count > 1)
            {
                //! 타게팅된 녀석을 컨트롤한다.
                if (m_isTouching == false && Input.GetMouseButtonDown(0))
                {
                    //! 터치시작시, 범위 초기화
                    m_isTouching = true;
                    m_findRange = 0;


                }
                else if (m_isTouching == true && Input.GetMouseButtonUp(0))
                {
                    //! 터치를 종료하면, 이동명령한다.
                    m_isTouching = false;
                    foreach(MUnit unit in m_targets)
                    {
                        unit.SetMoveRange(m_rangeCenterPosition, m_findRange);
                    }
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
                }
            }

            if (m_target != null)
            {
                //! 타게팅된 녀석을 컨트롤한다.
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
        }

        void OnDrawGizmos()
        {
            if(m_isTouching == true && m_clickAction_left == ClickAction.Select)
            {
                //Gizmos.DrawLine(m_dragPoint_1, m_dragPoint_2);

                Vector2 point3 = new Vector2(m_dragPoint_1.x, m_dragPoint_2.y);
                Vector2 point4 = new Vector2(m_dragPoint_2.x, m_dragPoint_1.y);

                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawLine(m_dragPoint_1, point3);
                Gizmos.DrawLine(m_dragPoint_1, point4);
                Gizmos.DrawLine(m_dragPoint_2, point3);
                Gizmos.DrawLine(m_dragPoint_2, point4);
            }

            if (m_targets.Count > 0)
            {
                for (int i = 0; i < m_targets.Count; ++i)
                {
                    if (m_targets[i] == null) { m_targets.RemoveAt(i); i--; }
                }

                foreach(MUnit unit in m_targets)
                {
                    Vector2 center = unit.transform.position;
                    float range = 0.2f;
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                    Gizmos.DrawSphere(center, range);
                }
            }
        }
    }

}
