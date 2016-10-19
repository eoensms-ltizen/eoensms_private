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

        public bool m_isTouching = false;

        public Vector2 m_dragPoint_1;
        public Vector2 m_dragPoint_2;


        void Start()
        {
            m_text = GetComponent<Text>();

            m_followObjectBehaviour = Camera.main.GetComponent<FollowObjectBehaviour>();
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

        void SetTextPosition(Vector3 position)
        {   
            position.z = m_text.transform.position.z;
            m_text.transform.position = position;
        }

        void DrawInfo()
        {
            if (m_targets.Count == 0)
            {
                m_text.text = "Click! Mouse rightButton\n Press Tab!";
               
                SetTextPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            else if(m_targets.Count == 1)
            {
                MUnit target = m_targets[0];
                
                m_text.text = "State : " + target.m_state;
                m_text.text += "\nCommand : " + target.m_command;
                m_text.text += "\nHp : " + target.m_hp;
                m_text.text += "\nPower : " + target.m_weapon.m_power;
                m_text.text += "\nCoolTime : " + target.m_attackCoolTime + " / " + target.m_weapon.m_delay;
                
                SetTextPosition(target.transform.position);
            }
            else
            {
                m_text.text = "Ready to Command!!";

                SetTextPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }

        void Update()
        {
            DrawInfo();

            if (m_targets.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.H))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.CommandHold();
                    }
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.CommandStop();
                    }
                }

                if (Input.GetKeyDown(KeyCode.N))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.CommandNone();
                    }
                }
            }


            if (m_clickAction_left == ClickAction.Select)
            {
                if (m_targets.Count > 0)
                {
                    if (Input.GetKeyDown(KeyCode.A)) { m_clickAction_left = ClickAction.Attack; return; }
                }

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
            else if(m_clickAction_left == ClickAction.Attack)
            {
                if (m_targets.Count == 0) { m_clickAction_left = ClickAction.Select; return; }

                if (Input.GetMouseButtonDown(0))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.CommandAttackGround(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    }                    
                    m_clickAction_left = ClickAction.Select; return;
                }
            }

            if (m_clickAction_right == ClickAction.Move)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.CommandMoveGround(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnSelect();
            }
        }

        
        

        void OnDrawGizmos()
        {
            if(m_isTouching == true && m_clickAction_left == ClickAction.Select)
            {
                Vector2 point3 = new Vector2(m_dragPoint_1.x, m_dragPoint_2.y);
                Vector2 point4 = new Vector2(m_dragPoint_2.x, m_dragPoint_1.y);

                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawLine(m_dragPoint_1, point3);
                Gizmos.DrawLine(m_dragPoint_1, point4);
                Gizmos.DrawLine(m_dragPoint_2, point3);
                Gizmos.DrawLine(m_dragPoint_2, point4);
            }

            if(m_clickAction_left == ClickAction.Attack)
            {
                
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                point.z = 0;

                Gizmos.DrawSphere(point, 0.1f);
                Gizmos.DrawLine(point, point + Vector3.right * 0.2f);
                Gizmos.DrawLine(point, point + Vector3.left * 0.2f);
                Gizmos.DrawLine(point, point + Vector3.up * 0.2f);
                Gizmos.DrawLine(point, point + Vector3.down * 0.2f);
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
