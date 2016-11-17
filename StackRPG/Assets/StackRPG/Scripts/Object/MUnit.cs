using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CreativeSpore;
using UnityEngine.Events;
using CreativeSpore.RpgMapEditor;
using DG.Tweening;

namespace stackRPG
{
    public enum State
    {
        Idle,
        Stun,
        Dead,
    }

    public enum Command
    {
        None, 
        Attack_Target,
        Attack_Ground,
        Move_Target,
        Move_Ground,
        Hold,
        Stop,
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

    //! 몬스터가 나오고 싸우기도 해야한다.
    [RequireComponent(typeof(MovingBehaviour))]
    [RequireComponent(typeof(MapPathFindingBehaviour))]
    
    public class MUnit : MonoBehaviour
    {
        public Guid m_guid;

        public SpriteRenderer m_spriteRenderer;
        public MovingBehaviour m_moving;
        public CharAnimationController m_animCtrl;
        public MapPathFindingBehaviour m_pathFindingBehaviour;
        public Rigidbody2D m_rigidbody2d;

        public int m_level;
        public Unit m_unit { get; private set; }

        public int m_teamId;
        public Color m_teamColor;        

        public State m_state;
        public Command m_command;

        void Awake()
        {
            m_animCtrl = GetComponent<CharAnimationController>();
            m_moving = GetComponent<MovingBehaviour>();
            m_pathFindingBehaviour = GetComponent<MapPathFindingBehaviour>();            
            m_rigidbody2d = GetComponent<Rigidbody2D>();
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_guid = Guid.NewGuid();
        }
        
        public void Init(Unit unit)
        {
            //! 초기화
            m_unit = unit;

            m_pathFindingBehaviour.TargetPos = transform.position;
            CommandNone();

            m_hp = m_base_hp;
            m_attackDamage = m_base_attackDamage;
            m_attackCoolTime = m_base_attackCoolTime;
            m_attackHoldTime = m_base_attackHoldTime;
            m_attackRange = m_base_attackRange;
            m_moveSpeed = m_base_moveSpeed;
            m_minDisToReachTarg = m_base_minDisToReachTarg;
        }
        //! 초기 능력치

        public int m_id { get { return m_unit.m_id; } }
        public float m_base_hp { get { return m_unit.m_hp[m_level]; } }
        public int m_base_attackDamage { get { return m_unit.m_attackDamage[m_level]; } }
        public float m_base_attackCoolTime { get { return m_unit.m_attackCoolTime; } }
        public float m_base_attackHoldTime { get { return m_unit.m_attackHoldTime; } }
        public float m_base_attackRange { get { return m_unit.m_attackRange; } }
        public float m_base_moveSpeed { get { return m_unit.m_moveSpeed; } }
        public float m_base_minDisToReachTarg { get { return m_unit.m_minDisToReachTarg; } }

        //! 현 상태
        public float m_attackHoldTime;
        public float m_attackRange;
        public float m_moveSpeed;
        public float m_minDisToReachTarg;

        //! 날때린 녀석        
        public float m_hp;
        public int m_attackDamage;
        public float m_attackCoolTime;
        public Dictionary<Guid, MUnit> m_damage_enemys = new Dictionary<Guid, MUnit>();
        
        public UnityAction<MUnit> m_changeStateDelegate;        

        void ChangeState(State state)
        {
            if (m_state == state) return;

            StopAllCoroutines();

            m_state = state;
            if (m_changeStateDelegate != null) m_changeStateDelegate(this);

            switch(state)
            {
                case State.Idle:

                    break;
                case State.Dead:
                    {
                        if (m_unit.m_deadPrefab != null)
                        {
                            GameObject obj = Instantiate(m_unit.m_deadPrefab, transform.position, Quaternion.identity) as GameObject;
                            obj.GetComponentInChildren<SpriteRenderer>().sprite = m_spriteRenderer.sprite;
                        }
                        
                        Destroy(transform.gameObject);
                    }
                    break;
                case State.Stun:
                    {

                    }
                    break;
            }
        }

        public void LevelUp()
        {
            m_level += 1;
            if (m_unit.m_levelUpPrefab != null) Instantiate(m_unit.m_levelUpPrefab, transform.position, Quaternion.identity);
        }

        public void Dead()
        {
            ChangeState(State.Dead);
        }
        //! 주체, 데미지 타입, 데미지 크기 등이 있어야한다.
        public void Damage(MUnit enemy, Damage damage)
        {
            //! 때린놈을 기억한다.
            //! 나의 주변의 아군에게 내가 맞은 사실을 알린다.
            BroadcastEnemy(enemy, 1, -1);

            m_hp -= damage.m_power;
            if(m_hp <= 0) { Dead(); }
            else
            {
                m_moving.ApplyForce(damage.m_force);

                //transform.DOMove(transform.position + (Vector3)damage.m_force, 0.5f);

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

            //! 최대 속도 조절
            m_moving.MaxSpeed = m_moveSpeed;

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

        public void BroadcastEnemy(MUnit unit, float range, int layerMask)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range, layerMask);

            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Unit") == true)
                {
                    MUnit team = col.GetComponent<MUnit>();
                    if (IsCanTargetingUnit(team) == false) continue;
                    if (IsEnemy(team) == true) continue;

                    team.AddAttackOnMeEnemy(unit);
                }
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
            yield return null;

            while (IsCanMove() == true && IsCanTargetingUnit(unit) == true && IsArrived(unit.transform.position, m_attackRange) == false)
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
            yield return null;

            while (IsArrived(point, m_minDisToReachTarg) == false)
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
            bool isCanMove = IsCanMove();
            m_pathFindingBehaviour.enabled = isCanMove;

            UpdateAnimDir();
        }

