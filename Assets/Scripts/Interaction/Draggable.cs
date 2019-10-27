using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour {

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

    void Start() {
        lastLocation = transform.position;
        boardManager = GameObject.Find("ChessBoard").GetComponent<BoardManager>();

        InteractEventsManager.MouseDoneDrag += () => {
            var chess = transform.GetComponent<ChessController>();
            var pos_from = new Vector2(lastLocation.x, lastLocation.z);
            var pos_to = new Vector2(newLocation.x, newLocation.z);
            boardManager.MoveChess(chess, pos_from, pos_to);
        };
    }

    void Update() {
        if(isCheckSelection) {
            CheckSelection();
        }
    }

    void OnMouseDown() {
        mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;

        // calc the offset between the selected object and cursor
        mOffset = transform.position - GetMouseWorldPosition();
    }

    void OnMouseDrag() {
        // move the selected object along with cursor
        var targetPos = GetMouseWorldPosition() + mOffset;
        targetPos.y = yPosOnDragging;

        transform.position = targetPos;
        isCheckSelection = true;

        InteractEventsManager.MouseDragging?.Invoke();
    }

    void OnMouseUp() {
        // drop the selected object into the drop area

        if (boardManager == null) {
            transform.position = new Vector3(0f, transform.position.y, 0f);
            return;
        } 

        lastLocation = transform.position;
        newLocation = boardManager.GetTileCenter(selected_X, selected_Y);
        transform.position = newLocation;

        isCheckSelection = false;
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
            var bottomLeft = Vector3.right * selected_X + Vector3.forward * selected_Y;
            var topRight = Vector3.right * (selected_X + 1) + Vector3.forward * (selected_Y + 1);

            var topLeft = Vector3.right * selected_X + Vector3.forward * (selected_Y + 1);
            var bottomRight = Vector3.right * (selected_X + 1) + Vector3.forward * selected_Y;

            Debug.DrawLine(bottomLeft, topRight, Color.green);
            Debug.DrawLine(topLeft, bottomRight, Color.green);
        }
    }

    private Vector3 GetMouseWorldPosition() {
        var screenPos = Input.mousePosition;
        screenPos.z = mZCoord;

        var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return worldPos;
    }
}
