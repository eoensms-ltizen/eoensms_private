using UnityEngine;
using System.Collections;

namespace CreativeSpore
{

    [RequireComponent(typeof(BoxCollider))]
    public class TeleporterBehaviour : MonoBehaviour
    {
        [Tooltip("Set the name of the target scene")]
        public string TargetSceneName;
        [Tooltip("Set the name of the target teleporter")]
        public string TargetTeleporterName;

        BoxCollider m_boxCollider;         

        void Reset()
        {
            m_boxCollider = GetComponent<BoxCollider>();
            m_boxCollider.isTrigger = true;
        }

        void OnLevelWasLoaded()
        {
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                TeleportTo(player.gameObject, TargetTeleporterName);
                Destroy(this);
            }
        }

        void TeleportTo(GameObject srcObj, string dstObjName)
        {
            GameObject targetTeleport = GameObject.Find(dstObjName);
            if (targetTeleport == null)
            {
                Debug.LogWarning(" Teleport destination not found: " + dstObjName);
            }
            else
            {
                Vector3 targetPos = targetTeleport.transform.position;
                targetPos.z = transform.position.z;
                srcObj.transform.position = targetPos;
            }
        }

        void Start()
        {
            Reset();
        }

        void OnTriggerStay(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && Input.GetKeyDown(KeyCode.Return))
            {
#if UNITY_5_3
                if (string.IsNullOrEmpty(TargetSceneName) || TargetSceneName == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
#else
                if ( string.IsNullOrEmpty(TargetSceneName) || TargetSceneName == Application.loadedLevelName)
#endif                    
                {
                    TeleportTo(player.gameObject, TargetTeleporterName);
                }
                else
                {
                    TeleporterBehaviour teleportComp = player.gameObject.AddComponent<TeleporterBehaviour>();
                    teleportComp.TargetTeleporterName = TargetTeleporterName;
#if UNITY_5_3
                    UnityEngine.SceneManagement.SceneManager.LoadScene(TargetSceneName);
#else
                    Application.LoadLevel(TargetSceneName);
#endif
                }
            }
        }
    }
}