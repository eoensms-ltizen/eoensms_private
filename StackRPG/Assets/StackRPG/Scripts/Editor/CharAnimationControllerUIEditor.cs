using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using CreativeSpore;

[CustomEditor(typeof(CharAnimationControllerUI))]
public class CharAnimationControllerUIEditor : Editor
{

    public override void OnInspectorGUI()
    {
        CharAnimationControllerUI targetComp = (CharAnimationControllerUI)target;
        if (GUILayout.Button("Open Editor..."))
        {
            CharAnimationUIWindow.Init(targetComp);
        }

        if (targetComp.IsDataBroken()) targetComp.CreateSpriteFrames();

        EditorGUI.BeginChangeCheck();
        targetComp.SpriteCharSet = (Sprite)EditorGUILayout.ObjectField("SpriteCharSet", targetComp.SpriteCharSet, typeof(Sprite), false);
        targetComp.CharsetType = (CharAnimationControllerUI.eCharSetType)EditorGUILayout.EnumPopup("Charset Type", targetComp.CharsetType);
        if (EditorGUI.EndChangeCheck())
        {
            targetComp.CreateSpriteFrames();
        }

        targetComp.TargetImage = (Image)EditorGUILayout.ObjectField("Target Image", targetComp.TargetImage, typeof(Image), true);
        targetComp.AnimSpeed = EditorGUILayout.FloatField("Anim Speed", targetComp.AnimSpeed);
        targetComp.IsPingPongAnim = EditorGUILayout.ToggleLeft("Ping-Pong Anim", targetComp.IsPingPongAnim);
        targetComp.CurrentDir = (CharAnimationControllerUI.eDir)EditorGUILayout.EnumPopup("Facing Dir", targetComp.CurrentDir);

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetComp);
        }
    }
}