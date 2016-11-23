using UnityEngine;
using System.Collections;

public class LookAtTest : MonoBehaviour
{
    public Transform m_missile;
	
	void Update ()
    {
        Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - m_missile.transform.position;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        m_missile.transform.rotation = Quaternion.Euler(0f, 0f, rot_z);
    }
}
