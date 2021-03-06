﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MSettings
{   
    public static void Log(string text)
    {   
        Debug.Log(text);
    }

    public static void LogWarning(string text)
    {
        Debug.LogWarning(text);
    }

    public static void LogError(string text)
    {   
        Debug.LogError(text);
    }

    public static void GizmoDrawRectByPoint(Vector2 pos1, Vector2 pos2)
    {
        Vector2 pos3 = new Vector2(pos1.x, pos2.y);
        Vector2 pos4 = new Vector2(pos2.x, pos1.y);

        Gizmos.DrawLine(pos1, pos3);
        Gizmos.DrawLine(pos1, pos4);
        Gizmos.DrawLine(pos2, pos3);
        Gizmos.DrawLine(pos2, pos4);
    }

    public static void GizmoDrawRect(Rect rect)
    {
        Vector3 point1;
        Vector3 point2;
        GetSidePointByRect(rect, out point1, out point2);
        GizmoDrawRectByPoint(point1, point2);
    }

    public static void GetSidePointByRect(Rect rect, out Vector3 point1, out Vector3 point2)
    {
        point1 = new Vector3(rect.xMin, rect.yMin);
        point2 = new Vector3(rect.xMax, rect.yMax);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;        
        while (n > 1)
        {
            int k = (Random(0, n) % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


    public class CompareVectorVertical : IComparer<Vector2>
    {
        // decending order by vector y value
        public int Compare(Vector2 a, Vector2 b)
        {
            if (a.y > b.y) return 1;            // if wanna change to ascending order, change compare option, greater than to less than
            else if (a.y < b.y) return -1;   // if wanna change to ascending order, change compare option, less than to greater than 
            else
            {
                if (a.x > b.x) return 1;
                else if (a.x < b.x) return -1;
                return 0;
            }
        }
    }

    public class CompareVectorHorizonal : IComparer<Vector2>
    {
        // decending order by vector y value
        public int Compare(Vector2 a, Vector2 b)
        {
            if (a.x > b.x) return 1;            // if wanna change to ascending order, change compare option, greater than to less than
            else if (a.x < b.x) return -1;   // if wanna change to ascending order, change compare option, less than to greater than 
            {
                if (a.y > b.y) return 1;            
                else if (a.y < b.y) return -1;
                return 0;
            }
        }
    }

    public static void LookAt2D(Transform from, Transform target, int digree = 0)
    {
        LookAt2D(from, target.position, digree);
    }

    public static void LookAt2D(Transform from, Vector3 target, int digree = 0)
    {
        Vector3 diff = target - from.position;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        from.rotation = Quaternion.Euler(0f, 0f, rot_z - digree);
    }

    public static int Random(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static float Random(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    private static void SetBoolPlayerPrefs(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    private static bool GetBoolPlayerPrefs(string key, bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 0 ? false : true;
    }

    //! PlayerPrefs key
    public const string PP_OWNER_COLOR = "OWNER_COLOR";
    public const string PP_OWNER_NICKNAME = "OWNER_NICKNAME";
    public const string PP_SHOW_MINIMAP = "SHOW_MINIMAP";
    public const string PP_PARTICIPANTS_COUNT = "PARTICIPANTS_COUNT";
    public const string PP_STAT_ = "STAT_";
    public const string PP_STAT_POINT = "STAT_POINT";

    public static int m_eatSoundIndex = 1;
    public static float m_eatSoundPitchValue = 0.025f;

    public enum StatType
    {
        AttackSpeed,
        AttackTime,
        DefenceSpeed,
        DefenceTime,
        Strength,
    }

    public static int GetStat(StatType statType)
    {
        return PlayerPrefs.GetInt(string.Format("{0}{1}", PP_STAT_, statType), 0);
    }
    public static void SetStat(StatType statType, int point)
    {
        PlayerPrefs.SetInt(string.Format("{0}{1}", PP_STAT_, statType) , point);
    }

    public static int GetStatPoint()
    {
        return PlayerPrefs.GetInt(PP_STAT_POINT, 0);
    }

    public static void SetStatPoint(int point)
    {
        PlayerPrefs.SetInt(PP_STAT_POINT, point);
    }    

    public static void SaveOwnerColor(Color color)
    {
        PlayerPrefs.SetString(MSettings.PP_OWNER_COLOR, color.ToString());
    }

    public static Color GetRandomColor()
    {
        int colorType = Random(0, 3);

        Color result = new Color(Random(0.0f, 1.0f), Random(0.0f, 1.0f), Random(0.0f, 1.0f), 1);
        result[colorType] = 1;

        return result;
    }

    public static Color GetOwnerColor()
    {
        string str_color;

        str_color = PlayerPrefs.GetString(MSettings.PP_OWNER_COLOR, GetRandomColor().ToString());
        str_color = str_color.Replace("RGBA(", "");
        str_color = str_color.Replace(")", "");

        var strings = str_color.Split(","[0]);

        Color outputcolor;
        outputcolor = Color.white;
        for (var i = 0; i < 4; i++)
        {
            outputcolor[i] = System.Single.Parse(strings[i]);
        }

        SaveOwnerColor(outputcolor);

        return outputcolor;
    }

    public static string GetRandomNickname()
    {
        return string.Format("AI_{0}", Random(1000, 10000));
    }

    public static void SetNickname(string nickname)
    {
        PlayerPrefs.SetString(PP_OWNER_NICKNAME, nickname);
    }

    public static string GetNickname()
    {
        return PlayerPrefs.GetString(PP_OWNER_NICKNAME, "");
    }


    public static void SetParticipantsCount(int count)
    {
        PlayerPrefs.SetInt(PP_PARTICIPANTS_COUNT, count);
    }

    public static int GetParticipantsCount()
    {
        return PlayerPrefs.GetInt(PP_PARTICIPANTS_COUNT, 8);
    }
    //!타입 해쉬
    public static int Square = "Square".GetHashCode();
    public static int Wall = "Wall".GetHashCode();
    public static int UISquare = "UISquare".GetHashCode();

    public const float MAX_MOVE_DISTANCE = 0.5f;

    public static int Shift_Layer_Every = -1;
    //! Wall 이 가지고있다.
    public static int Shift_Layer_Move = 1 << LayerMask.NameToLayer("Move");
    //! Square 이 가지고있다.
    public static int Shift_Layer_Square = 1 << LayerMask.NameToLayer("Square");
    //! HeadSquare 이 가지고있다.
    public static int Shift_Layer_HeadSquare = 1 << LayerMask.NameToLayer("HeadSquare");

    public static void SetShowMiniMap(bool value)
    {
        SetBoolPlayerPrefs(PP_SHOW_MINIMAP, value);
    }
    public static bool GetShowMiniMap()
    {
        return GetBoolPlayerPrefs(PP_SHOW_MINIMAP, true);
    }
}
