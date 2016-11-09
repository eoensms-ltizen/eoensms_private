using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(EffectTestScript))]
public class EffectTestScriptEditor : Editor {

    EffectTestScript m_effectTestScript { get { return (EffectTestScript)target; }}

    // Use this for initialization
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Load") == true)
        {
            m_effectTestScript.m_effectNames.Clear();

            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/2DxFX/Scripts/");
            foreach (string fileName in fileEntries)
            {
                if (fileName.Contains("meta") == true) continue;

                string[] temp = fileName.Split('/');
                string[] temp2 = temp[temp.Length - 1].Split('.');
                m_effectTestScript.m_effectNames.Add(temp2[0]);
            }
        }

        base.OnInspectorGUI();
    }

    
}
