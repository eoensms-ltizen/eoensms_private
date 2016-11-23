using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    UnitData m_unitData { get { return (UnitData)target; } }
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

    public enum ValueType
    {
        Cost,
        Hp,
        Damage,
    }

    private static ValueType m_valueType = ValueType.Cost;
    private static ValunceTable m_valunceTable;
    private static bool m_showDPS = false;
    private int[] m_tempTable = new int[0];
    private int[] m_baseTable = new int[0];

    

    void DrawEditTab()
    {
        m_showDPS = EditorGUILayout.Toggle("ShowDPS", m_showDPS);
        if (m_showDPS)
        {
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < m_unitData.m_unitData.m_upgradeCost.Length; ++i)
            {
                Unit unit = m_unitData.m_unitData;
                float dps = unit.m_attackDamage[i] / unit.m_attackCoolTime;
                float UnitValue = (unit.m_hp[i] + dps * dps);
                float UnitValueFormPrice = UnitValue/ unit.m_makePrice;
                GUILayout.Label(i + " Level UnitValue = " + UnitValue + ", / price = " + UnitValueFormPrice);
            }
            EditorGUILayout.EndVertical();
        }

        string[] toolBarButtonNames = System.Enum.GetNames(typeof(ValueType));
        m_valueType = (ValueType)GUILayout.Toolbar((int)m_valueType, toolBarButtonNames);
        switch (m_valueType)
        {
            case ValueType.Cost:
                m_valunceTable = m_unitData.m_upgradeCostValunceTable;
                break;
            case ValueType.Hp:
                m_valunceTable = m_unitData.m_hpValunceTable;
                break;
            case ValueType.Damage:
                m_valunceTable = m_unitData.m_damageValunceTable;
                break;
        }

        m_valunceTable.firstValue = EditorGUILayout.IntField("First", m_valunceTable.firstValue);
        m_valunceTable.length = EditorGUILayout.IntField("Length", m_valunceTable.length);
        m_valunceTable.modulusValue = EditorGUILayout.FloatField("Modulus ", m_valunceTable.modulusValue);

        switch (m_valueType)
        {
            case ValueType.Cost:
                m_baseTable = m_unitData.m_unitData.m_upgradeCost;
                break;
            case ValueType.Hp:
                m_baseTable = m_unitData.m_unitData.m_hp;
                break;
            case ValueType.Damage:
                m_baseTable = m_unitData.m_unitData.m_attackDamage;
                break;
        }

        if (GUILayout.Button("Calculate") == true)
        {
            m_tempTable = new int[m_valunceTable.length];
            for (int i = 0; i < m_valunceTable.length; ++i)
            {
                m_tempTable[i] = (int)(m_valunceTable.firstValue * Mathf.Pow(m_valunceTable.modulusValue, i));
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.Label("TempTable");
        for (int i = 0; i < m_tempTable.Length; ++i)
        {
            m_tempTable[i] = EditorGUILayout.IntField("" + i, m_tempTable[i]);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        GUILayout.Label("RealTable");
        for (int i = 0; i < m_baseTable.Length; ++i)
        {
            m_baseTable[i] = EditorGUILayout.IntField("" + i, m_baseTable[i]);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Save") == true)
        {
            switch (m_valueType)
            {
                case ValueType.Cost:
                    m_unitData.m_unitData.m_upgradeCost = new int[m_tempTable.Length];
                    m_baseTable = m_unitData.m_unitData.m_upgradeCost;
                    break;
                case ValueType.Hp:
                    m_unitData.m_unitData.m_hp = new int[m_tempTable.Length];
                    m_baseTable = m_unitData.m_unitData.m_hp;
                    break;
                case ValueType.Damage:
                    m_unitData.m_unitData.m_attackDamage = new int[m_tempTable.Length];
                    m_baseTable = m_unitData.m_unitData.m_attackDamage;
                    break;
            }

            for (int i = 0; i < m_tempTable.Length; ++i)
            {
                m_baseTable[i] =  m_tempTable[i];
            }
        }
    }

}
