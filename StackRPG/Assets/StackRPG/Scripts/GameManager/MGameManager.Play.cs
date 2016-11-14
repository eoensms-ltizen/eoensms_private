using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;
using stackRPG;

public partial class MGameManager : Singleton<MGameManager>
{
    
    IEnumerator Play()
    {
        MGameCamera.Instance.SetPlayCamera();

        //! 유저 상태값 바꾸기
        for (int i = 0; i < m_userList.Count; ++i)
        {
            m_userList[i].Play();
        }

        //! 유닛들 어택땅 (자동공격) 관련
        StartCoroutine(AutoAttackGround());
        

        //! 유저 컨트롤
        StartCoroutine(UpdateUserControll());

        //! 유저의 유닛카운트가 0이면, 유저를 죽인다.
        while (m_userList.Count > 1)
        {
            for (int i = 0; i < m_userList.Count; ++i)
            {
                MUser user = m_userList[i];
                if (user.m_aliveUnits.Count == 0)
                {
                    Debug.Log("user Die : " + user.m_id);
                    user.Dead();
                    m_userList.Remove(user);
                    i--;
                }
                yield return null;
            }
        }

        if (m_owner.m_state != UserState.Dead) ChangeGameState(GameState.ClearStage);
        else ChangeGameState(GameState.Result);
    }

    IEnumerator UpdateUserControll()
    {
        while (m_state == GameState.Play)
        {
            if (m_isCanControllUser)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    m_owner.AttackGround(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }

                if (Input.GetMouseButtonDown(1))
                {
                    m_owner.MoveGround(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 딜레이마다 반복해서 서로 자동 어택땅한다.
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator AutoAttackGround()
    {
        while (m_state == GameState.Play)
        {
            AutoAttackAllUser();
            yield return new WaitForSeconds(m_delayAttackGround);
        }
    }
    
    /// <summary>
    /// 다음 인덱스의 유저를 타겟으로 공격한다. 살아남은 유저가 혼자라면, 계산하지 않는다.
    /// </summary>
    private void AutoAttackAllUser()
    {
        for (int i = 0; i < m_userList.Count; ++i)
        {
            MUser user = m_userList[i];

            //! 오너는 자동 어택 하지 않는다.
            //if (user == m_owner) continue; 

            if (user.m_state != UserState.Play) continue;

            int targetIndex = i;
            MUser targetUser = null;
            while (targetUser == null)
            {
                targetIndex += 1;
                if (targetIndex >= m_userList.Count) targetIndex = 0;
                targetUser = m_userList[targetIndex];
                //! 자기 밖에 안남았으니 끝낸다.
                if (targetUser == user) return;
                if (targetUser.m_state != UserState.Play) { targetUser = null; continue; }
            }

            Vector3 attackPosition = Vector3.zero;
            if (GetUserUnitCenterPosition(targetUser.m_id, ref attackPosition) == true) user.AttackGround(attackPosition);
        }
    }

    /// <summary>
    /// 해당 유저의 타겟포인트를 계산한다. 유닛들의 중심을 구하고, 중심에서 가까운 유닛의 위치를 반환한다.
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool GetUserUnitCenterPosition(string userID, ref Vector3 position)
    {
        MUser user = GetUser(userID);
        if (user == null || user.m_aliveUnits.Count == 0) return false;

        //! 중심 위치 선정
        Vector3 totalPosition = Vector3.zero;
        for (int i = 0; i < user.m_aliveUnits.Count; ++i)
        {
            totalPosition += user.m_aliveUnits[i].transform.position;
        }
        position = totalPosition / user.m_aliveUnits.Count;

        //! 중심에서 가장 가까운 유닛의 위치를 타겟으로삼는다.
        //! 그러지 않으면, 갈수없는곳을 타겟으로 삼을수도있다.
        int unitIndex = -1;
        float sqrMagnitude = 0;
        for (int i = 0; i < user.m_aliveUnits.Count; ++i)
        {
            bool isChoose = false;
            float tempMagnitue = Vector3.SqrMagnitude(totalPosition - user.m_aliveUnits[i].transform.position);

            if (unitIndex == -1) isChoose = true;
            else if (sqrMagnitude > tempMagnitue) isChoose = true;

            if (isChoose == true)
            {
                unitIndex = i;
                sqrMagnitude = tempMagnitue;
            }
        }
        return true;
    }
}
