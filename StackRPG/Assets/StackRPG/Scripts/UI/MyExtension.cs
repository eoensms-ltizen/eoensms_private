using UnityEngine;
using System.Collections;
using CreativeSpore;

public static class MyExtension
{
    public static void SetUnit(this CharAnimationControllerUI charAnimationControllerUI, Unit unit)
    {
        charAnimationControllerUI.SetCharAnimationController(unit.m_prefab.GetComponent<CharAnimationController>());
    }

    public static void SetCharAnimationController(this CharAnimationControllerUI charAnimationControllerUI, CharAnimationController charAnimationController)
    {
        charAnimationControllerUI.SpriteCharSet = charAnimationController.SpriteCharSet;
        charAnimationControllerUI.CharsetType = (CharAnimationControllerUI.eCharSetType)charAnimationController.CharsetType;
        charAnimationControllerUI.CreateSpriteFrames();
    }
}
