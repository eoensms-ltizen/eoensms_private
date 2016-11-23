using UnityEngine;
using System.Collections;
using CreativeSpore.RpgMapEditor;
using CreativeSpore;
using System.Collections.Generic;
using stackRPG;

public class MHelper : Singleton<MHelper>
{
    public AutoTileMap m_autoTileMap;
    public Camera2DController m_camera2D;

    public static int[] m_nearPositionCount = { 0, 1, 5, 13, 25, 41, 66, 107, 173, 280, 453 };

    void OnEnable()
    {
        m_autoTileMap = FindObjectOfType<AutoTileMap>();
        m_camera2D = FindObjectOfType<Camera2DController>();
    }

    public float m_tileWidth { get { return m_autoTileMap.Tileset.TileWidth * m_camera2D.Zoom; } }
    public float m_tileHeight { get { return m_autoTileMap.Tileset.TileHeight * m_camera2D.Zoom; } }

    public void GetRectDrawPoint(Vector3 pos, out Vector2 pos1, out Vector2 pos2)
   {
       Vector2 center = GetTileCenterPosition(pos);  

        //!TODO: 정사각형이 아닐수도 있는 부분에 대한 대비가 필요하다.
       pos1 = center + Vector2.one * m_tileWidth * 0.5f;
       pos2 = center - Vector2.one * m_tileWidth * 0.5f;
   }
   
   public Vector2 GetTileCenterPosition(Vector3 pos)
   {
       AutoTile autoTile = RpgMapHelper.GetAutoTileByPosition(pos, 0);
       return RpgMapHelper.GetTileCenterPosition(autoTile.TileX, autoTile.TileY);
   }

    public static void GetCanMovePosition(Vector2 center, int count, out List<Vector2> positions)
    {
        positions = new List<Vector2>();
        AutoTile centerTile = RpgMapHelper.GetAutoTileByPosition(center, 0);

        int index = 0;
        while (positions.Count < count)
        {
            Vector2 pos = center;

            if (GetNextPosition(centerTile, index++, ref pos) == false) continue;
            if (IsCanMovePosition(pos) == false) continue;

            positions.Add(pos);
        }
    }

    public static bool GetNextPosition(AutoTile centerTile, int index, ref Vector2 pos)
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

        pos = RpgMapHelper.GetTileCenterPosition(x + centerTile.TileX, y + centerTile.TileY);

        return true;
    }

    public static bool IsCanMovePosition(Vector2 pos)
    {
        //! 해당위치가 가능한지 안한지는, 한타일을 9등분해서 판단한다. 젠장 즉, 옆에는 서있을수있다는거다. -_-
        if (AutoTileMap.Instance.GetAutotileCollisionAtPosition(pos) == eTileCollisionType.PASSABLE) return true;
        return false;
    }

    public static void AttackGround(List<MUnit> units, Vector3 position)
    {
        List<Vector2> canMovePositions;
        GetCanMovePosition(position, units.Count, out canMovePositions);
        for (int i = 0; i < units.Count; ++i) if(units[i]!=null) units[i].CommandAttackGround(canMovePositions[i]);
    }

    public static  void MoveGround(List<MUnit> units, Vector3 position)
    {
        List<Vector2> canMovePositions;
        GetCanMovePosition(position, units.Count, out canMovePositions);
        for (int i = 0; i < units.Count; ++i) if (units[i] != null) units[i].CommandMoveGround(canMovePositions[i]);
    }
}
