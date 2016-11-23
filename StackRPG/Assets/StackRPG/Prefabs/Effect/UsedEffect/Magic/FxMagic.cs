using UnityEngine;
using System.Collections;
using DG.Tweening;

public class FxMagic : FxEffect
{
    public float m_magicCastingTime;
    public GameObject m_magicCasting;
    public GameObject m_magicAttack;

    public override void SetEffect(MUnit from, MUnit target, Damage damage)
    {
        Vector3 fromPosition = from.m_spriteRenderer.bounds.center;
        Vector3 targetPosition = target.m_spriteRenderer.bounds.center;

        GameObject casting = null;
        if (m_magicCasting != null) casting = Instantiate(m_magicCasting, targetPosition, Quaternion.identity) as GameObject;
        DOVirtual.DelayedCall(m_magicCastingTime, () => 
        {
            if (casting != null) Destroy(casting);

            Instantiate(m_magicAttack, targetPosition, Quaternion.identity);


            int teamID = from.m_teamId;

            if (m_targetCount == 1) { if (target != null) target.Damage(from, damage); }
            else
            {
                Collider2D[] cols = Physics2D.OverlapCircleAll(targetPosition, m_multiTargetRange);
                int count = m_targetCount;
                for (int i = 0; i < cols.Length; ++i)
                {
                    MUnit munit = cols[i].transform.GetComponent<MUnit>();
                    if (munit == null) continue;
                    if (munit.m_teamId == teamID) continue;
                    munit.Damage(from, damage);
                    count--;
                    if (count == 0) break;
                }
            }

            Destroy(gameObject);            
        });
    }
}
