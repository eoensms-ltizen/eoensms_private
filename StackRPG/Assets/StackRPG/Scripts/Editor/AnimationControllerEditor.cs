using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEditor.Sprites;
using System.Collections.Generic;

[CustomEditor(typeof(AnimationController))]
public class AnimationControllerEditor : Editor
{
    AnimationController m_animationController { get { return (AnimationController)target; } }
    static TabType m_tabType;
    List<Texture2D> m_textures = new List<Texture2D>();
    float m_currAnimFrame;

    System.Diagnostics.Stopwatch m_stopwatch = new System.Diagnostics.Stopwatch();
    long m_lastMiliseconds = 0;

    void OnEnable()
    {
        m_stopwatch.Reset();
        m_stopwatch.Start();
    }

    void OnDisable()
    {
        m_stopwatch.Stop();
    }

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
        if(GUILayout.Button("ReloadData") == true)
        {
            ReloadData();
        }
        EditorGUILayout.HelpBox("folderPaht", MessageType.Info);
        m_animationController.m_folderPath = GUILayout.TextArea(m_animationController.m_folderPath);
        EditorGUILayout.HelpBox("Input multiple sprite fileName", MessageType.Info);
        m_animationController.m_fileName = EditorGUILayout.TextField(m_animationController.m_fileName);

        if (m_textures.Count == 0 && m_animationController.SpriteFrames.Count > 0)
        {
            ReloadSpriteFrame();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("All Load") == true)
        {
            string imagePath = m_animationController.m_folderPath + m_animationController.m_fileName + ".png";
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(imagePath).OfType<Sprite>().ToArray();

            m_animationController.m_startIndex = 0;
            m_animationController.m_endIndex = sprites.Length - 1;

            LoadSprite(sprites);
            ReloadSpriteFrame();
        }

        if (GUILayout.Button("range Load") == true)
        {
            string imagePath = m_animationController.m_folderPath + m_animationController.m_fileName + ".png";
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(imagePath).OfType<Sprite>().ToArray();

            LoadSprite(sprites);
            ReloadSpriteFrame();
        }
        m_animationController.m_startIndex = EditorGUILayout.IntField(m_animationController.m_startIndex);
        m_animationController.m_endIndex = EditorGUILayout.IntField(m_animationController.m_endIndex);

        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Animation");        
        GUILayout.BeginHorizontal();

        if (m_textures.Count > 0)
        {
            float afterAnimFrame = m_currAnimFrame + m_animationController.AnimSpeed * (m_stopwatch.ElapsedMilliseconds - m_lastMiliseconds) * 0.001f;

            m_lastMiliseconds = m_stopwatch.ElapsedMilliseconds;

            while (afterAnimFrame >= m_textures.Count)
                afterAnimFrame -= m_textures.Count;

            if (m_currAnimFrame != afterAnimFrame)
            {
                m_currAnimFrame = afterAnimFrame;
                Texture2D texture = m_textures[m_animationController.m_IsReverse ? m_textures.Count - 1 - (int)m_currAnimFrame : (int)m_currAnimFrame];


                //EditorGUI.DrawPreviewTexture(new Rect(0, 0, texture.width, texture.height), texture);

                GUIStyle tilesetStyle = new GUIStyle(GUI.skin.box);            
                tilesetStyle.normal.background = texture;
                tilesetStyle.border = tilesetStyle.margin = tilesetStyle.padding = new RectOffset(0, 0, 0, 0);
                float fWidth = texture.width;
                float fHeight = texture.height;
                GUILayout.Box("", tilesetStyle, GUILayout.Width(fWidth), GUILayout.Height(fHeight));

                EditorUtility.SetDirty(target);
            }            
        }
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("Speed");        
        m_animationController.AnimSpeed = EditorGUILayout.FloatField(m_animationController.AnimSpeed);
        EditorGUILayout.LabelField("Reverse");
        m_animationController.m_IsReverse = EditorGUILayout.Toggle(m_animationController.m_IsReverse);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    Texture2D GenerateTextureFromSprite(Sprite aSprite)
    {        
        var rect = aSprite.rect;
        var tex = new Texture2D((int)rect.width, (int)rect.height);
        var data = aSprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        tex.SetPixels(data);
        tex.Apply(true);
        return tex;
    }

    void ReloadSpriteFrame()
    {
        m_textures.Clear();
        for (int i = 0; i < m_animationController.SpriteFrames.Count; ++i)
        {
            m_textures.Add(GenerateTextureFromSprite(m_animationController.SpriteFrames[i]));
        }
    }

    void LoadSprite(Sprite[] sprites)
    {
        m_animationController.SpriteFrames.Clear();

        if (sprites == null || sprites.Length == 0) Debug.Log("Not Found multiple Sprites : " + m_animationController.m_folderPath);
        else
        {

            for (int i = m_animationController.m_startIndex; i <= m_animationController.m_endIndex; ++i)
            {
                m_animationController.SpriteFrames.Add(sprites[i]);
            }
        }
    }

    void ReloadData()
    {
        string[] temp = m_animationController.SpriteFrames[0].name.Split('_');
        m_animationController.m_fileName = temp[0].ToString();
        m_animationController.m_startIndex = int.Parse(temp[1].ToString());
        m_animationController.m_endIndex = int.Parse(m_animationController.SpriteFrames[m_animationController.SpriteFrames.Count - 1].name.Split('_')[1].ToString());
        m_animationController.m_folderPath = System.Text.RegularExpressions.Regex.Split(AssetDatabase.GetAssetPath(m_animationController.SpriteFrames[0]), m_animationController.m_fileName)[0];
    }

}
