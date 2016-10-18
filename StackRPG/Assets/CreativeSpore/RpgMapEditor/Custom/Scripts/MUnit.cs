using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CreativeSpore;
using UnityEngine.Events;
using CreativeSpore.RpgMapEditor;

namespace stackRPG
{
    //! 성별 
    public enum Sex
    {
        Men,
        Women,
        middle,
    }

    //! 림월드에서 중요도높음
    //! 건강상태 (림월드에는 카테고리로써의 건강상태이고, 부위별로 상새 데이터가 있음 (오른발 : 물린흉터)
    public enum Health
    {
        Top,
        Middle,
        Bottom,
    }

    //! 유년기
    public enum Childhood
    {
        //! 앤지니어 따위?
        //! 알바 뭐해봤냐?
    }

    //! 성년기
    public enum Adulthood
    {
        //! 앤지니어 따위?
        //! 직업이 뭐였냐?
    }

    //! 중요도 높음
    //! 특성에따라 많은부분이 바뀐다.
    public enum Traits
    {
       NightOwl,
    }

    public enum State
    {
        Idle,
        Stun,        
        Dead,
    }
    public enum Action
    {   
        None,
        EquipWaepon,
        AttackEnemy,        
        FindEnemy,
        ChaseTarget,
        AttackHold,
        PathMove,
    }

    public enum Propensity
    {
        Normal,
        Chase,
        Avoid,
    }

    public enum Type
    {
        Unit,
        Weapon,
        Tile,
    }


    [Serializable]
    public struct Damage
    {
        public int m_power;
        public Vector2 m_force;

        public Damage(int power, Vector2 force)
        {
            m_power = power;
            m_force = force;
        }
    }

    [Serializable]
    public struct Weapon
    {
        public bool m_enable;
        public GameObject m_effectPrefab;
        public GameObject m_criticalEffectPrefab;
        public float m_range;
        //! 공격력
        public int m_power;
        //! 밀치는 능력
        public float m_force;
        //! 연사속도
        public float m_delay;
        //! 공격후 홀드 시간
        public float m_holdTime;


        public Weapon(bool enable, GameObject effectPrefab, GameObject criticalEffectPrefab, float range, int power, float force, float delay, float holdTime)
        {
            m_enable = enable;
            m_effectPrefab = effectPrefab;
            m_criticalEffectPrefab = criticalEffectPrefab;
            m_range = range;
            m_power = power;
            m_force = force;
            m_delay = delay;
            m_holdTime = holdTime;
        }
    }

    //! 몬스터가 나오고 싸우기도 해야한다.
    [RequireComponent(typeof(MovingBehaviour))]
    [RequireComponent(typeof(MapPathFindingBehaviour))]
    public class MUnit : MonoBehaviour
    {
        public float MinDistToReachTarget = 0.16f;

        public MovingBehaviour m_moving;
        public SpriteRenderer m_spriteRenderer;
        public MapPathFindingBehaviour m_pathFindingBehaviour;

        public Guid m_guid;
        void Awake()
        {
            m_moving = GetComponent<MovingBehaviour>();
            m_pathFindingBehaviour = GetComponent<MapPathFindingBehaviour>();
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_guid = MGameManager.Instance.AddUnit(this);
        }

        void Start()
        {
            //! 초기화
            m_pathFindingBehaviour.TargetPos = transform.position;
            m_rangeCenterPosition = transform.position;
            m_findRange = m_minFindRange;
        }
        
        public int m_teamId;
        public State m_state;
        public Action m_action;

        private string m_name;
        private string m_firstname;
        private long m_birthday;

        private Sex m_sex;
        private Traits m_traits;

        private Childhood m_childhood;
        private Adulthood m_adlthood;

        public Propensity m_propensity;
        public Weapon m_weapon;

        //! 능력치들 (장비장착에 따라 달라질수 있다)
        public float m_speed;
        public float m_hp;
        
        
        public float m_attackCoolTime;
        public float m_attackHoldTime;

        //! 타겟 적군
        public MUnit m_target_enemy;
        //! 타겟 아군
        public MUnit m_target_allies;


        public UnityAction m_changeActionDelegate;
        public UnityAction m_changeStateDelegate;