        private void PathMoveStop()
        {
            m_pathFindingBehaviour.enabled = false;
            //m_physicCharBehaviour.enabled = true;
            //m_rigidbody2d.isKinematic = false;

            m_moving.Acc = Vector3.zero;
            m_moving.Veloc = Vector3.zero;

            UpdateAnimDir();
        }

        bool AttackTarget(MUnit unit)
        {
            if (IsCanAttackUnit(unit) == false) return false;

            if (m_unit.m_attackPrefab != null) Instantiate(m_unit.m_attackPrefab, unit.transform.position, Quaternion.identity);

            Damage damage = new Damage(m_attackDamage, (unit.transform.position - transform.position).normalized * m_unit.m_attackForce);
            unit.Damage(this, damage);
            m_attackCoolTime = m_base_attackCoolTime;
            m_attackHoldTime = m_base_attackHoldTime;

            return true;
        }


        IEnumerator FindEnemy(float range, int layerMask, Action<MUnit> end)
        {
            yield return null;

            MUnit unit = null;
            while(FindEnemy(ref unit, transform.position, range, layerMask) == false)
            {
                yield return null;
            }
            if (end != null) end(unit);
        }
        

        IEnumerator AttackTarget(MUnit unit, Action end)
        {
            //! 유닛을 표적으로 쫒아가지만, 근처에 공격할수있는 다른적이 있다면 그 적을 공격한다.
            yield return null;
            
            while(IsCanTargetingUnit(unit) == true)
            {
                if (FindEnemy(ref unit, transform.position, m_attackRange, -1) == true) { PathMoveStop(); AttackTarget(unit); }
                else PathMoveUpdate(unit.transform.position);

                yield return null;
            }
            PathMoveStop();
            if (end != null) end();
        }

        IEnumerator FightingBack(Action<MUnit> end)
        {
            yield return null;

            MUnit unit;
            while(GetNearestAttackOnMeEnemy(out unit) == false)
            {
                yield return null;   
            }
            if (end != null) end(unit);
        }

        IEnumerator Stop(float time, Action end)
        {
            yield return time;
            if (end != null) end();
        }





        public void SetCommand(Command command)
        {
            //m_damage_enemys.Clear();

            StopAllCoroutines();

            //! 이런게 더러운데..쩝;
            PathMoveStop();
            //Debug.Log(name + " ] SetCommand : " + m_command + "-> " + command);
            m_command = command;
        }

        public void CommandNone()
        {
            //! 지속상태 선공격, 추격한다.
            SetCommand(Command.None);
            
            StartCoroutine(FightingBack((unit)=> { CommandAttackTarget(unit); }));
            StartCoroutine(FindEnemy(m_attackRange, -1, (unit) => { StartCoroutine(AttackTarget(unit, CommandHold)); }));
        }
        public void CommandHold()
        {
            //! 지속상태 선공격, 추격하지 않는다.
            SetCommand(Command.Hold);

            //! 적을찾고, 찾으면 쏘고, 벗어나면, 다시 커멘드 홀드호출
            StartCoroutine(FindEnemy(m_attackRange, -1, (unit) => { StartCoroutine(AttackTarget(unit, CommandHold)); }));
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

            StartCoroutine(PathMove(point, CommandHold));
        }

        public void CommandAttackGround(Vector2 point)
        {
            //! 목표지역으로 이동한다. 이벤트(적발견, 공격당함) 발생시 AttackTarget으로 변경된다.
            SetCommand(Command.Attack_Ground);
            
            StartCoroutine(PathMove(point, CommandNone));
            StartCoroutine(FightingBack((unit) => { CommandAttackTarget(unit); }));
            StartCoroutine(FindEnemy(m_attackRange, -1, (unit)=> { CommandAttackTarget(unit); }));
        }
        public void CommandAttackTarget(MUnit unit)
        {
            SetCommand(Command.Attack_Target);

            StartCoroutine(AttackTarget(unit, CommandNone));
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
            if((unit.transform.position - transform.position).magnitude > m_attackRange) return false;
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
            bool result = false;
            Collider2D[] cols = Physics2D.OverlapCircleAll(center, range, layerMask);
            float distance = range * range;
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Unit") == true)
                {
                    MUnit unit = col.GetComponent<MUnit>();
                    if (IsCanTargetingUnit(unit) == false) continue;
                    if (IsEnemy(unit) == false) continue;
                    float newdistance = (unit.transform.position - center).sqrMagnitude;
                    if (distance < newdistance) continue;

                    enemy = unit;
                    distance = newdistance;
                    result = true;
                }
            }
            return result;
        }
    }
}
