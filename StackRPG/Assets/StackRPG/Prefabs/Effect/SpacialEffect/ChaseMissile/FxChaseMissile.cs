using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FxChaseMissile : FxEffect
{
    public GameObject m_chaseMissilePrefab;

    public override void SetEffect(MUnit from, MUnit target, Damage damage)
    {
        int teamID = from.m_teamId;
        Transform targetTransform = target.transform;

        List<MUnit> targets = new List<MUnit>();
        
        //! 범위내의 적들 찾기
        Collider2D[] cols = Physics2D.OverlapCircleAll(targetTransform.position, m_multiTargetRange);
        int count = m_targetCount;
        for (int i = 0; i < cols.Length; ++i)
        {
            MUnit munit = cols[i].transform.GetComponent<MUnit>();
            if (munit == null) continue;
            if (munit.m_teamId == teamID) continue;

            targets.Add(munit);

            count--;
            if (count == 0) break;
        }

        for (int i = 0; i < targets.Count; ++i)
        {
            MUnit realTarget = targets[i];
            GameObject obj = Instantiate(m_chaseMissilePrefab, from.m_spriteRenderer.bounds.center, Quaternion.Euler(0f, 0f, MSettings.Random(-1.0f, 1.0f) * 100)) as GameObject;
            ChaseMissile chaseMissile = obj.GetComponent<ChaseMissile>();
            chaseMissile.SetTarget(realTarget.transform,()=> 
            {
                if (realTarget != null) realTarget.Damage(from, damage);
            });
        }

        Destroy(gameObject);
    }
}
