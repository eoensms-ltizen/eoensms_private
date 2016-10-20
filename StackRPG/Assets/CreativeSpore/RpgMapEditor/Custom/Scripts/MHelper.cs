using UnityEngine;
using System.Collections;
using CreativeSpore.RpgMapEditor;
using CreativeSpore;

public class MHelper : Singleton<MHelper>
{
    public AutoTileMap m_autoTileMap;
    public Camera2DController m_camera2D;

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
}
