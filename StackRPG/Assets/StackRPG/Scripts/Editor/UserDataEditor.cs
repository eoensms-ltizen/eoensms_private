using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[CustomEditor(typeof(UserData))]
public class UserDataEditor : Editor
{
    UserData m_userData { get { return (UserData)target; } }
    private static TabType m_tabType = TabType.Play;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        string[] toolBarButtonNames = System.Enum.GetNames(typeof(TabType));
        m_tabType = (TabType)GUILayout.Toolbar((int)m_tabType, toolBarButtonNames);
        switch (m_tabType)
        {
            case TabType.Play: DrawPlayTab(); break;
            case TabType.Edit: DrawEditTab(); break;
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed) EditorUtility.SetDirty(target);
    }

    void DrawPlayTab()
    {
        base.DrawDefaultInspector();
    }

    void DrawEditTab()
    {

    }
}
