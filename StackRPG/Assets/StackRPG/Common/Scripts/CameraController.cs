using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public enum CameraType
{
    FreeMove,
    FocusTarget,
    FocusGroup,
    FollowTarget,
    FollowGroup,
    MapTure,
}

[Serializable]
public class CameraModeClass
{
    public CameraType m_type { get; protected set; }
}

[Serializable]
public class FreeMove : CameraModeClass
{
    public FreeMove() { m_type = CameraType.FreeMove; }
}

[Serializable]
public class FocusTarget : CameraModeClass
{
    public Vector2 m_target;
    public float m_zoom;
    public float m_speed; //! -1 일 경우 즉시이동
    public UnityAction m_onfinish;

    public FocusTarget() { m_type = CameraType.FocusTarget; }
    public FocusTarget(FocusGroup focusGroup) : this()
    {
        m_target = focusGroup.m_target[0];
        m_zoom = focusGroup.m_zoom;
        m_speed = focusGroup.m_speed;
        m_onfinish = focusGroup.m_onfinish;
    }
}

[Serializable]
public class FocusGroup : CameraModeClass
{
    public List<Vector2> m_target;
    public float m_zoom;
    public float m_speed; //! -1 일 경우 즉시이동
    public UnityAction m_onfinish;

    public FocusGroup() { m_type = CameraType.FocusGroup; }
}

[Serializable]
public class FollowTarget : CameraModeClass
{
    public Transform m_target;
    public float m_zoom;
    public float m_speed; //! -1 일 경우 즉시이동
    public UnityAction m_onfinish;

    public FollowTarget() { m_type = CameraType.FollowTarget; }    
    public FollowTarget(FollowGroup followGroup) : this()
    {
        m_target = followGroup.m_target[0];
        m_zoom = followGroup.m_zoom;
        m_speed = followGroup.m_speed;
        m_onfinish = followGroup.m_onfinish;
    }
}

[Serializable]
public class FollowGroup : CameraModeClass
{
    public List<Transform> m_target;
    public float m_zoom;
    public float m_speed; //! -1 일 경우 즉시이동
    public UnityAction m_onfinish;

    public FollowGroup() { m_type = CameraType.FollowGroup; }
}



/// <summary>
/// MapRect, Camera, Tartget 들을 잘 보이도록 조정한다.
/// </summary>

public class CameraController : MonoBehaviour
{
    private Vector3 velocity;

    public Camera m_camera;
    public CameraModeClass m_cameraModeClass { get; private set; }

    public float DampTime = 0.15f;        
    private Vector3 m_center = new Vector3(0.5f, 0.5f, 0);

    public Rect m_mapRect;
    public float m_mapTop;
    public float m_mapCenter;
    public float m_mapBottom;

    /// <summary>
    /// 더 확대 할 수 있지만 이 이상은 할수있어도 안하는게 좋다.(확대를 하지 않으면 맵밖을 벗어나는 경우는 한다.)
    /// </summary>
    public float m_baseMaxZoom = 2.0f;
    
    /// <summary>
    /// 더 축소 할 수 있지만 이 이하는 할수있어도 안하는게 좋다.(의미가 별로 없지만, 맵이 너무커서 너무 작게 보일경우 특정 크기를 확정시켜주기위해 필요할수도있다.)
    /// </summary>
    public float m_baseMinZoom = 1.0f;

    public float m_maxZoom = 2.0f;
    public float m_minZoom = 1.0f;

    private float PixelToUnits = 100;
    public float Zoom = 2;

    private float m_screenWidht;
    private float m_screenHeight;

    public bool m_isFinishInit = false;

    void Awake()
    {
        m_isFinishInit = false;
    }

