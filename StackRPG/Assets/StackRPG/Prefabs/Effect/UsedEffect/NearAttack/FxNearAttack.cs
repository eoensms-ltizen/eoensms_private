using UnityEngine;
using System.Collections;
using DG.Tweening;

public class FxNearAttack : FxEffect
{
    public float m_nearAttackHitTime;
    public GameObject m_nearAttack;

    public override void SetEffect(MUnit from, MUnit target, Damage damage)
    {
        Vector3 fromPosition = from.m_spriteRenderer.bounds.center;
        Vector3 targetPosition = target.m_spriteRenderer.bounds.center;

        if (m_nearAttack != null) Instantiate(m_nearAttack, targetPosition, Quaternion.identity);
        DOVirtual.DelayedCall(m_nearAttackHitTime, () => 
        {
            if (target != null) target.Damage(from, damage);

            Destroy(gameObject);
        });
    }
}
