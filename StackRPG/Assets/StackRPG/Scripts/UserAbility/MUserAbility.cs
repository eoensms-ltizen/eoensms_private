using UnityEngine;
using System.Collections;

public abstract class MUserAbility
{
    public AbilityType m_abilityType;
    public MUser m_muser;
    public abstract void AttachAbility(MUser user);
    public abstract void DetachAbility(MUser user);
}