        void ChangeAction(Action action)
        {
            if (m_action == action) return;
            m_action = action;
            if (m_changeActionDelegate != null) m_changeActionDelegate();
        }

        void EquipWeapon(Weapon weapon)
        {
            m_weapon = weapon;
        }

        void ChangeState(State state)
        {
            if (m_state == state) return;
            m_state = state;
            if (m_changeStateDelegate != null) m_changeStateDelegate();
            
            //! 아이들 상태가아니면 패스무브를 할수없다.
            if (m_state != State.Idle) m_pathFindingBehaviour.enabled = false;
        }

        //! 주체, 데미지 타입, 데미지 크기 등이 있어야한다.
        public void OnDamage(Damage damage)
        {
            m_hp -= damage.m_power;
            if(m_hp <= 0) { ChangeState(State.Dead); }
            else
            {
                m_moving.ApplyForce(damage.m_force);

                StartCoroutine(DamageEffect(0.25f));
            }
        }

        IEnumerator DamageEffect(float time)
        {
            m_spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(time);
            m_spriteRenderer.color = Color.white;
        }


        
        public float m_findRange { get; private set; }
        public float m_minFindRange = 0.5f;
        public float m_maxFindRange = 2;
        public Vector3 m_rangeCenterPosition { get; private set; }
                
        void Update()
        {
            if (m_attackCoolTime > 0) m_attackCoolTime -= Time.deltaTime;
            
            //! 적을 찾는다.
            switch (m_state)
            {   
                case State.Dead:
                    {
                        MGameManager.Instance.RemoveUnit(m_guid);
                        Destroy(transform.gameObject);
                    }
                    break;
                case State.Stun:

                    break;                
                case State.Idle:
                    {
                        switch(m_action)
                        {
                            case Action.None:
                                {
                                    switch (m_propensity)
                                    {
                                        case Propensity.Chase:
                                            {
                                                //! 옆에오면 문다.
                                                if (m_target_enemy == null) { ChangeAction(Action.FindEnemy); return; }
                                                if (IsInAttackRange(m_target_enemy) == false) { ChangeAction(Action.ChaseTarget); return; }

                                                ChangeAction(Action.AttackEnemy);
                                            }
                                            break;
                                        //case Propensity.Normal:
                                        //    {
                                        //        //! 때리면 문다.
                                        //    }
                                        //    break;
                                        //case Propensity.Avoid:
                                        //    {
                                        //        //! 때리면 도망간다.
                                        //    }
                                        //    break;
                                        default:
                                            {
                                                //! 일단 움직이게되면, 서려고 애쓴다.
                                                //m_moving.ApplyForce((-m_moving.Veloc).normalized * m_moving.MaxForce * Time.deltaTime);
                                            }
                                            break;
                                    }
                                }
                                break;
                            case Action.PathMove:
                                {
                                    if (IsInMoveRange(this) == true) { m_pathFindingBehaviour.enabled = false; ChangeAction(Action.None);}
                                    else
                                    {
                                        Vector3 vTarget = m_pathFindingBehaviour.TargetPos;
                                        vTarget.z = transform.position.z;
                                        // stop when target position has been reached
                                        Vector3 vDist = (vTarget - transform.position);
                                        //Debug.DrawLine(vTarget, transform.position); //TODO: the target is the touch position, not the target tile center. Fix this to go to target position once in the target tile
                                        m_pathFindingBehaviour.enabled = vDist.magnitude > MinDistToReachTarget;
                                        if (!m_pathFindingBehaviour.enabled)
                                        {
                                            m_moving.Veloc = Vector3.zero;
                                            ChangeAction(Action.None);
                                        }
                                    }
                                    
                                }
                                break;
                            case Action.EquipWaepon:
                                {
                                    
                                }
                                break;
                            case Action.AttackHold:
                                {
                                    //! 일종의 스턴과 같은개념이다.
                                    //! 죽는것을 제외하고 아무것도 할수가없다
                                    if (m_attackHoldTime > 0) m_attackHoldTime -= Time.deltaTime;
                                    else { ChangeAction(Action.None); }
                                }
                                break;
                            case Action.FindEnemy:
                                {
                                    if(FindEnemy(ref m_target_enemy) == true) ChangeAction(Action.None);
                                    else m_moving.Arrive(m_rangeCenterPosition);
                                }   
                                break;
                            case Action.ChaseTarget:
                                {
                                    //! 타겟이 특정 지역에서 벗어나면 놓는다.
                                    if (IsInMoveRange(m_target_enemy) == false)
                                    {
                                        Debug.Log(m_target_enemy.name + " 이 범위를 벗어났습니다.");
                                        m_target_enemy = null;
                                    }

                                    if (m_target_enemy == null) { ChangeAction(Action.None); return; }

                                    //! 공격 사정거리 안에 들어오면
                                    if (IsInAttackRange(m_target_enemy) == true) { m_moving.Arrive(transform.position); ChangeAction(Action.None); return; }

                                    Vector3 vTarget = m_target_enemy.transform.position; vTarget.z = transform.position.z;

                                    //! 계속 거기로 가려한다.
                                    m_moving.Arrive(vTarget);
                                }
                                break;
                            case Action.AttackEnemy:
                                {
                                    if(IsCanAttackUnit(m_target_enemy) == false) { ChangeAction(Action.None); }

                                    if (m_attackCoolTime > 0) return;

                                    if (m_weapon.m_effectPrefab != null)
                                    {
                                        Instantiate(m_weapon.m_effectPrefab, m_target_enemy.transform.position, Quaternion.identity);

                                        //GameObject effect = Instantiate(m_weapon.m_effectPrefab, m_target_enemy.transform.position, Quaternion.identity) as GameObject;
                                        //AnimationController aniCon = effect.GetComponent<AnimationController>();
                                        //float effectTime = aniCon.SpriteFrames.Count / aniCon.AnimSpeed;
                                    }

                                    Damage damage = new Damage(m_weapon.m_power, (m_target_enemy.transform.position - transform.position).normalized * m_weapon.m_force);
                                    m_target_enemy.OnDamage(damage);
                                    m_attackCoolTime = m_weapon.m_delay;
                                    m_attackHoldTime = m_weapon.m_holdTime;
                                    ChangeAction(Action.AttackHold);

                                }
                                break;
                        }
                    }
                    break;
            }
        }

