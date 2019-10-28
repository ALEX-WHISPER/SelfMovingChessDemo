using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public System.Action<ChessController, Vector2, Vector2> OnChessSetOnBoard;
    public List<ChessController> GetChessList_OtherSide { get { return this.otherSideChessList; } }
    public List<ChessController> GetChessList_SelfSide { get { return this.selfSideChessList; } }

    [Header("InitChess")]
    public int chessCount_OtherSide = 3;

    public List<GameObject> chessPrefab_OtherSide;
    public List<GameObject> chessPrefab_SelfSide;

    public int GetBoardGridStatus(int i, int j) {
        if (!(i >= 0 && i <= boardOccupiedStatus.GetLength(0)) || !(j >= 0 && j <= boardOccupiedStatus.GetLength(1))) {
            return -1;
        }

        return boardOccupiedStatus[i, j];
    }
    
    void OnEnable() {
        InteractEventsManager.MouseDragging += () => { isBoardInteractable = false; };
        InteractEventsManager.MouseDoneDrag += () => { isBoardInteractable = true; };
        InteractEventsManager.MouseEnterInteractable += () => { isBoardInteractable = false; };
        InteractEventsManager.MouseLeaveInteractable += () => { isBoardInteractable = true; };

        OnChessSetOnBoard += MoveChess;
        GameManager.Instance.OnChessPurchased += OnChessPurchasedCallback;
    }

    void Start() {
        InitChessLayout();
    }

    void Update() {
        //CreateBoardLayout();

        if (Input.GetKeyDown(KeyCode.Space) && !isBonded) {
            GameManager.Instance.BindingFocus();
            isBonded = true;
        }
    }

    // 可视化棋盘
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

    // 移动棋子
    private void MoveChess(ChessController chess, Vector2 pos_from, Vector2 pos_to) {
        if (chess == null) {
            return;
        }

        // 将棋子放回备战区
        if ((int)(pos_to.y) <= 0) {

            // 从战斗列表中移除
            if (selfSideChessList.Contains(chess)) {
                selfSideChessList.Remove(chess);
            }

            // 原位置置0，备战格位置-1
            boardOccupiedStatus[(int)pos_from.x, (int)pos_from.y] = 0;
            boardOccupiedStatus[(int)pos_to.x, (int)pos_to.y] = -1;
            chess.Position = pos_to;
        }

        // 将棋子放入战斗区
        else {

            // 加入至战斗列表
            if (!selfSideChessList.Contains(chess)) {
                selfSideChessList.Add(chess);
            }

            // 原位置置0，新位置置1
            boardOccupiedStatus[(int)pos_from.x, (int)pos_from.y] = 0;
            boardOccupiedStatus[(int)pos_to.x, (int)pos_to.y] = 1;
            chess.Position = pos_to;
        }
    }

    private void InitChessToBackupField(ChessController chess, Vector3 pos_to) {
        if (chess == null) {
            return;
        }

        // 从战斗列表中移除
        if (selfSideChessList.Contains(chess)) {
            selfSideChessList.Remove(chess);
        }

        // 备战格位置-1
        boardOccupiedStatus[(int)pos_to.x, (int)pos_to.y] = -1;
        chess.Position = pos_to;
    }

    // 置空棋盘中的指定位置
    private void ResetBoardSlot(int i, int j) {
        if (i < boardOccupiedStatus.GetLength(0) && j < boardOccupiedStatus.GetLength(1)) {
            boardOccupiedStatus[i, j] = 0;
        }
    }

    // 某棋子死后退出战场-从战斗列表中移除
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

        if (selfSideChessList.Count == 0) {
            GameManager.Instance.OnOtherSideVictory?.Invoke();
        }

        if (otherSideChessList.Count == 0) {
            GameManager.Instance.OnSelfSideVictory?.Invoke();
        }
    }
    
    // i: 横向行序，j: 纵向列序
    public Vector3 GetTileCenter(int i, int j) {
        if (!IsInRange(i, j)) {
            return new Vector3(TILE_OFFSET, 0, TILE_OFFSET);
        }
        return new Vector3((TILE_SIZE * i) + TILE_OFFSET, 0, (TILE_SIZE * j) + TILE_OFFSET);
    }
    
    // 判断要指定的位置是否合法
    private bool IsInRange(int i, int j) {
        if (!(i >= 0 && j >= 0) || !(i < boardRowCount && j < boardRowCount)) {
            return false;
        }

        return true;
    }

    public Vector3 GetFirstAvailableFromBackupField() {
        for (int i = 0; i < boardOccupiedStatus.GetLength(0); i++) {
            if (boardOccupiedStatus[i, 0] == 0) {
                return GetTileCenter(i, 0);
            }
            continue;
        }
        return Vector3.zero;
    }

    private void OnChessPurchasedCallback(ChessHero character) {
        // select available position in backup field
        var _pos = GetFirstAvailableFromBackupField();

        // instatiate it
        var _prefab = chessPrefab_SelfSide.Where(c=>c.GetComponent<ChessController>()._chessCharacter == character).First();
        var _go = Instantiate(_prefab, _pos, _prefab.transform.localRotation);

        // move to
        InitChessToBackupField(_go.GetComponent<ChessController>(), _pos);
    }
    #region unused
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
    #endregion
}
