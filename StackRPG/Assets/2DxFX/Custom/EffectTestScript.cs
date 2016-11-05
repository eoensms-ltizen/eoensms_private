using UnityEngine;
using System.Collections;
using stackRPG;
using System;
using System.Collections.Generic;

public class EffectTestScript : MonoBehaviour {

    public MUnit unit;
    public UnitData unitData;

    int count = 0;

    public List<string> m_effectNames = new List<string>();
    
	void Start ()
    {
        List<Vector2> canMovePositions;
        GetCanMovePosition(unit.transform.position, m_effectNames.Count + 1, out canMovePositions);

        for (int i = 0; i < m_effectNames.Count; ++i)
        {
            GameObject obj = new GameObject();
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            obj.transform.localScale = unit.m_spriteRenderer.transform.lossyScale;
            spriteRenderer.sprite = unit.m_spriteRenderer.sprite;
            obj.AddComponent(Type.GetType(m_effectNames[i]));
            obj.transform.position = canMovePositions[i + 1];
            obj.name = "[" + i + "]" + m_effectNames[i];
        }

        //unit.Init(unitData.m_unitData);
    }
	
	// Update is called once per frame
	void Update ()
    {
	    //if(Input.GetKeyDown(KeyCode.Tab))
        //{
        //    List<Vector2> canMovePositions;
        //    GetCanMovePosition(unit.transform.position, m_effectNames.Count + 1, out canMovePositions);
        //
        //    for (int i = 0;i<m_effectNames.Count;++i)
        //    {
        //        GameObject obj = new GameObject();
        //        SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
        //        obj.transform.localScale = unit.m_spriteRenderer.transform.lossyScale;
        //        spriteRenderer.sprite = unit.m_spriteRenderer.sprite;
        //        obj.AddComponent(Type.GetType(m_effectNames[i]));
        //        obj.transform.position = canMovePositions[i + 1];
        //        obj.name = "[" + i + "]" + m_effectNames[i];
        //    }
        //
        //    //Destroy(unit.gameObject);
        //}
	}

    void GetCanMovePosition(Vector2 center, int count, out List<Vector2> positions)
    {
        positions = new List<Vector2>();

        int index = 0;
        while (positions.Count < count)
        {
            Vector2 pos = center;

            if (GetNextPosition(index++, ref pos) == false) continue;

            positions.Add(pos);
        }
    }

    int[] m_nearPositionCount = { 0, 1, 5, 13, 25, 41, 66, 107, 173, 280, 453 };

    bool GetNextPosition(int index, ref Vector2 pos)
    {
        int distance = -1;
        int number = -1;
        for (int i = 0; i < m_nearPositionCount.Length; i++)
        {
            if (m_nearPositionCount[i] > index)
            {
                distance = i - 1;
                number = index - m_nearPositionCount[i - 1];
                break;
            }
        }

        if (distance == -1) return false;


        int x = -distance + (number + 1) / 2;
        int y = distance - Mathf.Abs(x);
        if (number % 2 == 0) y *= -1;

        pos = new Vector2(x, y);

        return true;
    }
}
