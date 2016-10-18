using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using CreativeSpore.RpgMapEditor;

namespace CreativeSpore
{
    [RequireComponent(typeof(MovingBehaviour))]
    [RequireComponent(typeof(PhysicCharBehaviour))]
    [RequireComponent(typeof(MapPathFindingBehaviour))]
    [RequireComponent(typeof(CharAnimationController))]
	public class FollowerAI : MonoBehaviour {

		GameObject m_player;
		MovingBehaviour m_moving;
		PhysicCharBehaviour m_phyChar;
        MapPathFindingBehaviour m_pathFindingBehaviour;
        CharAnimationController m_animCtrl;

        public bool LockAnimDir = false;
        /// <summary>
        /// Distance the object can see the target to follow him
        /// </summary>
        public float SightDistance = 4f;

        /// <summary>
        /// If true, sight line will be blocked by block tiles and enemy won't follow the hidden target
        /// </summary>
        public bool IsSightBlockedByBlockedTiles = true;
        /// <summary>
        /// Distance to stop doing path finding and just go to player position directly
        /// </summary>
        public float MinDistToReachTarget = 0.32f;
		public float AngRandRadious = 0.16f;
		public float AngRandOff = 15f;
		float m_fAngOff;

		// Use this for initialization
		void Start () 
		{
			m_fAngOff = 360f * Random.value;
			m_player = GameObject.FindGameObjectWithTag("Player");
			m_moving = GetComponent<MovingBehaviour>();
			m_phyChar = GetComponent<PhysicCharBehaviour>();
            m_pathFindingBehaviour = GetComponent<MapPathFindingBehaviour>();
            m_animCtrl = GetComponent<CharAnimationController>();
		}

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Handles.CircleCap(0, transform.position, transform.rotation, SightDistance);
        }
#endif

        void UpdateAnimDir()
        {
            if (m_moving.Veloc.magnitude >= (m_moving.MaxSpeed / 2f))
            {
                if (Mathf.Abs(m_moving.Veloc.x) > Mathf.Abs(m_moving.Veloc.y))
                {
                    if (m_moving.Veloc.x > 0)
                        m_animCtrl.CurrentDir = CharAnimationController.eDir.RIGHT;
                    else if (m_moving.Veloc.x < 0)
                        m_animCtrl.CurrentDir = CharAnimationController.eDir.LEFT;
                }
                else
                {
                    if (m_moving.Veloc.y > 0)
                        m_animCtrl.CurrentDir = CharAnimationController.eDir.UP;
                    else if (m_moving.Veloc.y < 0)
                        m_animCtrl.CurrentDir = CharAnimationController.eDir.DOWN;
                }
            }
        }

		// Update is called once per frame
		void Update() 
		{
            m_pathFindingBehaviour.TargetPos = m_player.transform.position;
            Vector3 vTarget = m_player.transform.position; vTarget.z = transform.position.z;

            Ray2D sightRay = new Ray2D(transform.position, vTarget - transform.position);
            float distToTarget = (vTarget - transform.position).magnitude;

            float fSightBlockedDist = IsSightBlockedByBlockedTiles? RpgMapHelper.Raycast(sightRay, distToTarget) : -1f;

            // NOTE: fSightBlockedDist will be -1f if sight line is not blocked by blocked collision tile
            if (distToTarget >= SightDistance || fSightBlockedDist >= 0f)
            {
                // Move around
                m_pathFindingBehaviour.enabled = false;
                vTarget = transform.position;
                m_fAngOff += Random.Range(-AngRandOff, AngRandOff);
                Vector3 vOffset = Quaternion.AngleAxis(m_fAngOff, Vector3.forward) * (AngRandRadious * Vector3.right);
                vTarget += vOffset;
                m_moving.Arrive(vTarget);
            }
            else // Follow the player
            {                
                // stop following the path when closed enough to target
                m_pathFindingBehaviour.enabled = (vTarget - transform.position).magnitude > MinDistToReachTarget;
                if (!m_pathFindingBehaviour.enabled)
                {
                    m_fAngOff += Random.Range(-AngRandOff, AngRandOff);
                    Vector3 vOffset = Quaternion.AngleAxis(m_fAngOff, Vector3.forward) * (AngRandRadious * Vector3.right);
                    vTarget += vOffset;
                    Debug.DrawLine(transform.position, m_player.transform.position, Color.blue);
                    Debug.DrawRay(m_player.transform.position, vOffset, Color.blue);

                    m_moving.Arrive(vTarget);
                }
            }

            //+++avoid obstacles
            Vector3 vTurnVel = Vector3.zero;
			if ( 0 != (m_phyChar.CollFlags & PhysicCharBehaviour.eCollFlags.RIGHT))
			{
                vTurnVel.x = -m_moving.MaxSpeed;
			}
			else if ( 0 != (m_phyChar.CollFlags & PhysicCharBehaviour.eCollFlags.LEFT))
			{
                vTurnVel.x = m_moving.MaxSpeed;
			}
			if ( 0 != (m_phyChar.CollFlags & PhysicCharBehaviour.eCollFlags.DOWN))
			{
                vTurnVel.y = m_moving.MaxSpeed;
			}
			else if ( 0 != (m_phyChar.CollFlags & PhysicCharBehaviour.eCollFlags.UP))
			{
                vTurnVel.y = -m_moving.MaxSpeed;
			}
            if( vTurnVel != Vector3.zero )
            {
                m_moving.ApplyForce(vTurnVel - m_moving.Veloc);
            }
            //---

            //fix to avoid flickering of the creature when collides with wall
            if (Time.frameCount % 16 == 0)
            //---            
            {
                if (!LockAnimDir) UpdateAnimDir();
            }
		}
	}
}