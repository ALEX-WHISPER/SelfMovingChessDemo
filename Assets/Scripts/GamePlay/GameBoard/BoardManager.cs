using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class BoardManager : MonoBehaviour {
    
    public int boardRowCount = 8;
    public int boardColCount = 8;
    public GameProp _gameProp;

    public Transform chessHolder_SelfSide;
    public Transform chessHolder_OtherSide;
    
    private const int TILE_SIZE = 1;
    private const float TILE_OFFSET = 0.5f;

    private float selected_X;
    private float selected_Y;
    private bool isBoardInteractable = true;
    private bool isChessMovable = true;

    public Action<int, int> OnChessListChanged;

    [SerializeField]
    private List<ChessController> battleFieldChess_Other = new List<ChessController>();
    [SerializeField]
    private List<ChessController> battleFieldChess_Self = new List<ChessController>();
    [SerializeField]
    private List<ChessController> backupFieldChessList = new List<ChessController>();
    [SerializeField]
    private int[,] boardOccupiedStatus = new int[8, 8];
    
    public List<ChessController> GetBackupFieldList { get { return this.backupFieldChessList; } }
    public List<ChessController> GetChessList_OtherSide { get { return this.battleFieldChess_Other; } }
    public List<ChessController> GetChessList_SelfSide { get { return this.battleFieldChess_Self; } }

    [Header("InitChess")]
    public int chessCount_OtherSide = 3;

    public List<GameObject> chessPrefab_OtherSide_Normal;
    public List<GameObject> chessPrefab_OtherSide_Hero;
    public List<GameObject> chessPrefab_SelfSide;

    public int GetBoardGridStatus(int i, int j) {
        if (!(i >= 0 && i < boardOccupiedStatus.GetLength(0)) || !(j >= 0 && j < boardOccupiedStatus.GetLength(1))) {
            return -1;
        }

        return boardOccupiedStatus[i, j];
    }
    
    void OnEnable() {
        InteractEventsManager.MouseDragging += () => { isBoardInteractable = false; };
        InteractEventsManager.MouseDoneDrag += () => { isBoardInteractable = true; };
        InteractEventsManager.MouseEnterInteractable += () => { isBoardInteractable = false; };
        InteractEventsManager.MouseLeaveInteractable += () => { isBoardInteractable = true; };
    }

    void Start() {
        //InitChessLayout();
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
    public void MoveChess(ChessController chess, Vector2 pos_from, Vector2 pos_to) {
        if (chess == null || !isChessMovable) {
            return;
        }

        // 将棋子放回备战区
        if ((int)(pos_to.y) <= 0) {

            // 从战斗列表中移除
            if (battleFieldChess_Self.Contains(chess)) {
                battleFieldChess_Self.Remove(chess);
            }

            // 添加至备战区列表 
            if (!backupFieldChessList.Contains(chess)) {
                backupFieldChessList.Add(chess);
            }

            // 原位置置0，备战格位置-1
            boardOccupiedStatus[(int)pos_from.x, (int)pos_from.y] = 0;
            boardOccupiedStatus[(int)pos_to.x, (int)pos_to.y] = -1;
            chess.Position = pos_to;
        }

        // 将棋子放入战斗区
        if ((int)(pos_to.y) > 0) {
            
            // 若场上棋子数已超过本轮最大可战斗棋子数，则无法移动
            if (_gameProp.MaxChessNum <= battleFieldChess_Self.Count) {
                chess.Position = pos_from;
                return;
            }

            // 加入至战斗列表
            if (!battleFieldChess_Self.Contains(chess)) {
                battleFieldChess_Self.Add(chess);
            }

            // 从备战区列表移除
            if (backupFieldChessList.Contains(chess)) {
                backupFieldChessList.Remove(chess);
            }

            // 原位置置0，新位置置1
            boardOccupiedStatus[(int)pos_from.x, (int)pos_from.y] = 0;
            boardOccupiedStatus[(int)pos_to.x, (int)pos_to.y] = 1;
            chess.Position = pos_to;
        }
        OnChessListChanged(battleFieldChess_Self.Count, battleFieldChess_Other.Count);
    }

    // 获取棋盘矩阵中指定坐标的实际位置
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
    
    // 清除指定格位
    private void ResetBoardSlot(int i, int j) {
        if (i < boardOccupiedStatus.GetLength(0) && j < boardOccupiedStatus.GetLength(1)) {
            boardOccupiedStatus[i, j] = 0;
        }
    }

    #region backup field
    // 将购买的棋子置入备战区
    public void SetNewChessToBackupField(ChessProp prop) {
        // select available position in backup field
        var _pos = GetFirstAvailableFromBackupField();

        // no more room in backup field
        if (_pos.sqrMagnitude == 0) {
            return;
        }

        // instatiate it
        var _prefab = chessPrefab_SelfSide.Where(c => c.GetComponent<ChessController>().CharacterType == prop.character).First();
        var _go = Instantiate(_prefab, _pos, _prefab.transform.localRotation, chessHolder_SelfSide);

        // move to
        InitChessToBackupField(_go.GetComponent<ChessController>(), _pos);
    }

    // 添加棋子至备战区
    private void InitChessToBackupField(ChessController chess, Vector3 pos_to) {
        if (chess == null) {
            return;
        }

        if (!backupFieldChessList.Contains(chess)) {
            backupFieldChessList.Add(chess);
        }

        // 备战格位置-1
        boardOccupiedStatus[(int)pos_to.x, (int)pos_to.y] = -1;
        chess.Position = pos_to;
        chess.transform.position = pos_to;

        for (int i = 0; i < backupFieldChessList.Count; i++) {
            backupFieldChessList[i].ResetChess();
        }
    }

    // 获取备战区内第一个空闲位置
    private Vector3 GetFirstAvailableFromBackupField() {
        for (int i = 0; i < boardOccupiedStatus.GetLength(0); i++) {
            if (boardOccupiedStatus[i, 0] == 0) {
                return GetTileCenter(i, 0);
            }
            continue;
        }
        return Vector3.zero;
    }

    #endregion

    #region battle field
    // 棋子阵亡后退出战场
    public void QuitBattleField(ChessController _chess) {
        if (_chess.Camp == ChessCamp.SELF_SIDE) {
            if (battleFieldChess_Self.Contains(_chess)) {
                battleFieldChess_Self.Remove(_chess);
            }
        }

        if (_chess.Camp == ChessCamp.OTHER_SIDE) {
            if (battleFieldChess_Other.Contains(_chess)) {
                battleFieldChess_Other.Remove(_chess);
            }
        }

        ResetBoardSlot((int)_chess.Position.x, (int)_chess.Position.y);

        OnChessListChanged?.Invoke(battleFieldChess_Self.Count, battleFieldChess_Other.Count);
    }

    // 本轮战斗结束后，恢复存活棋子状态
    private void ResetSurvivedChess() {
        for (int i = 0; i < battleFieldChess_Self.Count; i++) {
            InitChessToBackupField(battleFieldChess_Self[i], GetFirstAvailableFromBackupField());
        }
    }

    private void ResetBackupChess() {
        for (int i = 0; i < backupFieldChessList.Count; i++) {
            backupFieldChessList[i].ResetChess();
        }
    }
    #endregion
    
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
