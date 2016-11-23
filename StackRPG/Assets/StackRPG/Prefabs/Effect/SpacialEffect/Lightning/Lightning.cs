using UnityEngine;
using System.Collections;

public class Lightning : MonoBehaviour
{
    LineRenderer Line; // 라인렌더러

    public Transform From;
    public Vector3 m_lastFromPosition;

    public Transform Target; // 전기가 도달할 목적지의 Transform 오브젝트
    public Vector3 m_lastTargetPosition;
    public int Steps = 2; // 전기가 꺾어지는 지점의 개수
    public float Width = 1f; // 전기가 꺾어지는 간격 (값이 클수록 변화가 크다)
    public float Delay = 0.05f; // 각 지점의 값을 리셋하는 타이머 (적을수록 빨리 변화한다)

	void Start()
	{
        Line = GetComponent<LineRenderer>(); // 라인렌더러 컴포넌트를 가져온다.
        Line.SetVertexCount(Steps); // 전기를 구성할 버텍스 개수를 지정한다.
        StartCoroutine(UpdateLightning()); // 전기의 방향을 변화시킬 코루틴을 실행한다.
    }

    public void SetTarget(Transform target)
    {
        Target = target;
        m_lastTargetPosition = target.position;
    }

    public void SetFrom(Transform from)
    {
        From = from;
        m_lastFromPosition = from.position;
    }

    public void SetTarget(Vector3 position)
    {
        m_lastTargetPosition = position;
    }

    public void SetFrom(Vector3 position)
    {
        m_lastFromPosition = position;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator UpdateLightning()
    {
        while (true)
        {
            if (Target != null) m_lastTargetPosition = Target.position;
            if (From != null) m_lastFromPosition = From.position;

            transform.position = m_lastFromPosition;
            // 버텍스와 버텍스 사이의 간격을 구한다.
            float dist = Vector3.Distance(m_lastFromPosition, m_lastTargetPosition) / Steps;

            // 시작점과 끝점을 제외한 나머지 지점만 변화시킨다.
            for (int i = 1; i < Steps - 1; i++)
            {
                // 시작점부터 끝점까지 dist 간격으로 이동시킨다.
                Vector3 pos = Vector3.MoveTowards(m_lastFromPosition, m_lastTargetPosition, i * dist);
                pos += transform.up * Random.Range(-Width, Width); // 상하로 랜덤하게 변화시킨다.
                pos += transform.right * Random.Range(-Width, Width); // 좌우로 랜덤하게 변화시킨다.
                Line.SetPosition(i, pos); // 변화된 버텍스 좌표를 설정한다.
            }
            yield return new WaitForSeconds(Delay); // 지정된 시간만큼 딜레이시킨다.
        }
    }

    void Update()
    {
        transform.LookAt(m_lastTargetPosition); // 시작점이 항상 끝점을 바라보게 한다.
        Line.SetPosition(0, m_lastFromPosition); // 시작점의 좌표를 고정한다.
        Line.SetPosition(Steps - 1, m_lastTargetPosition); // 끝점의 좌표를 목표물에 고정한다.
    }
}
