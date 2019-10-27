using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardManager : MonoBehaviour {

    public int boardRowCount = 8;
    public int boardColCount = 8;
    
    private const int TILE_SIZE = 1;
    private const float TILE_OFFSET = 0.5f;
    private bool isBonded = false;

    private float selected_X;
    private float selected_Y;
    private bool isBoardInteractable = true;
    
    [SerializeField]
    private List<ChessController> otherSideChessList = new List<ChessController>();
    [SerializeField]
    private List<ChessController> selfSideChessList = new List<ChessController>();
    [SerializeField]
    private int[,] boardOccupiedStatus = new int[8, 8];

    public List<ChessController> GetChessList_OtherSide { get { return this.otherSideChessList; } }
    public List<ChessController> GetChessList_SelfSide { get { return this.selfSideChessList; } }

    public void MoveChess(ChessController chess, Vector2 pos_from, Vector2 pos_to) {
        if (chess == null) {
            return;
        }

        if (!selfSideChessList.Contains(chess)) {
            selfSideChessList.Add(chess);
        }

        boardOccupiedStatus[(int)pos_from.x, (int)pos_from.y] = 0;
        boardOccupiedStatus[(int)pos_to.x, (int)pos_to.y] = 0;
        chess.Position = pos_to;
    }
    
    void OnEnable() {
        InteractEventsManager.MouseDragging += () => { isBoardInteractable = false; };
        InteractEventsManager.MouseDoneDrag += () => { isBoardInteractable = true; };
        InteractEventsManager.MouseEnterInteractable += () => { isBoardInteractable = false; };
        InteractEventsManager.MouseLeaveInteractable += () => { isBoardInteractable = true; };
    }

    void Start() {
        InitChessLayout();
    }

    void Update() {
        CreateBoardLayout();
        InteractableCheck();

        if (Input.GetKeyDown(KeyCode.Space) && !isBonded) {
            GameManager.Instance.BindingFocus();
            isBonded = true;
        }
    }
    
    private void InteractableCheck() {
        if (!isBoardInteractable) {
            return;
        }

        if (!Camera.main) {
            return;
        }

        var originPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(originPoint, out RaycastHit hitInfo, 25.0f, LayerMask.GetMask("ChessPanel"))) {
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

            Debug.DrawLine(bottomLeft, topRight, Color.yellow);
            Debug.DrawLine(topLeft, bottomRight, Color.yellow);
        }
    }

    private void CreateBoardLayout() {
        var widthVector = Vector3.right * boardColCount;
        var heightVector = Vector3.forward * boardRowCount;

        for (int i = 0; i <= boardRowCount; i++) {
            var start_H = Vector3.forward * i;
            var end_H = start_H + widthVector;
            Debug.DrawLine(start_H, end_H);

            for (int j = 0; j <= boardColCount; j++) {
                var start_V = Vector3.right * j;
                var end_V = start_V + heightVector;
                Debug.DrawLine(start_V, end_V);
            }
        }
    }

    private void ResetBoardSlot(int i, int j) {
        if (i < boardOccupiedStatus.GetLength(0) && j < boardOccupiedStatus.GetLength(1)) {
            boardOccupiedStatus[i, j] = 0;
        }
    }

    public void QuitBoardField(ChessController _chess) {
        if (_chess._chessType == ChessType.SELF_SIDE) {
            if (selfSideChessList.Contains(_chess)) {
                selfSideChessList.Remove(_chess);
            }
        }

        if (_chess._chessType == ChessType.OTHER_SIDE) {
            if (otherSideChessList.Contains(_chess)) {
                otherSideChessList.Remove(_chess);
            }
        }

        ResetBoardSlot((int)_chess.Position.x, (int)_chess.Position.y);
    }
    
    // i: 行下标，j: 列下标
    public Vector3 GetTileCenter(int i, int j) {
        if (!IsInRange(i, j)) {
            return new Vector3(TILE_OFFSET, 0, TILE_OFFSET);
        }
        return new Vector3((TILE_SIZE * i) + TILE_OFFSET, 0, (TILE_SIZE * j) + TILE_OFFSET);
    }

    public bool IsSelected {
        get {
            if (!isBoardInteractable) {
                return false;
            }
            return IsInRange((int)selected_X, (int)selected_Y);
        }
    }

    public Vector3 SelectedTilePos {
        get { return GetTileCenter((int)selected_X, (int)selected_Y); }
    }

    private bool IsInRange(int i, int j) {
        if (!(i >= 0 && j >= 0) || !(i < boardRowCount && j < boardRowCount)) {
            return false;
        }

        return true;
    }
}
