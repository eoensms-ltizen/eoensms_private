using UnityEngine;
using System.Collections;

namespace CreativeSpore
{
    [RequireComponent(typeof(CharAnimationController))]
    [RequireComponent(typeof(PhysicCharBehaviour))]
    public class MCharBasicController : MonoBehaviour
    {
        public CharAnimationController AnimCtrl { get { return m_animCtrl; } }
        public PhysicCharBehaviour PhyCtrl { get { return m_phyChar; } }

        public bool IsVisible
        {
            get
            {
                return m_animCtrl.TargetSpriteRenderer.enabled;
            }

            set
            {
                SetVisible(value);
            }
        }

        protected CharAnimationController m_animCtrl;
        protected PhysicCharBehaviour m_phyChar;

        protected float m_timerBlockDir = 0f;

        protected virtual void Start()
        {
            m_animCtrl = GetComponent<CharAnimationController>();
            m_phyChar = GetComponent<PhysicCharBehaviour>();
        }

        private bool m_isMouseDown = false;
        public float m_sendsitivity = 3;
        private Vector2 m_range;
        protected virtual void Update()
        {
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

            UpdateMovement(m_range.x, m_range.y);
        }

        public void DoUpdateMovement(float fAxisX, float fAxisY)
        {
            UpdateMovement(fAxisX, fAxisY);
        }


        protected void UpdateMovement(float fAxisX, float fAxisY)
        {
            m_timerBlockDir -= Time.deltaTime;
            m_phyChar.Dir = new Vector3(fAxisX, fAxisY, 0);

            if (m_phyChar.IsMoving)
            {
                m_animCtrl.IsAnimated = true;

                if (m_timerBlockDir <= 0f)
                {
                    if (Mathf.Abs(fAxisX) > Mathf.Abs(fAxisY))
                    {
                        if (fAxisX > 0)
                            m_animCtrl.CurrentDir = CharAnimationController.eDir.RIGHT;
                        else if (fAxisX < 0)
                            m_animCtrl.CurrentDir = CharAnimationController.eDir.LEFT;
                    }
                    else
                    {
                        if (fAxisY > 0)
                            m_animCtrl.CurrentDir = CharAnimationController.eDir.UP;
                        else if (fAxisY < 0)
                            m_animCtrl.CurrentDir = CharAnimationController.eDir.DOWN;
                    }
                }
            }
            else
            {
                m_animCtrl.IsAnimated = false;
            }
        }

        public virtual void SetVisible(bool value)
        {
            m_animCtrl.TargetSpriteRenderer.enabled = value;
        }
    }
}