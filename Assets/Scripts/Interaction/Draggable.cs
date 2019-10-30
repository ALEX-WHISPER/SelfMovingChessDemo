using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour {

    public GameObject display_AllowPlacing;
    public GameObject display_NoPlacing;

    public LayerMask interactLayer;
    public float yPosOnDragging = 0f;
    public float yPosOnDragged = 0f;

    private BoardManager boardManager;

    private Vector3 lastLocation;
    private Vector3 newLocation;
    private Vector3 mOffset;
    private float mZCoord;
    
    private int selected_X = -1;
    private int selected_Y = -1;
    private bool isCheckSelection = false;
    private bool isAllowedPlacing = false;

    private GameObject display_Green = null;
    private GameObject display_Red = null;

    public bool IsDraggable { get; set; }

    void Start() {
        lastLocation = transform.position;
        boardManager = GameObject.Find("ChessBoard").GetComponent<BoardManager>();
        IsDraggable = true;
    }

    void Update() {
        if(isCheckSelection && IsDraggable) {
            CheckSelection();
        }
    }

    void OnMouseDown() {
        if (!IsDraggable) {
            return;
        }

        mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;
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

        DeactivateDisplayEffect();
        if (boardManager == null || !isAllowedPlacing) {
            transform.position = lastLocation;
            return;
        }

        newLocation = boardManager.GetTileCenter(selected_X, selected_Y);
        transform.position = newLocation;

        var chess = transform.GetComponent<ChessController>();
        var pos_from = new Vector2(lastLocation.x, lastLocation.z);
        var pos_to = new Vector2(newLocation.x, newLocation.z);

        isCheckSelection = false;
        
        boardManager.MoveChess(chess, pos_from, pos_to);
        if (chess.Position == pos_from) {
            transform.position = lastLocation;
        }

        InteractEventsManager.MouseDoneDrag?.Invoke();
    }
    
    private void CheckSelection() {
        if (!Camera.main) {
            return;
        }

        var originPoint = transform.position;
        if (Physics.Raycast(originPoint, Vector3.down, out RaycastHit hitInfo, 100.0f, interactLayer)) {
            selected_X = (int)hitInfo.point.x;
            selected_Y = (int)hitInfo.point.z;
        } else {
            selected_X = -1;
            selected_Y = -1;
        }

        // draw selecting area
        if (selected_X >= 0 && selected_Y >= 0) {
            // this position is occupied
            if (boardManager.GetBoardGridStatus(selected_X, selected_Y) != 0) {
                ActivateDisplayEffect(false);
                isAllowedPlacing = false;
            } else {
                ActivateDisplayEffect(true);
                isAllowedPlacing = true;
            }
        }
    }

    private Vector3 GetMouseWorldPosition() {
        var screenPos = Input.mousePosition;
        screenPos.z = mZCoord;

        var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return worldPos;
    }

    private void ActivateDisplayEffect(bool isAllowPlacing) {
        var characterLocation = boardManager.GetTileCenter(selected_X, selected_Y);

        // 在当前位置尝试点击拖拽时不显示放置效果
        if (characterLocation.x == lastLocation.x && characterLocation.z == lastLocation.z) {
            return;
        }

        var displayLocation = new Vector3(characterLocation.x, 0.01f, characterLocation.z);

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

    private void DeactivateDisplayEffect() {
        if (display_Red != null)
            display_Red.SetActive(false);

        if (display_Green != null)
            display_Green.SetActive(false);
    }
}
