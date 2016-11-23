using UnityEngine;
using DG.Tweening;

public class FxGun : FxEffect
{
    public float m_gunDuration;
    public GameObject m_gunStart;
    public GameObject m_gunMove;
    public GameObject m_gunArrived;

    public override void SetEffect(MUnit from, MUnit target, Damage damage)
    {
        Vector3 fromPosition = from.m_spriteRenderer.bounds.center;
        Vector3 targetPosition = target.m_spriteRenderer.bounds.center;

        if (m_gunStart != null) Instantiate(m_gunStart, fromPosition, Quaternion.identity);
        if (m_gunMove != null)
        {
            Transform obj1 = (Instantiate(m_gunMove, fromPosition, Quaternion.identity) as GameObject).transform;
            MSettings.LookAt2D(obj1, targetPosition);
            obj1.DOMove(targetPosition, m_gunDuration).OnComplete(() =>
            {
                if (m_gunArrived != null) Instantiate(m_gunArrived, targetPosition, Quaternion.identity);

                if (target != null) target.Damage(from, damage);

                Destroy(gameObject);
            });
        }
    }
}
