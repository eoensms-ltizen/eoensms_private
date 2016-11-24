using UnityEngine;

public abstract class FxEffect : MonoBehaviour
{   
    public int m_targetCount;
    public float m_multiTargetRange;

    public abstract void SetEffect(MUnit from, MUnit target, Damage damage);
}
