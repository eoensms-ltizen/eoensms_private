using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3F;
    private float yVelocity = 0.0F;
    public float m_maxSpeed;
    void Update()
    {
        float newPosition = Mathf.SmoothDamp(transform.position.y, target.position.y, ref yVelocity, smoothTime, m_maxSpeed);
        transform.position = new Vector3(transform.position.x, newPosition, transform.position.z);
    }
}