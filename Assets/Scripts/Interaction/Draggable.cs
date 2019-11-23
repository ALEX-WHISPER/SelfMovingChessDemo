using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour {

    public GameObject display_AllowPlacing; // prefab: 表示允许放置
    public GameObject display_NoPlacing;    // prefab: 表示不允许放置

    public LayerMask interactLayer;
    public float yPosOnDragging = 0f;   // 拖拽过程中棋子的 y 坐标（略高出棋盘表面）
    public float yPosOnDragged = 0f;    // 拖拽结束后棋子的 y 坐标（等于棋盘表面）

    private BoardManager boardManager;

    private Vector3 lastLocation;   // 拖拽前的位置
    private Vector3 newLocation;    // 拖拽后的位置
    private Vector3 mOffset;
    private float mZCoord;
    
    private int selected_X = -1;
    private int selected_Y = -1;
    private bool isCheckSelection = false;
    private bool isAllowedPlacing = false;

    private GameObject display_Green = null;    // 实例化的表示格位空闲的物体
    private GameObject display_Red = null;      // 实例化的表示格位非空闲的物体

    // 该棋子是否可拖拽
    public bool IsDraggable { get; set; }

    void Start() {
        lastLocation = transform.position;
        boardManager = GameObject.Find("ChessBoard").GetComponent<BoardManager>();
        IsDraggable = true;
    }

    void Update() {
        // 拖拽时的射线检测
        if(isCheckSelection && IsDraggable) {
            CheckSelection();
        }

        // 单击后的射线检测
        if (Input.GetMouseButtonDown(0)) {
            var originPoint = Camera.main.ScreenPointToRay(Input.mousePosition);

            // layermask 为棋子对象所在的 layer
            if (Physics.Raycast(originPoint, out RaycastHit hitInfo, 25.0f, LayerMask.GetMask("MyChess"))) {
                if (hitInfo.collider.name == transform.name) {
                    GameManager.Instance.SelectChessToSell(GetComponent<ChessController>());
                }
            } else {
                GameManager.Instance.DeSelectChessToSell();
            }
        }
    }

    void OnMouseDown() {
        if (!IsDraggable) {
            return;
        }

        // 获取当前棋子在屏幕坐标系下的 z 坐标
        mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;

        // 保存当前位置
        lastLocation = transform.position;
    }

    void OnMouseDrag() {
        // move the selected object along with cursor
        //var targetPos = GetMouseWorldPosition() + mOffset;
        
        if (!IsDraggable) {
            return;
        }

        var targetPos = GetMouseWorldPosition();
        targetPos.y = yPosOnDragging;

        transform.position = targetPos;
        isCheckSelection = true;

        InteractEventsManager.MouseDragging?.Invoke();
    }

    void OnMouseUp() {
        // drop the selected object into the drop area
        if (!IsDraggable) {
            return;
        }

        DeactivateDragEffect();
        //DeactivateSelectedEffect();

        if (boardManager == null || !isAllowedPlacing) {
            transform.position = lastLocation;
            return;
        }

        newLocation = boardManager.GetTileCenter(selected_X, selected_Y);
        var chess = transform.GetComponent<ChessController>();
        var pos_from = new Vector2(lastLocation.x, lastLocation.z);
        var pos_to = new Vector2(newLocation.x, newLocation.z);

        isCheckSelection = false;
        
        if (boardManager.MoveChess(chess, pos_from, pos_to)) {
            transform.position = newLocation;
        } else {
            transform.position = lastLocation;
        }

        //InteractEventsManager.MouseDoneDrag?.Invoke();
    }
    
    /// <summary>
    /// 棋子被拖拽、移动过程中，绘制表示相应格位是否空闲的预制体
    /// </summary>
    private void CheckSelection() {
        if (!Camera.main) {
            return;
        }

        var originPoint = transform.position;

        // layermask 为棋盘所在的 Layer
        if (Physics.Raycast(originPoint, Vector3.down, out RaycastHit hitInfo, 100.0f, interactLayer)) {
            // 相当于向下取整
            selected_X = (int)hitInfo.point.x;
            selected_Y = (int)hitInfo.point.z;
        } else {
            selected_X = -1;
            selected_Y = -1;
        }

        // draw selecting area
        if (selected_X >= 0 && selected_Y >= 0) {    
            // position is occupied
            if (boardManager.GetBoardGridStatus(selected_X, selected_Y) != 0) {
                ActivateDragEffect(false);
                isAllowedPlacing = false;
            } 
            
            // position is available
            else {
                ActivateDragEffect(true);
                isAllowedPlacing = true;
            }
        }
    }

    /// <summary>
    /// 获取鼠标在世界坐标下的位置
    /// </summary>
    /// <returns></returns>
    private Vector3 GetMouseWorldPosition() {
        var screenPos = Input.mousePosition;
        screenPos.z = mZCoord;

        var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return worldPos;
    }

    /// <summary>
    /// 显示拖拽效果
    /// </summary>
    /// <param name="isAllowPlacing"></param>
    private void ActivateDragEffect(bool isAllowPlacing) {
        var characterLocation = boardManager.GetTileCenter(selected_X, selected_Y);

        // 在当前位置尝试点击拖拽时不显示放置效果
        if (characterLocation.x == lastLocation.x && characterLocation.z == lastLocation.z) {
            return;
        }

        var displayLocation = new Vector3(characterLocation.x, 0.03f, characterLocation.z);

        if (!isAllowPlacing) {
            if (display_Red == null) {
                display_Red = Instantiate(display_NoPlacing, displayLocation, display_NoPlacing.transform.rotation);
            } else {
                if (display_Green != null)
                    display_Green.SetActive(false);
                display_Red.transform.position = displayLocation;
                display_Red.SetActive(true);
            }
        } else {
            if (display_Green == null) {
                display_Green = Instantiate(display_AllowPlacing, displayLocation, display_AllowPlacing.transform.rotation);
            } else {
                if (display_Red != null)
                    display_Red.SetActive(false);
                display_Green.transform.position = displayLocation;
                display_Green.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 隐藏拖拽效果
    /// </summary>
    private void DeactivateDragEffect() {
        if (display_Red != null)
            display_Red.SetActive(false);

        if (display_Green != null)
            display_Green.SetActive(false);
    }
}