        public void SetMoveRange(Vector2 rangeCenterPosition, float range)
        {
            m_rangeCenterPosition = rangeCenterPosition;
            m_findRange = Mathf.Clamp(range, m_minFindRange, m_maxFindRange);
            m_pathFindingBehaviour.TargetPos = m_rangeCenterPosition;

            if (m_state == State.Idle){ChangeAction(Action.PathMove);}
        }

        private bool IsInMoveRange(MUnit unit)
        {
            if (unit == null) return false;
            if ((unit.transform.position - m_rangeCenterPosition).magnitude > m_findRange) return false;
            return true;
        }

        private bool IsInAttackRange(MUnit unit)
        {
            if (unit == null) return false;
            if((unit.transform.position - transform.position).magnitude > m_weapon.m_range) return false;
            return true;
        }

        private bool IsEnemy(MUnit unit)
        {
            if (unit.m_teamId == this.m_teamId) return false;
            return true;
        }

        private bool IsCanAttackUnit(MUnit unit)
        {
            if (unit == null) { Debug.Log("타겟이 없습니다."); return false; }
            if (IsInMoveRange(unit) == false) { Debug.Log(unit.name + " 이 인식 범위를 벗어났습니다."); return false; }
            if (IsInAttackRange(unit) == false) { Debug.Log(unit.name + " 이 공격 범위를 벗어났습니다."); return false; }
            if (unit.m_state == State.Dead) { Debug.Log(unit.name + " 이미 죽었습니다."); return false; }
            

            return true;
        }

        private bool FindEnemy(ref MUnit enemy)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(m_rangeCenterPosition, m_findRange, -1);
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Unit") == true)
                {
                    MUnit unit = col.GetComponent<MUnit>();
                    if (IsEnemy(unit) == false) continue;
                    if (IsInMoveRange(unit) == false) continue;
                    enemy = unit;
                    return true;
                }
            }
            return false;
        }
    }
}
