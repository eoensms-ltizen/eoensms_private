using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace stackRPG
{
    public interface MCommander
    {
        //! Action 이 끝났다는 이벤트를 받아서, 자신의 커멘드에 따라 다음번 Action을 하라고 전달한다.

        void OnFinishAction(UnitAction action);

        void UpdateAction(UnitAction action);
    }
}
