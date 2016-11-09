using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEditor.Sprites;
using System.Collections.Generic;

[CustomEditor(typeof(HiResScreenShots))]
public class HiResScreenShotsEditor : Editor
{
    HiResScreenShots m_hiResScreenShots { get { return (HiResScreenShots)target; } }

    static TabType m_tabType;
    static string m_fileName = "Loading.jpeg";

    public override void OnInspectorGUI()
    {
        //serializedObject.Update();

        string[] toolBarButtonNames = System.Enum.GetNames(typeof(TabType));
        m_tabType = (TabType)GUILayout.Toolbar((int)m_tabType, toolBarButtonNames);

        switch (m_tabType)
        {
            case TabType.Play: base.OnInspectorGUI(); break;
            case TabType.Edit: DrawEditTab(); break;
        }

        //serializedObject.ApplyModifiedProperties();
        //SceneView.RepaintAll();
    }

    void DrawEditTab()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("FileName");
        m_fileName = GUILayout.TextField(m_fileName);
        if (GUILayout.Button("ScreenShot") == true)
        {   
            m_hiResScreenShots.TakeHiResShotNow(m_fileName);
        }
        GUILayout.EndHorizontal();
    }
}
