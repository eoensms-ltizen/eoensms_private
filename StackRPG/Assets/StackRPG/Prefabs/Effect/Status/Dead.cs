using UnityEngine;
using System.Collections;

public class Dead : MonoBehaviour
{
    public void OnFinish()
    {
        Destroy(gameObject);
    }
}