    public void Init(float screenWidth, float screenHeight, Rect mapRect, float pixelToUnits)
    {   
        m_mapRect = mapRect;
        PixelToUnits = pixelToUnits;
        m_screenWidht = screenWidth;
        m_screenHeight = screenHeight;

        m_maxZoom = Mathf.Max(m_baseMaxZoom, Mathf.Max(m_screenWidht / (PixelToUnits * m_mapRect.width), m_screenHeight / (PixelToUnits * m_mapRect.height)));
        m_minZoom = Mathf.Max(m_baseMinZoom, Mathf.Max(m_screenWidht / (PixelToUnits * m_mapRect.width), m_screenHeight / (PixelToUnits * m_mapRect.height)));

        m_isFinishInit = true;
    }
    

    public void OnChangeCameraType(CameraModeClass camearaMode)
    {
        if (m_cameraModeClass == camearaMode) return;
        m_cameraModeClass = camearaMode;

        switch (m_cameraModeClass.m_type)
        {
            case CameraType.FreeMove:

                break;
            case CameraType.FocusTarget:

                break;
            case CameraType.FollowTarget:

                break;
            case CameraType.FollowGroup:                
                
                break;
            case CameraType.MapTure:
                
                break;
        }
    }

    void LateUpdate()
    {
        if (m_isFinishInit == false) return;
        m_camera.orthographicSize = (m_screenHeight) / (2f * Zoom * PixelToUnits);
    }

    void Update()
    {
        if (m_cameraModeClass != null)
        {
            switch (m_cameraModeClass.m_type)
            {
                case CameraType.FreeMove:
                    UpdateFree();
                    break;
                case CameraType.FocusTarget:
                    UpdateFocusTarget();
                    break;
                case CameraType.FocusGroup:
                    UpdateFocusGroup();
                    break;
                case CameraType.FollowTarget:
                    UpdateFollowTarget();
                    break;
                case CameraType.FollowGroup:
                    UpdateFollowGroup();
                    break;
                case CameraType.MapTure:

                    break;
            }
        }

        DoKeepInsideMapBounds();
    }

    void UpdateFree()
    {
        MapMove();
        PinchZoom();
    }


    private Rect GetRectForPositions(List<Vector2> positions, float margin = 0)
    {
        float minX = 0;
        float minY = 0;
        float maxX = 0;
        float maxY = 0;
        for (int i = 0;i<positions.Count;++i)
        {
            Vector2 pos = positions[i];
            if (i == 0) { minX = pos.x; minY = pos.y; maxX = pos.x; maxY = pos.y; }
            else
            {
                if (minX > pos.x) minX = pos.x;
                if (minY > pos.y) minY = pos.y;
                if (maxX < pos.x) maxX = pos.x;
                if (maxY < pos.y) maxY = pos.y;
            }
        }
        return Rect.MinMaxRect(minX - margin, minY - margin, maxX + margin, maxY + margin);
    }

    private Rect GetRectForPositions(List<Transform> trans, float margin = 0)
    {
        float minX = 0;
        float minY = 0;
        float maxX = 0;
        float maxY = 0;
        for (int i = 0; i < trans.Count; ++i)
        {
            Vector3 pos = trans[i].position;
            if (i == 0) { minX = pos.x; minY = pos.y; maxX = pos.x; maxY = pos.y; }
            else
            {
                if (minX > pos.x) minX = pos.x;
                if (minY > pos.y) minY = pos.y;
                if (maxX < pos.x) maxX = pos.x;
                if (maxY < pos.y) maxY = pos.y;
            }
        }
        return Rect.MinMaxRect(minX - margin, minY - margin, maxX + margin, maxY + margin);
    }

