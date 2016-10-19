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
    public enum UnitAction
    {   
        None,
        EquipWaepon,
        AttackEnemy,        
        FindEnemy,
        ChaseTarget,
        AvoidTarget,
        AttackHold,
        PathMove,
    }

    public enum Result
    {
        Play,
        Pause,
        Success,
        Fail,
        Cancel,
    }

    public enum Command
    {
        None, //! 케릭터 특성에 따라 왠만하면 (Attack_Ground)
        Attack_Target,
        Attack_Ground,
        Move_Target,
        Move_Ground,
        Hold,
        Stop,
    }



    public enum Propensity
    {
        Avoid = 0,
        Normal = 1,
        Chase = 2,
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

    public enum WeaponType
    {
        Sword,
        Gun,
        Magic,
    }

    [Serializable]
    public struct Weapon
    {
        public bool m_enable;
        public WeaponType m_weaponType;
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


        public Weapon(bool enable, WeaponType weaponType, GameObject effectPrefab, GameObject criticalEffectPrefab, float range, int power, float force, float delay, float holdTime)
        {
            m_enable = enable;
            m_weaponType = weaponType;
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
        public CharAnimationController m_animCtrl;
        public SpriteRenderer m_spriteRenderer;
        public MapPathFindingBehaviour m_pathFindingBehaviour;

        public Guid m_guid;
        void Awake()
        {
            m_animCtrl = GetComponent<CharAnimationController>();
            m_moving = GetComponent<MovingBehaviour>();
            m_pathFindingBehaviour = GetComponent<MapPathFindingBehaviour>();
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_guid = MGameManager.Instance.AddUnit(this);
        }

        void OnEnable()
        {
            m_pathFindingBehaviour.TargetPos = transform.position;
            CommandNone();
        }

        void Start()
        {
            
        }
        
        public int m_teamId;
        public State m_state;
        public Command m_command;

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

        //! 날때린 녀석        
        public Dictionary<Guid, MUnit> m_damage_enemys = new Dictionary<Guid, MUnit>();
        
        public UnityAction m_changeStateDelegate;


        void EquipWeapon(Weapon weapon)
        {
            m_weapon = weapon;
        }

        void ChangeState(State state)
        {
            if (m_state == state) return;
            m_state = state;
            if (m_changeStateDelegate != null) m_changeStateDelegate();
        }

        public void Dead()
        {
            ChangeState(State.Dead);

            MGameManager.Instance.RemoveUnit(m_guid);
            Destroy(transform.gameObject);
        }
        //! 주체, 데미지 타입, 데미지 크기 등이 있어야한다.
        public void Damage(MUnit enemy, Damage damage)
        {
            //! 때린놈을 기억한다.
            AddAttackOnMeEnemy(enemy);

            m_hp -= damage.m_power;
            if(m_hp <= 0) { Dead(); }
            else
            {
                m_moving.ApplyForce(damage.m_force);
                m_animCtrl.StartCoroutine(DamageEffect(0.25f));
            }
        }

        IEnumerator DamageEffect(float time)
        {
            m_spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(time);
            m_spriteRenderer.color = Color.white;
        }
        
        
                
        void Update()
        {
            if (m_attackCoolTime > 0) m_attackCoolTime -= Time.deltaTime;
            if (m_attackHoldTime > 0) m_attackHoldTime -= Time.deltaTime;

            List<Guid> removeList = new List<Guid>();
            foreach(KeyValuePair<Guid,MUnit> value in m_damage_enemys)
            {
                if (value.Value == null) removeList.Add(value.Key);
            }
            foreach(Guid guid in removeList)
            {
                m_damage_enemys.Remove(guid);
            }
        }

        void UpdateAnimDir()
        {
            float absVx = Mathf.Abs(m_moving.Veloc.x);
            float absVy = Mathf.Abs(m_moving.Veloc.y);
            m_animCtrl.IsAnimated = true;
            if (absVx > absVy)
            {
                if (m_moving.Veloc.x > 0)
                    m_animCtrl.CurrentDir = CharAnimationController.eDir.RIGHT;
                else if (m_moving.Veloc.x < 0)
                    m_animCtrl.CurrentDir = CharAnimationController.eDir.LEFT;
            }
            else if (absVy > 0f)
            {
                if (m_moving.Veloc.y > 0)
                    m_animCtrl.CurrentDir = CharAnimationController.eDir.UP;
                else if (m_moving.Veloc.y < 0)
                    m_animCtrl.CurrentDir = CharAnimationController.eDir.DOWN;
            }
            else
            {
                m_animCtrl.IsAnimated = false;
            }
        }


        public void AddAttackOnMeEnemy(MUnit unit)
        {
            if (unit == null) return;
            if (m_damage_enemys.ContainsKey(unit.m_guid) == true) return;

            m_damage_enemys.Add(unit.m_guid, unit);
        }

        public bool IsAttackOnMeEnemy(MUnit unit)
        {
            if (unit == null) return false;
            return m_damage_enemys.ContainsKey(unit.m_guid);
        }

        public void RemoveAttackOnMeEnemy(MUnit unit)
        {
            if (unit == null) return;
            if (m_damage_enemys.ContainsKey(unit.m_guid) == false) return;

            m_damage_enemys.Remove(unit.m_guid);
        }

        public bool GetNearestAttackOnMeEnemy(out MUnit unit)
        {   
            float sqrMagnitude = 0;
            unit = null;            
            foreach (KeyValuePair<Guid,MUnit> value in m_damage_enemys)
            {
                if (value.Value == null) continue;

                if (unit == null || sqrMagnitude > (value.Value.transform.position - transform.position).sqrMagnitude)
                {
                    unit = value.Value;
                    sqrMagnitude = (value.Value.transform.position - transform.position).sqrMagnitude;
                }
            }
            return unit != null ? true : false;
        }

        //! 쫒아가라
        IEnumerator ChaseTarget(MUnit unit, Action end)
        {
            while (IsCanMove() == true && IsCanTargetingUnit(unit) == true && IsArrived(unit.transform.position, m_weapon.m_range) == false)
            {
                PathMoveUpdate(unit.transform.position);
                yield return null;
            }
            PathMoveStop();
            if (end != null) end();
        }
        //! 이동해라
        IEnumerator PathMove(Vector3 point, Action end)
        {
            while (IsArrived(point, MinDistToReachTarget) == false)
            {
                PathMoveUpdate(point);
                yield return null;
            }
            PathMoveStop();
            if (end != null) end();
        }
        
        private bool IsArrived(Vector3 target, float minDistToReachTarget)
        {
            return (target - transform.position).magnitude <= minDistToReachTarget;
        }
        
        private void PathMoveUpdate(Vector3 point)
        {
            point.z = transform.position.z;
            m_pathFindingBehaviour.TargetPos = point;
            m_pathFindingBehaviour.enabled = IsCanMove();

            UpdateAnimDir();
        }

        private void PathMoveStop()
        {
            m_pathFindingBehaviour.enabled = false;
            m_moving.Acc = Vector3.zero;
            m_moving.Veloc = Vector3.zero;

            UpdateAnimDir();
        }


        IEnumerator FindEnemy(float range, int layerMask, Action<MUnit> end)
        {
            MUnit unit = null;
            while(FindEnemy(ref unit, transform.position, range, layerMask) == false)
            {
                yield return null;
            }
            if (end != null) end(unit);
        }


        bool AttackTarget(MUnit unit)
        {
            if (IsCanAttackUnit(unit) == false) return false;

            if (m_weapon.m_effectPrefab != null) Instantiate(m_weapon.m_effectPrefab, unit.transform.position, Quaternion.identity);

            Damage damage = new Damage(m_weapon.m_power, (unit.transform.position - transform.position).normalized * m_weapon.m_force);
            unit.Damage(this, damage);
            m_attackCoolTime = m_weapon.m_delay;
            m_attackHoldTime = m_weapon.m_holdTime;

            return true;
        }
        

        IEnumerator AttackTarget(MUnit unit, Action end)
        {
            while (IsCanTargetingUnit(unit) == true)
            {
                yield return StartCoroutine(ChaseTarget(unit, null));
                AttackTarget(unit);
            }
            if (end != null) end();
        }

        IEnumerator FightingBack()
        {
            MUnit unit;
            while(GetNearestAttackOnMeEnemy(out unit) == false)
            {
                yield return null;   
            }
            CommandAttackTarget(unit);
        }

        IEnumerator Stop(float time, Action end)
        {
            yield return time;
            if (end != null) end();
        }

        public void SetCommand(Command command)
        {
            StopAllCoroutines();

            //! 이런게 더러운데..쩝;
            PathMoveStop();
            Debug.Log("SetCommand : " + m_command + "-> " + command);
            m_command = command;
        }

        public void CommandNone()
        {
            //! 지속상태 선공격, 추격한다.
            SetCommand(Command.None);

            StartCoroutine(FightingBack());
            StartCoroutine(FindEnemy(m_weapon.m_range, -1, (unit) => { StartCoroutine(AttackTarget(unit, CommandHold)); }));
        }
        public void CommandHold()
        {
            //! 지속상태 선공격, 추격하지 않는다.
            SetCommand(Command.Hold);

            //! 적을찾고, 찾으면 쏘고, 벗어나면, 다시 커멘드 홀드호출
            StartCoroutine(FindEnemy(m_weapon.m_range, -1, (unit) => { StartCoroutine(AttackTarget(unit, CommandHold)); }));
        }

        public void CommandStop()
        {
            SetCommand(Command.Stop);

            StartCoroutine(Stop(0.5f, CommandNone));
        }

        public void CommandMoveGround(Vector2 point)
        {
            //! 반격하지않고. 강제 이동만한다. 이동 이후 None으로 간다.
            SetCommand(Command.Move_Ground);

            StartCoroutine(PathMove(point, ()=> { SetCommand(Command.None); }));
        }

        public void CommandAttackGround(Vector2 point)
        {
            //! 목표지역으로 이동한다. 이벤트(적발견, 공격당함) 발생시 AttackTarget으로 변경된다.
            SetCommand(Command.Attack_Ground);

            StartCoroutine(PathMove(point, CommandNone));
            StartCoroutine(FindEnemy(m_weapon.m_range, -1, CommandAttackTarget));
        }
        public void CommandAttackTarget(MUnit unit)
        {
            SetCommand(Command.Attack_Target);

            StartCoroutine(AttackTarget(unit, () => { SetCommand(Command.None); }));
        }


        private bool IsCanMove()
        {
            if (m_state != State.Idle) return false;
            if (m_command == Command.Hold || m_command == Command.Stop) return false;
            if (m_attackHoldTime > 0) return false;
            return true;
        }

        private bool IsEnemy(MUnit unit)
        {
            if (unit.m_teamId == this.m_teamId) return false;
            return true;
        }

        private bool IsInAttackRange(MUnit unit)
        {
            if (unit == null) return false;
            if((unit.transform.position - transform.position).magnitude > m_weapon.m_range) return false;
            return true;
        }

        private bool IsCanTargetingUnit(MUnit unit)
        {
            if (unit == null) { return false; }
            if (unit.m_state == State.Dead) { return false; }
            return true;
        }


        private bool IsCanAttackUnit(MUnit unit)
        {
            if (IsCanTargetingUnit(unit) == false) { return false; }
            if (IsInAttackRange(unit) == false) { return false; }
            if (m_attackCoolTime > 0) return false;

            return true;
        }

        private bool FindEnemy(ref MUnit enemy, Vector3 center, float range, int layerMask)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(center, range, layerMask);
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Unit") == true)
                {
                    MUnit unit = col.GetComponent<MUnit>();
                    if (IsCanTargetingUnit(unit) == false) continue;
                    if (IsEnemy(unit) == false) continue;                    
                    enemy = unit;
                    return true;
                }
            }
            return false;
        }
    }
}
