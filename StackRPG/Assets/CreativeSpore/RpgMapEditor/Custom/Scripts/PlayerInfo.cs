using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using CreativeSpore;
using CreativeSpore.RpgMapEditor;

namespace stackRPG
{
    [RequireComponent(typeof(Text))]
    public class PlayerInfo : MonoBehaviour
    {
        //! 임시
        public AutoTileMap m_autoTileMap;
        public Camera2DController m_camera2D;

        public MUnit m_target;

        [SerializeField]
        public List<MUnit> m_targets = new List<MUnit>();

        public Text m_text;
        private LinkedListNode<MUnit> m_unitLinkedListNode;

        private FollowObjectBehaviour m_followObjectBehaviour;


        public enum ClickAction
        {
            Select,
            Move,
            Attack,
            MapMove,
        }

        public ClickAction m_clickAction_left;
        public ClickAction m_clickAction_right;

        public bool m_isTouching = false;

        public Vector2 m_dragPoint_1;
        public Vector2 m_dragPoint_2;
        
        void OnEnable()
        {
            m_text = GetComponent<Text>();
            m_followObjectBehaviour = Camera.main.GetComponent<FollowObjectBehaviour>();

            UnSelect();
        }

        

        void Update()
        {
            TestCode();

            DrawInfo();

            UpdateKeyBoard();

            UpdateMouseLeft();

            UpdateMouseRight();

            UpdateMouseWheel();
        }



        void UnSelect()
        {
            m_targets.Clear();
            m_clickAction_left = ClickAction.Select;
            m_clickAction_right = ClickAction.MapMove;
        }

        void Select(Collider2D[] cols)
        {
            m_targets.Clear();
            foreach (Collider2D col in cols)
            {
                if (col.CompareTag("Unit") == true) m_targets.Add(col.GetComponent<MUnit>());
            }

            if (m_targets.Count == 0) m_clickAction_right = ClickAction.MapMove;
            else
            {
                m_clickAction_right = ClickAction.Move;
            }
        }

        void SetTextPosition(Vector3 position)
        {   
            position.z = m_text.transform.position.z;
            m_text.transform.position = position;
        }

        
        

        void TestCode()
        {
            if (Input.GetKey(KeyCode.T))
            {
                Vector2 mousePos = RpgMapHelper.GetMouseWorldPosition();
                int tileIdx = RpgMapHelper.GetTileIdxByPosition(mousePos);
                Debug.Log("tileIdx : " + tileIdx + ", mousePos : " + mousePos);

                AutoTile autoTile = RpgMapHelper.GetAutoTileByPosition(mousePos, 0);

                Debug.Log("Tile Pos : " + autoTile.TileX + " , " + autoTile.TileY + " centerPos : " + RpgMapHelper.GetTileCenterPosition(autoTile.TileX,autoTile.TileY));

                Debug.Log("AutoTileMap.Instance.MapTile : " + AutoTileMap.Instance.MapTileWidth + " , " + AutoTileMap.Instance.MapTileHeight);

                Debug.Log("AutoTileMap.Instance.GetAutotileCollisionAtPosition : " + AutoTileMap.Instance.GetAutotileCollisionAtPosition(mousePos));
            }
        }