    void UpdateFocusGroup()
    {
        if (m_cameraModeClass == null || m_cameraModeClass.m_type != CameraType.FocusGroup) return;

        FocusGroup followGroup = (FocusGroup)m_cameraModeClass;

        if (followGroup.m_target.Count == 0) return;
        if (followGroup.m_target.Count == 1) { OnChangeCameraType(new FocusTarget(followGroup)); return; }

        Rect rect = GetRectForPositions(followGroup.m_target, 1);

        Vector3 target = rect.center;
        Vector3 point = m_camera.WorldToViewportPoint(target);
        point.x = m_center.x;
        point.y = m_center.y;

        float zoom = followGroup.m_zoom;
        float speed = followGroup.m_speed;
        if (speed == -1)
        {
            SetRectAndZoom(rect, zoom);
        }
        else
        {
            Vector3 delta = target - m_camera.ViewportToWorldPoint(point);
            Vector3 destination = m_camera.transform.position + delta;
            Vector3 position = Vector3.SmoothDamp(m_camera.transform.position, destination, ref velocity, DampTime);

            rect.center = position;
            SetRectAndZoom(rect, zoom);
        }
    }
    void UpdateFollowGroup()
    {
        if (m_cameraModeClass == null || m_cameraModeClass.m_type != CameraType.FollowGroup) return;

        FollowGroup followGroup = (FollowGroup)m_cameraModeClass;

        for(int i= 0;i< followGroup.m_target.Count; ++i)
        {
            if (followGroup.m_target[i] == null)
            {
                followGroup.m_target.RemoveAt(i);
                --i;
            }
        }

        if (followGroup.m_target.Count == 0) return; 
        if (followGroup.m_target.Count == 1) { OnChangeCameraType(new FollowTarget(followGroup)); return; }

        Rect rect = GetRectForPositions(followGroup.m_target, 1);

        Vector3 target = rect.center;
        Vector3 point = m_camera.WorldToViewportPoint(target);
        point.x = m_center.x;
        point.y = m_center.y;

        float zoom = followGroup.m_zoom;
        float speed = followGroup.m_speed;
        if (speed == -1)
        {
            SetRectAndZoom(rect, zoom);
        }
        else
        {
            Vector3 delta = target - m_camera.ViewportToWorldPoint(point);
            Vector3 destination = m_camera.transform.position + delta;
            Vector3 position = Vector3.SmoothDamp(m_camera.transform.position, destination, ref velocity, DampTime);

            rect.center = position;
            SetRectAndZoom(rect, zoom);
        }
    }

    void UpdateFollowTarget()
    {
        if (m_cameraModeClass == null || m_cameraModeClass.m_type != CameraType.FollowTarget) return;

        FollowTarget followTarget = (FollowTarget)m_cameraModeClass;

        if(followTarget.m_target == null) { m_cameraModeClass = null; return; }

        Vector3 target = followTarget.m_target.position;
        Vector3 point = m_camera.WorldToViewportPoint(target);
        point.x = m_center.x;
        point.y = m_center.y;

        float zoom = followTarget.m_zoom;
        float speed = followTarget.m_speed;
        if (speed == -1)
        {   
            SetPositionAndZoom(target, zoom);
        }
        else
        {
            Vector3 delta = target - m_camera.ViewportToWorldPoint(point);
            Vector3 destination = m_camera.transform.position + delta;
            Vector3 position = Vector3.SmoothDamp(m_camera.transform.position, destination, ref velocity, DampTime);

            SetPositionAndZoom(position, zoom);
        }
    }

    void UpdateFocusTarget()
    {
        if (m_cameraModeClass == null || m_cameraModeClass.m_type != CameraType.FocusTarget) return;

        FocusTarget focusTarget = (FocusTarget)m_cameraModeClass;

        Vector3 target = focusTarget.m_target;
        Vector3 point = m_camera.WorldToViewportPoint(target);
        point.x = m_center.x;
        point.y = m_center.y;

        float zoom = focusTarget.m_zoom;
        float speed = focusTarget.m_speed;
        if (speed == -1)
        {
            target.z = point.z;
            SetPositionAndZoom(target, zoom);
        }
        else
        {
            Vector3 delta = target - m_camera.ViewportToWorldPoint(point);
            Vector3 destination = m_camera.transform.position + delta;
            Vector3 position = Vector3.SmoothDamp(m_camera.transform.position, destination, ref velocity, DampTime);

            SetPositionAndZoom(position, zoom);
        }
    }

    void SetPositionAndZoom(Vector3 position, float zoom)
    {
        SetCameraPosition(position);        
        if (m_mapRect.Contains(position) == false) { Debug.LogWarning("out of MapBound position"); }
        else
        {   
            //! 지도안에 카메라가 있게하기위해 줌을 활용한다.
            SetZoomInsideBounds(m_mapRect, GetCameraRect(zoom), ref zoom);

            SetCameraZoom(zoom);
        }
    }

