using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace stackRPG
{
    public class MFollowObjectBehaviour : MonoBehaviour
    {

        public float DampTime = 0.15f;
        public List<MUser> m_targets = new List<MUser>();

        public Vector3 m_target;

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
            MGameManager.Instance.m_changeUserEvent += SetTarget;
        }

        // Update is called once per frame
        void Update()
        {
            if (m_camera == null) return;

            Vector3 point = m_camera.WorldToViewportPoint(m_target);
            Vector3 delta = m_target - m_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, DampTime);
        }

        public void SetTarget(Vector3 position)
        {
            m_targets.Clear();
            m_target = position;
        }

        public void SetTarget(MUser user)
        {
            m_targets.Clear();
            m_targets.Add(user);
        }
    }

}
