using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Events;
using UnityEditor;


public abstract class FxEffect : MonoBehaviour
{   
    public int m_targetCount;
    public float m_multiTargetRange;

    public abstract void SetEffect(MUnit from, MUnit target, Damage damage);
}
