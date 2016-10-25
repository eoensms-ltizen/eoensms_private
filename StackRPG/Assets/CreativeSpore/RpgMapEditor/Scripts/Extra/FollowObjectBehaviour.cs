using UnityEngine;
using System.Collections;
using stackRPG;

namespace CreativeSpore
{
	public class FollowObjectBehaviour : MonoBehaviour {

		public float DampTime = 0.15f;
		public Transform m_target;
        public Transform m_positionTarget;
		private Vector3 velocity = Vector3.zero;
        private Camera m_camera;

        void Awake()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            yield return StartCoroutine(MGameManager.Instance.WaitPrecess());
            m_camera = GetComponent<Camera>();
            MGameManager.Instance.m_changeUserEvent += OnChangeUser;
        }
		
        void OnChangeUser(MUser user)
        {
            if (user != null) SetTarget(user.m_startPoint);
        }
		// Update is called once per frame
		void Update () 
		{
			if (m_target)
			{
                Vector3 point = m_camera.WorldToViewportPoint(m_target.position);
                Vector3 delta = m_target.position - m_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
				Vector3 destination = transform.position + delta;
				transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, DampTime);
			}		
		}

        public void SetTarget(Transform target)
        {
            m_target = target;
        }

        public void SetTarget(Vector3 position)
        {
            m_positionTarget.position = position;
            SetTarget(m_positionTarget);
        }
	}
}