        void DrawInfo()
        {
            if (m_targets.Count == 0)
            {
                m_text.text = "Select : Drag Left! \nMove Camera : Drag Right!";
               
                SetTextPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            else if(m_targets.Count == 1)
            {
                MUnit target = m_targets[0];
                
                m_text.text = "State : " + target.m_state;
                m_text.text += "\nCommand : " + target.m_command;
                m_text.text += "\nHp : " + target.m_hp;
                m_text.text += "\nPower : " + target.m_weapon.m_power;
                m_text.text += "\nCoolTime : " + target.m_attackCoolTime + " / " + target.m_weapon.m_delay;
                
                SetTextPosition(target.transform.position);
            }
            else
            {
                m_text.text = "Ready to Command!!";

                SetTextPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }

        public float m_screenScrollSpeed = 2.0f;

        void UpdateKeyBoard()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnSelect();
            }

            Vector2 scrollSpeed = Vector2.zero;
            if (Input.GetKey(KeyCode.DownArrow)) scrollSpeed += Vector2.down;            
            if (Input.GetKey(KeyCode.UpArrow)) scrollSpeed += Vector2.up;
            if (Input.GetKey(KeyCode.RightArrow)) scrollSpeed += Vector2.right;
            if (Input.GetKey(KeyCode.LeftArrow)) scrollSpeed += Vector2.left;
            m_camera2D.transform.Translate(scrollSpeed * m_screenScrollSpeed * m_camera2D.Camera.orthographicSize * m_camera2D.Zoom * Time.deltaTime);


            if (m_targets.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.H))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.CommandHold();
                    }
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.CommandStop();
                    }
                }

                if (Input.GetKeyDown(KeyCode.N))
                {
                    foreach (MUnit unit in m_targets)
                    {
                        unit.CommandNone();
                    }
                }

                if (Input.GetKeyDown(KeyCode.A))
                {
                    if (m_clickAction_left == ClickAction.Select)
                    {
                        m_clickAction_left = ClickAction.Attack;
                    }
                }
            }
        }
        void UpdateMouseLeft()
        {
            if (m_clickAction_left == ClickAction.Select)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    m_isTouching = true;
                    m_dragPoint_1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }

                if (Input.GetMouseButton(0) && m_isTouching == true)
                {
                    m_dragPoint_2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    m_isTouching = false;

                    if (RpgMapHelper.GetTileIdxByPosition(m_dragPoint_1) == RpgMapHelper.GetTileIdxByPosition(m_dragPoint_2)) Select(Physics2D.OverlapPointAll(m_dragPoint_1));
                    else Select(Physics2D.OverlapAreaAll(m_dragPoint_1, m_dragPoint_2, -1));
                }
            }
            else if (m_clickAction_left == ClickAction.Attack)
            {
                if (m_targets.Count == 0) { m_clickAction_left = ClickAction.Select; return; }
                else
                {
                    if (Input.GetMouseButtonUp(0))
                    {

                        List<Vector2> canMovePositions;
                        GetCanMovePosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), m_targets.Count, out canMovePositions);
                        for (int i = 0; i < m_targets.Count; ++i) m_targets[i].CommandAttackGround(canMovePositions[i]);

                        //foreach (MUnit unit in m_targets)
                        //{
                        //    unit.CommandAttackGround(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        //}

                        m_clickAction_left = ClickAction.Select; return;
                    }
                }
            }
        }

        void UpdateMouseRight()
        {
            if (m_clickAction_right == ClickAction.Move)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    List<Vector2> canMovePositions;
                    GetCanMovePosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), m_targets.Count, out canMovePositions);
                    for (int i = 0; i < m_targets.Count; ++i) m_targets[i].CommandMoveGround(canMovePositions[i]);
                }
            }
            else if (m_clickAction_right == ClickAction.MapMove)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    m_isTouching = true;
                    m_dragPoint_1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }

                if (Input.GetMouseButton(1) && m_isTouching == true)
                {
                    m_dragPoint_2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    if (m_dragPoint_1 != m_dragPoint_2)
                    {
                        m_camera2D.transform.Translate(m_dragPoint_1 - m_dragPoint_2);
                        m_dragPoint_1 = m_dragPoint_2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    }
                }

                if (Input.GetMouseButtonUp(1))
                {
                    m_isTouching = false;
                }
            }
        }

        void UpdateMouseWheel()
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
            {
                if (m_camera2D.Zoom > 1f)
                    m_camera2D.Zoom = Mathf.Max(m_camera2D.Zoom - 1, 1);
                else
                    m_camera2D.Zoom = Mathf.Max(m_camera2D.Zoom / 2f, 0.05f);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
            {
                if (m_camera2D.Zoom >= 1f)
                    m_camera2D.Zoom = Mathf.Min(m_camera2D.Zoom + 1, 10);
                else
                    m_camera2D.Zoom *= 2f;
            }
        }

        

        void GetCanMovePosition(Vector2 center, int count, out List<Vector2> positions)
        {
            positions = new List<Vector2>();
            AutoTile centerTile = RpgMapHelper.GetAutoTileByPosition(center, 0);

            int index = 0;
            while(positions.Count < count)
            {
                Vector2 pos = center;

                if (GetNextPosition(centerTile, index++, ref pos) == false) continue;
                if (IsCanMovePosition(pos) == false) continue;

                positions.Add(pos);
            }
        }

        bool IsCanMovePosition(Vector2 pos)
        {   
            //! 해당위치가 가능한지 안한지는, 한타일을 9등분해서 판단한다. 젠장 즉, 옆에는 서있을수있다는거다. -_-
            if (AutoTileMap.Instance.GetAutotileCollisionAtPosition(pos) == eTileCollisionType.PASSABLE) return true;
            return false;
        }

        int[] m_nearPositionCount = { 0, 1, 5, 13, 25, 41, 66, 107 };

        bool GetNextPosition(AutoTile centerTile, int index, ref Vector2 pos)
        {
            int distance = -1;
            int number = -1;
            for(int i = 0;i<m_nearPositionCount.Length;i++)
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



        void OnDrawGizmos()
        {
            if(m_isTouching == true && m_clickAction_left == ClickAction.Select)
            { 
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                MSettings.GizmoDrawRectByPoint(m_dragPoint_1, m_dragPoint_2);
            }

            if(m_clickAction_left == ClickAction.Attack)
            {
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                point.z = 0;

                Gizmos.DrawSphere(point, 0.1f);
                Gizmos.DrawLine(point, point + Vector3.right * 0.2f);
                Gizmos.DrawLine(point, point + Vector3.left * 0.2f);
                Gizmos.DrawLine(point, point + Vector3.up * 0.2f);
                Gizmos.DrawLine(point, point + Vector3.down * 0.2f);
            }

            if (m_targets.Count > 0)
            {
                for (int i = 0; i < m_targets.Count; ++i)
                {
                    if (m_targets[i] == null) { m_targets.RemoveAt(i); i--; }
                }

                foreach(MUnit unit in m_targets)
                {
                    Vector2 center = unit.transform.position;
                    float range = 0.2f;
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                    Gizmos.DrawSphere(center, range);
                }
            }
        }


        void OnGUI()
        {
            return;

            if (Input.GetMouseButton(0))
            {
                AutoTile autoTile1 = RpgMapHelper.GetAutoTileByPosition(m_dragPoint_1, 0);
                AutoTile autoTile2 = RpgMapHelper.GetAutoTileByPosition(m_dragPoint_2, 0);


                int m_startDragTileX = autoTile1.TileX;
                int m_startDragTileY = autoTile1.TileY;

                int m_dragTileX = autoTile2.TileX;
                int m_dragTileY = autoTile2.TileY;

                Rect selRect = new Rect();
                selRect.width = (Mathf.Abs(m_dragTileX - m_startDragTileX) + 1) * m_autoTileMap.Tileset.TileWidth * m_camera2D.Zoom;
                selRect.height = (Mathf.Abs(m_dragTileY - m_startDragTileY) + 1) * m_autoTileMap.Tileset.TileHeight * m_camera2D.Zoom;
                float worldX = Mathf.Min(m_startDragTileX, m_dragTileX) * m_autoTileMap.Tileset.TileWorldWidth;
                float worldY = -Mathf.Min(m_startDragTileY, m_dragTileY) * m_autoTileMap.Tileset.TileWorldHeight;
                Vector3 vScreen = m_camera2D.Camera.WorldToScreenPoint(new Vector3(worldX, worldY) + m_autoTileMap.transform.position);
                selRect.position = new Vector2(vScreen.x, vScreen.y);
                selRect.y = Screen.height - selRect.y;
                UtilsGuiDrawing.DrawRectWithOutline(selRect, new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 1f));
            }
        }
    }

}