    void SetRectAndZoom(Rect rect, float zoom)
    {   
        Vector3 position = rect.center;

        SetCameraPosition(position);
        if (m_mapRect.Contains(position) == false) { Debug.LogWarning("out of MapBound position");}
        else
        {
            Rect cameraRect = GetCameraRect(zoom);
            //! 카메라안에 지역이 가운데 들어가게 하기위해 줌을 활용한다.
            if (IsInsideRect(cameraRect, rect) == false) zoom *= Mathf.Min(cameraRect.width / rect.width, cameraRect.height / rect.height);

            //! 지도안에 카메라가 있게하기위해 줌을 활용한다.
            SetZoomInsideBounds(m_mapRect, GetCameraRect(zoom), ref zoom);

            SetCameraZoom(zoom);
        }
    }

    private Rect GetCameraRect(float zoom)
    {
        Rect cameraRect = new Rect();
        cameraRect.width = m_screenWidht / (PixelToUnits * zoom);
        cameraRect.height = m_screenHeight / (PixelToUnits * zoom);
        cameraRect.center = m_camera.transform.position;
        return cameraRect;
    }

    const float EPSILON = 0.0001f;
    bool IsInsideRect(Rect baseRect, Rect setRect)
    {   
        if (baseRect.xMin > setRect.xMin && baseRect.xMin - setRect.xMin > EPSILON) { return false; }
        if (baseRect.yMin > setRect.yMin && baseRect.yMin - setRect.yMin > EPSILON) { return false; }
        if (baseRect.xMax < setRect.xMax && setRect.xMax - baseRect.xMax > EPSILON) { return false; }
        if (baseRect.yMax < setRect.yMax && setRect.yMax - baseRect.yMax > EPSILON) { return false; }
        return true;
    }

    void SetZoomInsideBounds(Rect baseRect, Rect setRect, ref float zoom)
    {
        if (baseRect.xMin > setRect.xMin && baseRect.xMin - setRect.xMin > EPSILON) { zoom = Mathf.Max(zoom, GetZoomHalfWidth(setRect.center.x - baseRect.xMin)); }
        if (baseRect.yMin > setRect.yMin && baseRect.yMin - setRect.yMin > EPSILON) { zoom = Mathf.Max(zoom, GetZoomHalfHeight(setRect.center.y - baseRect.yMin)); }
        if (baseRect.xMax < setRect.xMax && setRect.xMax - baseRect.xMax > EPSILON) { zoom = Mathf.Max(zoom, GetZoomHalfWidth(baseRect.xMax - setRect.center.x)); }
        if (baseRect.yMax < setRect.yMax && setRect.yMax - baseRect.yMax > EPSILON) { zoom = Mathf.Max(zoom, GetZoomHalfHeight(baseRect.yMax - setRect.center.y)); }        
    }

    float GetZoomHalfWidth(float halfWidth)
    {
        return m_screenWidht / (PixelToUnits * halfWidth * 2);
    }

    float GetZoomHalfHeight(float halfHeight)
    {
        return m_screenHeight / (PixelToUnits * halfHeight * 2);
    }

    void DoKeepInsideMapBounds()
    {
        Rect rCamera = new Rect();
        rCamera.width = m_screenWidht / (PixelToUnits * Zoom);
        rCamera.height = m_screenHeight / (PixelToUnits * Zoom);
        rCamera.center = m_camera.transform.position;


        Vector3 vOffset = Vector3.zero;
        float right = (rCamera.x < m_mapRect.x) ? m_mapRect.x - rCamera.x : 0f;
        float left = (rCamera.xMax > m_mapRect.xMax) ? m_mapRect.xMax - rCamera.xMax : 0f;
        float down = (rCamera.y < m_mapRect.y) ? m_mapRect.y - rCamera.y : 0f;
        float up = (rCamera.yMax > m_mapRect.yMax) ? m_mapRect.yMax - rCamera.yMax : 0f;

        vOffset.x = (right != 0f && left != 0f) ? m_mapRect.center.x - m_camera.transform.position.x : right + left;
        vOffset.y = (down != 0f && up != 0f) ? m_mapRect.center.y - m_camera.transform.position.y : up + down;

        m_camera.transform.position += vOffset;
    }

