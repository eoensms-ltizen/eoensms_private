using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CreativeSpore;
using CreativeSpore.RpgMapEditor;
using UnityEngine.Events;
using stackRPG;

public enum CameraMode
{
    None,
    Normal,
    Make,
    Fight,
}


public class MGameCamera : MonoBehaviour
{
    public static MGameCamera Instance { get; private set; }

    public CameraController m_cameraController;

    public Camera m_camera { get { return m_cameraController.m_camera; } }

    public float PixelToUnits = 100f;

    public static Vector2 m_pivotCenter = new Vector2(0.5f, 0.5f);
    public static Vector2 m_pivotLeft = new Vector2(0.25f, 0.5f);

    void Awake()
    {   
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(transform.gameObject);
    }

    //! 맵이 바뀔때마다, 셋팅하는것
    public void InitMapData()
    {
        Rect mapRect = new Rect();
        mapRect.width = CreativeSpore.RpgMapEditor.AutoTileMap.Instance.MapTileWidth * CreativeSpore.RpgMapEditor.AutoTileMap.Instance.Tileset.TileWorldWidth;
        mapRect.height = CreativeSpore.RpgMapEditor.AutoTileMap.Instance.MapTileHeight * CreativeSpore.RpgMapEditor.AutoTileMap.Instance.Tileset.TileWorldHeight;
        mapRect.x = CreativeSpore.RpgMapEditor.AutoTileMap.Instance.transform.position.x;
        mapRect.y = CreativeSpore.RpgMapEditor.AutoTileMap.Instance.transform.position.y;
        mapRect.y -= mapRect.height;

        m_cameraController.Init(MApplicationManager.width, MApplicationManager.height, mapRect, PixelToUnits);
        m_cameraController.SetPivot(new Vector2(0.5f,0.5f));
    }


    public void FullScreenAndCenter()
    {
        m_cameraController.FullScreenAndCenter();
    }

    public IEnumerator MapTour()
    {
        yield return StartCoroutine(m_cameraController.MapTour());
    }

    public void SetPlayCamera()
    {
        List<Transform> targets = new List<Transform>();
        for(int i = 0;i<MGameManager.Instance.m_userList.Count;++i)
        {
            MUser user = MGameManager.Instance.m_userList[i];
            for(int j = 0;j< user.m_aliveUnits.Count;++j)
            {
                targets.Add(user.m_aliveUnits[j].transform);
            }
        }

        SetFollowGroup(targets);
    }

    public void SetFollowGroup(List<Transform> targets)
    {
        FollowGroup followGroup = new FollowGroup();
        followGroup.m_target = targets;
        followGroup.m_speed = 1;
        followGroup.m_zoom = 2;
        followGroup.m_onfinish = () => { Debug.Log("FollowGroup 완료"); };

        ChangeCameraType(followGroup);
    }

    public void GameOver()
    {
        m_cameraController.SetGrayScale(1, 0.5f);
    }

    public void Finish()
    {
        m_cameraController.SetMotionBlur();
    }

    public void OnFocusUserStartingPosition(MUser user)
    {
        //! 유저의 스타팅포인트가 포커싱이 되도록한다.
        FocusGroup focusGroup = new FocusGroup();
        focusGroup.m_target = GetTileCenterPositions(user.m_startingPosition.m_positions);
        focusGroup.m_speed = 1;
        focusGroup.m_zoom = 2;
        focusGroup.m_onfinish = () => { Debug.Log(user.m_nickName + " 포커싱 완료"); };
        ChangeCameraType(focusGroup);
    }

    public List<Vector2> GetTileCenterPositions(List<Vector2> tilePositions)
    {
        List<Vector2> result = new List<Vector2>();
        for(int i = 0;i<tilePositions.Count;++i)
        {
            result.Add(RpgMapHelper.GetTileCenterPosition((int)tilePositions[i].x, (int)tilePositions[i].y));
        }
        return result;
    }

    public void SetPivot(Vector2 pivot)
    {
        m_cameraController.SetPivot(pivot);
    }

    public void SetTarget(Vector3 position)
    {
        FocusTarget focusTarget = new FocusTarget();
        focusTarget.m_target = position;
        focusTarget.m_speed = 1;
        focusTarget.m_zoom = 2;
        focusTarget.m_onfinish = () => { Debug.Log(" SetTarget 종료"); };

        ChangeCameraType(focusTarget);
    }

    public void SetTarget(int tileX, int tileY)
    {
        SetTarget(RpgMapHelper.GetTileCenterPosition(tileX, tileY));
    }

    private CameraModeClass m_lastCameraModeClass;
    bool m_isForceFreeMove;
    public void SetFreeMove(bool value)
    {
        if (m_isForceFreeMove == value) return;
        
        if (value == true)
        {
            m_lastCameraModeClass = m_cameraController.m_cameraModeClass;
            ChangeCameraType(new FreeMove());
            m_isForceFreeMove = true;
        }
        else
        {
            m_isForceFreeMove = false;
            ChangeCameraType(m_lastCameraModeClass);
        }
    }

    private void ChangeCameraType(CameraModeClass cameraModeClass)
    {
        if (m_isForceFreeMove == true) { m_lastCameraModeClass = cameraModeClass; }
        else { m_cameraController.OnChangeCameraType(cameraModeClass); }
    }

    private void GetCenterPosition(List<Vector2> positions, ref Vector3 position)
    {
        Vector2 pos = Vector2.zero;

        for (int i = 0; i < positions.Count; ++i)
        {
            pos += positions[i];
        }

        if (positions.Count != 0) position = pos / positions.Count;
    }    
}