﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CreativeSpore;
using CreativeSpore.RpgMapEditor;

namespace stackRPG
{
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
        
        public float DampTime = 0.15f;
        private Vector3 _left = new Vector3(0.25f, 0.5f, 0);
        private Vector3 _center = new Vector3(0.5f, 0.5f, 0);
        public Vector3 m_center
        {
            get { if (PlayUI.Instance.m_readyPanel.activeSelf == true) return _left; else return _center; }
        }
        public Vector3 m_target;

        private Vector3 velocity = Vector3.zero;
        public Camera m_camera { get; private set; }

        public CameraMode m_currentMode;

        public Rect m_mapRect;
        public float m_mapTop;
        public float m_mapCenter;
        public float m_mapBottom;
        
        public float m_maxZoom = 2.0f;
        public float m_minZoom = 1.0f;

        public bool m_isZoomOut = false;

        public float Zoom = 1f;
        public float PixelToUnits = 100f;
        public bool KeepInsideMapBounds = true;

        private List<MUser> m_focusUser = new List<MUser>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Init();
            }
            else if (Instance != this) Destroy(transform.gameObject);
        }

        void Init()
        {
            m_camera = MGameManager.Instance.m_camera;
            m_camera.transform.position = m_camera.transform.position;

            MGameManager.Instance.m_changeUserEvent += OnChangeUser;
            MGameManager.Instance.m_changeGameState += OnChangeGameState;
        }

        //! 맵이 바뀔때마다, 셋팅하는것
        void InitMapData()
        {
            m_mapRect = new Rect();
            m_mapRect.width = CreativeSpore.RpgMapEditor.AutoTileMap.Instance.MapTileWidth * CreativeSpore.RpgMapEditor.AutoTileMap.Instance.Tileset.TileWorldWidth;
            m_mapRect.height = CreativeSpore.RpgMapEditor.AutoTileMap.Instance.MapTileHeight * CreativeSpore.RpgMapEditor.AutoTileMap.Instance.Tileset.TileWorldHeight;
            m_mapRect.x = CreativeSpore.RpgMapEditor.AutoTileMap.Instance.transform.position.x;
            m_mapRect.y = CreativeSpore.RpgMapEditor.AutoTileMap.Instance.transform.position.y;
            m_mapRect.y -= m_mapRect.height;

            m_minZoom = Screen.width / (PixelToUnits * m_mapRect.width);

            m_mapTop = (m_mapRect.height - Screen.height / (PixelToUnits * Zoom)) * 0.5f;
            m_mapCenter = -m_mapRect.height * 0.5f;
            m_mapBottom = (m_mapRect.height - Screen.height / (PixelToUnits * Zoom)) * -0.5f;
        }

        private void CameraSize(out float width, out float height)
        {
            height = 2 * m_camera.orthographicSize;
            width = height * m_camera.aspect;
            if (MGameManager.Instance.m_state == GameState.WaitReady) width *= 0.5f;
        }
        public bool m_isTouching = false;

        public Vector2 m_dragPoint_1;
        public Vector2 m_dragPoint_2;

        // Update is called once per frame
        void Update()
        {
            switch(m_currentMode)
            {
                case CameraMode.None:
                    {

                    }
                    break;
                case CameraMode.Normal:
                    {
                        
                    }
                    break;
                case CameraMode.Make:
                    {
                        if(MGameManager.Instance.m_currentUser != MGameManager.Instance.m_owner)
                        {
                            //! 적이 생성할때,
                            Zoom = m_maxZoom;
                            FollowTarget(m_target);
                        }
                        else
                        {   
                            if (m_focusUser.Count == 0)
                            {
                                MapMove();
                                PinchZoom();
                            }
                            else
                            {
                                MUser user = m_focusUser[0];
                                Vector3 target = m_camera.transform.position;
                                GetFocusUserStartingPositionCenter(m_focusUser, ref target);
                                FollowTarget(target);

                                Zoom += Time.deltaTime; if (Zoom > m_maxZoom) Zoom = m_maxZoom;
                            }
                           
                        }
                    }
                    break;
                case CameraMode.Fight:
                    {
                        if (m_focusUser.Count == 0) return;

                        Vector3 target = m_camera.transform.position;                        
                        GetFocusUserUnitCenterPosition(m_focusUser, ref target);
                        FollowTarget(target);

                        if (IsAllUnitInsideCamera(m_focusUser,0.5f) == true) { m_isZoomOut = true; if(Zoom < m_maxZoom ) Zoom += Time.deltaTime; }
                        else if (m_isZoomOut == false) if (Zoom > m_minZoom) Zoom -= Time.deltaTime;
                    }
                    break;
            }
        }

        void StartingFocus()
        {

        }
        void FollowTarget(Vector3 target)
        {
            Vector3 point = m_camera.WorldToViewportPoint(target);
            point.x = m_center.x;
            point.y = m_center.y;
            Vector3 delta = target - m_camera.ViewportToWorldPoint(point);
            Vector3 destination = m_camera.transform.position + delta;
            m_camera.transform.position = Vector3.SmoothDamp(m_camera.transform.position, destination, ref velocity, DampTime);
        }

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
            if (Zoom > m_maxZoom) Zoom = m_maxZoom;
            if(Zoom < m_minZoom) Zoom = m_minZoom;            
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
        }

        public void ClearCameraFocus()
        {
            m_focusUser.Clear();
        }
        public void SetCameraFocus(string id, bool value)
        {
            MUser user = MGameManager.Instance.GetUser(id);
            if (user == null) return;

            if (value == true && m_focusUser.Contains(user) == false)
            {
                m_focusUser.Add(user);
                PlayUI.Instance.SetCameraFocus(id, value);
            }
            else if (value == false && m_focusUser.Contains(user) == true)
            {
                m_focusUser.Remove(user);
                PlayUI.Instance.SetCameraFocus(id, value);
            }
        }
        void LateUpdate()
        {
            if (m_currentMode == CameraMode.None) return;

            m_camera.orthographicSize = (Screen.height) / (2f * Zoom * PixelToUnits);
            Vector3 vOri = m_camera.ScreenPointToRay(Vector3.zero).origin;
            
            Vector3 vPos = m_camera.transform.position;
            float mod = (1f / (Zoom * PixelToUnits));
            vPos.x -= vOri.x % mod;
            vPos.y -= vOri.y % mod;
            m_camera.transform.position = vPos;

            if (KeepInsideMapBounds)
            {
                DoKeepInsideMapBounds();
            }
        }

        // Update is called once per frame
        void DoKeepInsideMapBounds()
        {
            Rect rCamera = new Rect();
            rCamera.width = Screen.width / (PixelToUnits * Zoom);
            rCamera.height = Screen.height / (PixelToUnits * Zoom);
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

        void OnChangeGameState(GameState gameState)
        {
            m_isZoomOut = false;

            switch (gameState)
            {
                case GameState.Init:
                case GameState.LoadMap: ChangeCameraMode(CameraMode.None); break;
                case GameState.StartStage: ChangeCameraMode(CameraMode.Normal); break;
                case GameState.WaitReady: ChangeCameraMode(CameraMode.Make); break;
                case GameState.Play: ChangeCameraMode(CameraMode.Fight); break;
                
                default: ChangeCameraMode(CameraMode.Normal); break;
            }
        }

        void ChangeCameraMode(CameraMode cameraMode)
        {
            if (m_currentMode == cameraMode) return;

            m_currentMode = cameraMode;

            switch(cameraMode)
            {
                case CameraMode.Normal:
                    {
                        InitMapData();
                    }
                    break;
                case CameraMode.Fight:
                    {
                        for(int i = 0;i<MGameManager.Instance.m_userList.Count;++i)
                        {
                            SetCameraFocus(MGameManager.Instance.m_userList[i].m_id, true);
                        }
                    }
                    break;
            }
        }

        void OnChangeUser(MUser user)
        {
            for (int i = 0; i < MGameManager.Instance.m_userList.Count; ++i)
            {
                SetCameraFocus(MGameManager.Instance.m_userList[i].m_id, false);
            }
            SetCameraFocus(user.m_id, true);

            Vector3 position = Vector3.zero;
            GetCenterPosition(user.m_startingPosition.m_positions, ref position);
            SetTarget((int)position.x, (int)position.y);
        }

        public void SetTarget(Vector3 position)
        {  
            m_target = position;
        }
        
        public void SetTarget(int x, int y)
        {
            m_target = RpgMapHelper.GetTileCenterPosition(x, y);
        }
      
        private void GetCenterPosition(List<Vector2> positions, ref Vector3 position)
        {
            Vector2 pos = Vector2.zero;

            for (int i = 0; i < positions.Count; ++i)
            {
                pos += positions[i];
            }

           if(positions.Count != 0) position = pos / positions.Count;
        }

        private void GetFocusUserStartingPositionCenter(List<MUser> users, ref Vector3 position)
        {
            List<Vector2> positions = new List<Vector2>();
            for (int i = 0; i < users.Count; ++i)
            {
                MUser user = users[i];
                int count = 0;
                Vector3 pos = Vector3.zero;
                for (int j = 0; j < user.m_startingPosition.m_positions.Count; ++j)
                {
                    Vector2 point = user.m_startingPosition.m_positions[j];
                    pos += RpgMapHelper.GetTileCenterPosition((int)point.x, (int)point.y);
                    count++;
                }
                if (count != 0) positions.Add(pos / user.m_startingPosition.m_positions.Count);
            }

            GetCenterPosition(positions, ref position);
        }
        private void GetFocusUserUnitCenterPosition(List<MUser> users, ref Vector3 position)
        {
            List<Vector2> positions = new List<Vector2>();
            for(int i = 0;i< users.Count;++i)
            {
                MUser user = users[i];
                int count = 0;                
                Vector3 pos = Vector3.zero;
                for (int j = 0;j< user.m_aliveUnits.Count;++j)
                {
                    MUnit unit = user.m_aliveUnits[j];
                    pos += unit.transform.position;
                    count++;
                }
                if(count != 0) positions.Add(pos / user.m_aliveUnits.Count);
            }

            GetCenterPosition(positions, ref position);
        }

        private bool InAllInSideCamera(List<Vector2> position, float margin)
        {
            float cameraHeight;
            float cameraWidth;
            CameraSize(out cameraWidth, out cameraHeight);

            float minX = 0;
            float minY = 0;
            float maxX = 0;
            float maxY = 0;

            float totalWidth = 0;
            float totalHeight = 0;

            int count = 0;
            for (int i = 0;i<position.Count;++i)
            {
                Vector2 pos = position[i];
                if (count == 0)
                {
                    minX = pos.x;
                    minY = pos.y;
                    maxX = pos.x;
                    maxY = pos.y;
                }

                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;

                totalWidth = Mathf.Abs(minX - maxX);
                totalHeight = Mathf.Abs(minY - maxY);

                if (totalHeight * (1 + margin) > cameraHeight) return false;
                if (totalWidth * (1 + margin) > cameraWidth) return false;

                count++;
            }

            return true;
        }

        private bool IsAllUnitInsideCamera(List<MUser> users, float margin)
        {
            List<Vector2> positions = new List<Vector2>();
            for (int i = 0; i < users.Count; ++i)
            {
                MUser user = users[i];

                for (int j = 0; j < user.m_aliveUnits.Count; ++j)
                {
                    positions.Add(user.m_aliveUnits[j].transform.position);
                }
            }
            return InAllInSideCamera(positions, margin);
        }

        private bool IsAllStartingPointsInsideCamera(List<MUser> users, float margin)
        {
            List<Vector2> positions = new List<Vector2>();
            for (int i = 0; i < users.Count; ++i)
            {
                MUser user = users[i];

                for (int j = 0; j < user.m_startingPosition.m_positions.Count; ++j)
                {
                    positions.Add(user.m_startingPosition.m_positions[j]);
                }
            }
            return InAllInSideCamera(positions, margin);
        }

        public void FullScreenAndCenter()
        {
            Zoom = m_minZoom;
            m_camera.transform.position = new Vector3(m_mapRect.width * 0.5f, -m_mapRect.height * 0.5f, m_camera.transform.position.z);
        }

        public IEnumerator MapTour()
        {
            //! 가장 윗쪽 중심에서 가장 작게 보이게 한다음, 아래로 끝까지 내린다.
            FullScreenAndCenter();
            float topY = m_mapTop;
            float bottomY = m_mapBottom;
            //m_camera.transform.position += new Vector3(0, topY, 0);


            float time = 0;
            float duration = 1;
            Vector3 position = m_camera.transform.position;

            while (time < duration)
            {
                yield return null;
                position.y = m_mapCenter + Mathf.Lerp(0, topY, time / duration);
                m_camera.transform.position = position;
                time += Time.deltaTime;
            }

            time = 0;
            duration = 2;
            while (time < duration)
            {
                yield return null;
                position.y = m_mapCenter + Mathf.Lerp(topY, bottomY, time / duration);
                m_camera.transform.position = position;
                time += Time.deltaTime;
            }
        }
    }

}