    public void FullScreenAndCenter()
    {   
        SetCameraZoom(m_minZoom);
        SetCameraPosition(new Vector3(m_mapRect.width * 0.5f, -m_mapRect.height * 0.5f, m_camera.transform.position.z));
    }

    public IEnumerator MapTour()
    {
        //! 가장 윗쪽 중심에서 가장 작게 보이게 한다음, 아래로 끝까지 내린다.
        FullScreenAndCenter();
        yield break;
        //float topY = m_mapTop;
        //float bottomY = m_mapBottom;
        ////m_camera.transform.position += new Vector3(0, topY, 0);
        //
        //
        //float time = 0;
        //float duration = 1;
        //Vector3 position = m_camera.transform.position;
        //
        //while (time < duration)
        //{
        //    yield return null;
        //    position.y = m_mapCenter + Mathf.Lerp(0, topY, time / duration);
        //    m_camera.transform.position = position;
        //    time += Time.deltaTime;
        //}
        //
        //time = 0;
        //duration = 2;
        //while (time < duration)
        //{
        //    yield return null;
        //    position.y = m_mapCenter + Mathf.Lerp(topY, bottomY, time / duration);
        //    m_camera.transform.position = position;
        //    time += Time.deltaTime;
        //}
    }
    public bool m_isTouching = false;

    public Vector2 m_dragPoint_1;
    public Vector2 m_dragPoint_2;
    void MapMove()
    {
        //! 유저가 생성할때,
        if (Input.GetMouseButtonDown(0))
        {
            m_isTouching = true;
            m_dragPoint_1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && m_isTouching == true)
        {
            m_dragPoint_2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (m_dragPoint_1 != m_dragPoint_2)
            {
                m_camera.transform.Translate(m_dragPoint_1 - m_dragPoint_2);
                m_dragPoint_1 = m_dragPoint_2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_isTouching = false;
        }
    }
    void PinchZoom()
    {
#if UNITY_EDITOR
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        Zoom += wheel;
#elif UNITY_ANDROID
            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                Zoom -= deltaMagnitudeDiff * Time.deltaTime;
            }
#endif

        //! 넘어가지 않게한다.
        if (Zoom > m_maxZoom) Zoom = m_maxZoom;
        if (Zoom < m_minZoom) Zoom = m_minZoom;
    }

    void SetCameraPosition(Vector3 position)
    {
        position.z = m_camera.transform.position.z;
        m_camera.transform.position = position;
    }

    void SetCameraZoom(float zoom)
    {
        //if (zoom < m_minZoom) zoom = m_minZoom;
        //if (zoom > m_maxZoom) zoom = m_maxZoom;
        Zoom = zoom;

    }

    public void SetPivot(Vector2 pivot)
    {
        m_center = pivot;
    }

    public void SetGrayScale(float delay, float grayScale)
    {
        SetMotionBlur();

        Grayscale grayscale  = m_camera.GetComponent<Grayscale>();
        //grayscale.shader = Shader.Find("GrayscaleEffect");
        grayscale.rampOffset = 0;
        grayscale.enabled = true;
    }

    public void SetMotionBlur()
    {
        MotionBlur motionBlur = m_camera.GetComponent<MotionBlur>();
        //motionBlur.shader = Shader.Find("MotionBlur");
        motionBlur.blurAmount = 0.8f;
        motionBlur.extraBlur = true;
        motionBlur.enabled = true;
    }

    //void OnDrawGizmos()
    //{   
    //    Gizmos.color = new Color(0, 1, 0, 0.5f);
    //    MSettings.GizmoDrawRect(testCamera);
    //    Gizmos.color = new Color(0, 0, 1, 0.5f);
    //    MSettings.GizmoDrawRect(testRect);
    //}
}
