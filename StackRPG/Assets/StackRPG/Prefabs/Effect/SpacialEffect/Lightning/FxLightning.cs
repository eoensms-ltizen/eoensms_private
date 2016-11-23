using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FxLightning : FxEffect
{
    public GameObject m_lightningPrefab;
    private List<Lightning> m_lightning = new List<Lightning>();
    public float m_chaningSpeed = 0.15f;
    public int m_chaningEffectCount = 2;
    public int m_chaningType = 0;

    public override void SetEffect(MUnit from, MUnit target, Damage damage)
    {
        for(int i = 0;i<m_chaningEffectCount;++i)
        {
            GameObject obj = Instantiate(m_lightningPrefab) as GameObject;
            obj.transform.SetParent(transform);
            obj.transform.position = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            m_lightning.Add(obj.GetComponent<Lightning>());
        }

        if(m_chaningType == 0) StartCoroutine(ChainLightning0(from, target, damage));
        else StartCoroutine(ChainLightning1(from, target, damage));
    }

    IEnumerator ChainLightning0(MUnit from, MUnit target, Damage damage)
    {
        int teamID = from.m_teamId;

        Vector3 lastFromPosition = from.m_spriteRenderer.bounds.center;
        MUnit lastTarget = target;
        for (int i = 0; i < m_targetCount; ++i)
        {
            Vector3 lastTargetPosition = lastTarget.m_spriteRenderer.bounds.center;

            for (int k = 0; k < m_lightning.Count; ++k)
            {
                m_lightning[k].SetFrom(lastFromPosition);
                m_lightning[k].SetTarget(lastTargetPosition);
                m_lightning[k].gameObject.SetActive(true);
            }
            
            lastTarget.Damage(from, damage);
            //! 죽여야 다음으로 체인이된다.
            //if (lastTarget != null && lastTarget.m_state != State.Dead) { break; }

            lastFromPosition = lastTargetPosition;
            lastTarget = null;
            yield return new WaitForSeconds(m_chaningSpeed);

            Collider2D[] cols = Physics2D.OverlapCircleAll(lastTargetPosition, m_multiTargetRange);
            for (int j = 0; j < cols.Length; ++j)
            {
                MUnit munit = cols[j].transform.GetComponent<MUnit>();
                if (munit == null) continue;
                if (munit.m_teamId == teamID) continue;

                lastTarget = munit;
                break;
            }
            //! 범위내에 없으면 체인 종료된다.
            if (lastTarget == null) { break; }
        }
        Destroy(gameObject, 1);
    }

    IEnumerator ChainLightning1(MUnit from, MUnit target, Damage damage)
    {
        int teamID = from.m_teamId;

        Vector3 lastFromPosition = from.m_spriteRenderer.bounds.center;
        MUnit lastTarget = target;
        for (int i = 0; i < m_targetCount; ++i)
        {
            Vector3 lastTargetPosition = lastTarget.m_spriteRenderer.bounds.center;

            for (int k = 0; k < m_lightning.Count; ++k)
            {
                m_lightning[k].SetFrom(lastFromPosition);
                m_lightning[k].SetTarget(lastTargetPosition);
                m_lightning[k].gameObject.SetActive(true);
            }
            
            lastTarget.Damage(from, damage);

            //! 죽여야 다음으로 체인이된다.
            //if (lastTarget != null && lastTarget.m_state!= State.Dead) { break; }

            lastTarget = null;
            yield return new WaitForSeconds(m_chaningSpeed);
            
            Collider2D[] cols = Physics2D.OverlapCircleAll(lastTargetPosition, m_multiTargetRange);
            for (int j = 0; j < cols.Length; ++j)
            {
                MUnit munit = cols[j].transform.GetComponent<MUnit>();
                if (munit == null) continue;
                if (munit.m_teamId == teamID) continue;

                lastTarget = munit;
                break;
            }

            //! 범위내에 없으면 체인 종료된다.
            if (lastTarget == null) {  break; }
        }
        Destroy(gameObject, 1);
    }
}
