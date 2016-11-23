using UnityEngine;
using UnityEditor;
using System.Collections;
using CreativeSpore;
using CreativeSpore.RpgMapEditor;

[CustomEditor(typeof(MUnit))]
public class MUnitEditor : Editor
{
    MUnit myTarget { get { return (MUnit)target; } }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    public void OnSceneGUI()
    {
        MUnit myTarget = (MUnit)target;

        Vector2 position = myTarget.m_spriteRenderer.bounds.center - myTarget.m_spriteRenderer.bounds.size * 0.5f;

        Rect rect = new Rect(position, myTarget.m_spriteRenderer.bounds.size);        
        Handles.DrawSolidRectangleWithOutline(rect, new Color(0, 0, 0, 0), new Color(0f, 0.4f, 1f));        
       
        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }
}
