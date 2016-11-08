using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using CreativeSpore.RpgMapEditor;
using UnityEditorInternal;
using System;

public enum TabType
{
    Play,
    Edit,
}

[CustomEditor(typeof(MapFactory))]
public class MapFactoryEditor : Editor
{
    MapFactory m_mapFactory { get { return (MapFactory)target; } }
    MapData m_mapData { get { return m_mapFactory.m_mapData; } }

    AutoTileMap m_autoTileMap;
    ReorderableList m_layerList;

    int m_userIdx;

    

    public enum EditType
    {
        StartPoint,
        AttackPoint,
    }
    static EditType m_editType = EditType.StartPoint;
    private static TabType m_tabType = TabType.Play;

    void OnEnable()
    {
        m_autoTileMap = FindObjectOfType<AutoTileMap>();
        if (m_autoTileMap == null)
        {
            string prefabPath = "Assets/CreativeSpore/RpgMapEditor/Custom/Prefabs/Map/AutoTileMap.prefab";
            Instantiate(AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)));
            m_autoTileMap = FindObjectOfType<AutoTileMap>();
        }

        m_autoTileMap.Tileset = m_mapData.m_map.m_autoTileset;
        m_autoTileMap.MapData = m_mapData.m_map.m_autoTileMapData;

        //! 이식
        m_mapFactory.m_makeUnitPositions = m_mapData.m_map.m_makeUnitPositions;

        m_layerList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_makeUnitPositions"), true, true, true, true);
        m_layerList.drawElementCallback += _LayerList_DrawElementCallback;
        m_layerList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Starting Points");
        };
        m_layerList.onChangedCallback = (ReorderableList list) =>
        {
            m_mapData.m_map.m_makeUnitPositions = m_mapFactory.m_makeUnitPositions;
            m_userIdx = m_layerList.index;
            serializedObject.ApplyModifiedProperties();
        };
        m_layerList.onAddCallback = (ReorderableList list) =>
        {   

            list.index = m_mapData.m_map.m_makeUnitPositions.Count;
            m_mapFactory.m_makeUnitPositions.Add(new StartingPoint());
        };
    }

    void OnDisable()
    {

    }

    
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

        SceneView.RepaintAll();
    }

    void DrawPlayTab()
    {
        base.OnInspectorGUI();
    }


    private void _LayerList_DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        float savedLabelWidth = EditorGUIUtility.labelWidth;
        var element = m_layerList.serializedProperty.GetArrayElementAtIndex(index);
        rect.y += 2;
        float elemWidth = 20; float elemX = rect.x;
        EditorGUI.PropertyField(
            new Rect(elemX, rect.y, elemWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("m_visible"), GUIContent.none);
        elemX += elemWidth; elemWidth = Mathf.Clamp(Screen.width - 500, 50, 240);
        EditorGUI.PropertyField(
            new Rect(elemX, rect.y, elemWidth, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("m_color"), GUIContent.none);
        elemX += elemWidth; elemWidth = Mathf.Clamp(Screen.width - 500, 50, 240);
        EditorGUI.TextArea(
            new Rect(elemX, rect.y, elemWidth, EditorGUIUtility.singleLineHeight), 
            m_mapData.m_map.m_makeUnitPositions[index].m_positions.Count + "/" + m_mapData.m_map.m_canMakeUnitCount);
        elemX += elemWidth; elemWidth = Mathf.Clamp(Screen.width - 500, 50, 240);
        EditorGUIUtility.labelWidth = savedLabelWidth;
    }

   
    
    void DrawEditTab()
    {

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.HelpBox(m_userIdx + " 유저의 [" + m_editType.ToString() + "] 을 찍으세요", MessageType.Info);

        string[] toolBarButtonNames = System.Enum.GetNames(typeof(EditType));
        m_editType = (EditType)GUILayout.Toolbar((int)m_editType, toolBarButtonNames);

        m_mapData.m_map.m_canMakeUnitCount = EditorGUILayout.IntField("Unit Count", m_mapData.m_map.m_canMakeUnitCount);
        m_layerList.DoLayoutList();
        m_userIdx = Mathf.Max(m_layerList.index, 0);
        
        if(EditorGUI.EndChangeCheck() == true) EditorUtility.SetDirty(target);
    }

    void SetAttackPoint(Vector2 pos)
    {
        m_mapData.m_map.m_attackPoint = pos;
    }
    void AddUserPoint(int userID, Vector2 pos)
    {
        if (m_mapData.m_map.m_makeUnitPositions.Count <= userID) return;

        if (m_mapData.m_map.m_makeUnitPositions[userID].m_positions.Count >= m_mapData.m_map.m_canMakeUnitCount) return;

        if (m_mapData.m_map.m_makeUnitPositions[userID].m_positions.Contains(pos) == true) return;

        m_mapData.m_map.m_makeUnitPositions[userID].m_positions.Add(pos);
    }

    void RemoveUserPoint(int userID, Vector2 pos)
    {
        if (m_mapData.m_map.m_makeUnitPositions.Count <= userID) return;

        if (m_mapData.m_map.m_makeUnitPositions[userID].m_positions.Contains(pos) == false) return;

        m_mapData.m_map.m_makeUnitPositions[userID].m_positions.Remove(pos);
    }

    private int m_tilesetSelStart;
    private int m_tilesetSelEnd;

    int m_selectedTileId = 0;

    private int m_prevMouseTileX = -1;
    private int m_prevMouseTileY = -1;
    private int m_startDragTileX = -1;
    private int m_startDragTileY = -1;
    private int m_dragTileX = -1;
    private int m_dragTileY = -1;

    void UpdateSelectToMouse(Action<int,int> onDragAndTileChanged, Action onDragFinish,bool isDrawDragSelect)
    {
        Rect rSceneView = new Rect(0, 0, Screen.width, Screen.height);
        if (rSceneView.Contains(Event.current.mousePosition))
        {
            UpdateMouseInputs();

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane hPlane = new Plane(Vector3.forward, Vector3.zero);
            float distance = 0;
            if (hPlane.Raycast(ray, out distance))
            {
                Vector3 vPos = ray.GetPoint(distance);

                if (m_isMouseRight || m_isMouseLeft)
                {
                    int tile_x = (int)(vPos.x / m_autoTileMap.Tileset.TileWorldWidth);
                    int tile_y = (int)(-vPos.y / m_autoTileMap.Tileset.TileWorldHeight);

                    bool isMouseTileChanged = (tile_x != m_prevMouseTileX) || (tile_y != m_prevMouseTileY);

                    if (m_isMouseRightDown || m_isMouseLeftDown)
                    {
                        m_startDragTileX = tile_x;
                        m_startDragTileY = tile_y;

                        m_tilesetSelStart = m_tilesetSelEnd = -1;

                        m_dragTileX = tile_x;
                        m_dragTileY = tile_y;
                    }

                    if (isMouseTileChanged)
                    {
                        m_dragTileX = tile_x;
                        m_dragTileY = tile_y;

                        if (onDragAndTileChanged != null) onDragAndTileChanged(tile_x, tile_y);
                    }
                    m_prevMouseTileX = tile_x;
                    m_prevMouseTileY = tile_y;
                }
                else
                {
                    if (m_dragTileX != -1 && m_dragTileY != -1)
                    {
                        if (onDragFinish != null) onDragFinish();

                         m_dragTileX = m_dragTileY = -1;
                    }
                }
            }

            // Draw selection rect
            if ((m_isMouseRight || m_isMouseLeft) && isDrawDragSelect)
            {
                float rX = m_autoTileMap.transform.position.x + Mathf.Min(m_startDragTileX, m_dragTileX) * m_autoTileMap.Tileset.TileWorldWidth;
                float rY = m_autoTileMap.transform.position.y + Mathf.Min(m_startDragTileY, m_dragTileY) * m_autoTileMap.Tileset.TileWorldHeight;
                float rWidth = (Mathf.Abs(m_dragTileX - m_startDragTileX) + 1) * m_autoTileMap.Tileset.TileWorldWidth;
                float rHeight = (Mathf.Abs(m_dragTileY - m_startDragTileY) + 1) * m_autoTileMap.Tileset.TileWorldHeight;
                Rect rSelection = new Rect(rX, -rY, rWidth, -rHeight);
                UtilsGuiDrawing.DrawRectWithOutline(rSelection, new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 1f));
            }
        }
    }

    public void OnSceneGUI()
    {
        DoToolBar();

        if (m_tabType == TabType.Edit)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            EventType currentEventType = Event.current.GetTypeForControl(controlID);
            bool skip = false;
            int saveControl = GUIUtility.hotControl;

            if (currentEventType == EventType.Layout) { skip = true; }
            else if (currentEventType == EventType.ScrollWheel) { skip = true; }

            if (!skip)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), MouseCursor.Arrow);
                GUIUtility.hotControl = controlID;

                //! 드레그가 종료되는 순간.
                Action onDragFinish = () =>
                {
                    //int startX = Mathf.Min(m_startDragTileX, m_dragTileX);
                    //int startY = Mathf.Min(m_startDragTileY, m_dragTileY);
                    //
                    //int endX = Mathf.Max(m_startDragTileX, m_dragTileX);
                    //int endY = Mathf.Max(m_startDragTileY, m_dragTileY);
                    //
                    //for (int x = startX; x <= endX; ++x)
                    //{
                    //    for (int y = startY; y <= endY; ++y)
                    //    {
                    //        if (m_autoTileMap.IsValidAutoTilePos(x, y))
                    //        {
                    //            
                    //        }
                    //    }
                    //}
                };

                //! 드레그중 타일위치가 바뀐경우
                Action<int,int> onDragAndTileChanged = (x,y) =>
                {
                    Vector2 pos = new Vector2(x, y);

                    if (m_autoTileMap.IsValidAutoTilePos(x, y) == false) return;

                    if (Event.current.button == 0)
                    {
                        switch (m_editType)
                        {
                            case EditType.AttackPoint: SetAttackPoint(pos); break;
                            case EditType.StartPoint: RemoveUserPoint(m_userIdx, pos); break;
                        }
                    }
                    else if (Event.current.button == 1)
                    {
                        switch (m_editType)
                        {
                            case EditType.AttackPoint:SetAttackPoint(pos);break;
                            case EditType.StartPoint: AddUserPoint(m_userIdx, pos); break;
                        }       
                    }
                };

                UpdateSelectToMouse(onDragAndTileChanged, onDragFinish, false);

                if (currentEventType == EventType.MouseDrag && Event.current.button < 2) // 2 is for central mouse button
                {
                    Event.current.Use();
                }
            }

            GUIUtility.hotControl = saveControl;

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        //! StartPoint        
        for(int j = 0; j< m_mapData.m_map.m_makeUnitPositions.Count;++j)
        {
            StartingPoint startingPoint = m_mapData.m_map.m_makeUnitPositions[j];
            if (startingPoint.m_visible == false) continue;
            for (int i = 0; i < startingPoint.m_positions.Count; ++i)
            {
                Vector2 position = startingPoint.m_positions[i];
                Color alphaColor = startingPoint.m_color;
                alphaColor.a = 0.2f;
                DrawTileWithOutline((int)position.x, (int)position.y, alphaColor, startingPoint.m_color);
            }
        }

        //! AttackPoint
        DrawTileWithOutline((int)m_mapData.m_map.m_attackPoint.x, (int)m_mapData.m_map.m_attackPoint.y, Color.red, Color.red);
    }

    void DrawTileWithOutline(int x, int y, Color color, Color lineColor)
    {
        float rX = m_autoTileMap.transform.position.x + x * m_autoTileMap.Tileset.TileWorldWidth;
        float rY = m_autoTileMap.transform.position.y + y * m_autoTileMap.Tileset.TileWorldHeight;
        float rWidth = m_autoTileMap.Tileset.TileWorldWidth;
        float rHeight = m_autoTileMap.Tileset.TileWorldHeight;
        Rect rSelection = new Rect(rX, -rY, rWidth, -rHeight);

        UtilsGuiDrawing.DrawRectWithOutline(rSelection, color, lineColor);
    }

    private GUIStyle m_toolbarBoxStyle;
    static Color s_toolbarBoxBgColor = new Color(0f, 0f, .4f, 0.4f);
    static Color s_toolbarBoxOutlineColor = new Color(.25f, .25f, 1f, 0.70f);
    bool DoToolBar()
    {
        bool isMouseInsideToolbar = false;
        if (m_toolbarBoxStyle == null)
        {
            m_toolbarBoxStyle = new GUIStyle();
            m_toolbarBoxStyle.normal.textColor = Color.white;
            m_toolbarBoxStyle.richText = true;
        }

        GUIContent brushCoords = new GUIContent("<b> "+ Event.current.type +" </b>");
        GUIContent selectedTileId = new GUIContent("<b> 뭐적지? </b>");

        Rect rTools = new Rect(4f, 4f, Mathf.Max(m_toolbarBoxStyle.CalcSize(brushCoords).x, m_toolbarBoxStyle.CalcSize(selectedTileId).x) + 4f, 44f);

        Handles.BeginGUI();
        GUILayout.BeginArea(rTools);
        HandlesEx.DrawRectWithOutline(new Rect(0, 0, rTools.size.x, rTools.size.y), s_toolbarBoxBgColor, s_toolbarBoxOutlineColor);

        GUILayout.Space(2f);
        GUILayout.Label(brushCoords, m_toolbarBoxStyle);
        GUILayout.Label(selectedTileId, m_toolbarBoxStyle);
        GUILayout.Label("<b> 툴어렵 </b>", m_toolbarBoxStyle);
        GUILayout.EndArea();

        Handles.EndGUI();
        return isMouseInsideToolbar;
    }


    bool m_isMouseLeft;
    bool m_isMouseRight;
    //bool m_isMouseMiddle;
    bool m_isMouseLeftDown;
    bool m_isMouseRightDown;
    //bool m_isMouseMiddleDown;

    void UpdateMouseInputs()
    {
        m_isMouseLeftDown = false;
        m_isMouseRightDown = false;
        //m_isMouseMiddleDown = false;

        if (Event.current.isMouse)
        {
            m_isMouseLeftDown = (Event.current.type == EventType.MouseDown && Event.current.button == 0);
            m_isMouseRightDown = (Event.current.type == EventType.MouseDown && Event.current.button == 1);
            //m_isMouseMiddleDown = ( Event.current.type == EventType.MouseDown && Event.current.button == 1);
            m_isMouseLeft = m_isMouseLeftDown || (Event.current.type == EventType.MouseDrag && Event.current.button == 0);
            m_isMouseRight = m_isMouseRightDown || (Event.current.type == EventType.MouseDrag && Event.current.button == 1);
            //m_isMouseMiddle = m_isMouseMiddleDown || ( Event.current.type == EventType.MouseDrag && Event.current.button == 2);
        }
    }

}
