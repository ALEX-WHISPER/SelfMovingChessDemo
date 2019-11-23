using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class BoardManager : MonoBehaviour {
    
    // 8 x 8 棋盘
    public int boardRowCount = 8;
    public int boardColCount = 8;

    public GameProp _gameProp;

    // 可视化物体，选中某棋子后显示在其格位上
    public GameObject display_SelectedPos;
    private GameObject displayTemp;
    
    public Transform chessHolder_SelfSide;  // 己方棋子对象的父物体
    public Transform chessHolder_OtherSide; // 敌方棋子对象的父物体

    private const int TILE_SIZE = 1;    // 格位尺寸大小
    private const float TILE_OFFSET = 0.5f; // 格位中心距离原点的偏移量

    private float selected_X;
    private float selected_Y;

    private bool isBoardInteractable = true;    // 棋盘是否可交互
    private bool isChessMovable = true; // 棋盘内棋子是否可移动

    // 战斗区棋子数量变化回调
    public Action<int, int> OnChessListChanged;

    [SerializeField]
    // 战斗区敌方棋子列表
    private List<ChessController> battleFieldChess_Other = new List<ChessController>();

    [SerializeField]
    // 战斗区己方棋子列表
    private List<ChessController> battleFieldChess_Self = new List<ChessController>();

    [SerializeField]
    // 备战区棋子列表
    private List<ChessController> backupFieldChessList = new List<ChessController>();

    [SerializeField]
    // 棋盘占用情况
    private int[,] boardOccupiedStatus = new int[8, 8];
    
    public List<ChessController> GetBackupFieldList { get { return this.backupFieldChessList; } }
    public List<ChessController> GetChessList_OtherSide { get { return this.battleFieldChess_Other; } }
    public List<ChessController> GetChessList_SelfSide { get { return this.battleFieldChess_Self; } }

    [Header("InitChess")]
    public int chessCount_OtherSide = 3;

    public List<GameObject> chessPrefab_OtherSide_Normal;
    public List<GameObject> chessPrefab_OtherSide_Hero;
    public List<GameObject> chessPrefab_SelfSide;

    /// <summary>
    /// 获取某格位的占用情况
    /// </summary>
    /// <param name="i"> 横坐标 </param>
    /// <param name="j"> 纵坐标 </param>
    /// <returns></returns>
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

    private void Update() {
        //InteractableCheck();
    }

    /// <summary>
    /// 可视化棋盘
    /// </summary>
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

    /// <summary>
    /// 销毁备战区内一枚棋子
    /// </summary>
    /// <param name="chess"></param>
    public void DestroyBackupFieldChess(ChessController chess) {
        if (chess == null || !backupFieldChessList.Contains(chess)) {
            return;
        }

        // remove from backup field list
        backupFieldChessList.Remove(chess);

        // reset position status
        boardOccupiedStatus[(int)chess.Position.x, (int)chess.Position.y] = 0;

        // destroy the gameObject
        Destroy(chess.gameObject);
    }

    /// <summary>
    /// 移动棋子
    /// </summary>
    /// <param name="chess"> 要移动的棋子 </param>
    /// <param name="pos_from"> 起始位置 </param>
    /// <param name="pos_to"> 目标位置 </param>
    /// <returns></returns>
    public bool MoveChess(ChessController chess, Vector2 pos_from, Vector2 pos_to) {
        if (chess == null || !isChessMovable) {
            return false;
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
            
            // 若要增加上场的棋子数量，但场上棋子数已超过本轮最大可战斗棋子数，则无法移动
            if ((int)(pos_from.y) <= 0 && _gameProp.MaxChessNum <= battleFieldChess_Self.Count) {
                chess.Position = pos_from;
                return false;
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
        return true;
    }

    // 获取棋盘矩阵中指定坐标的实际位置
    // i: 横向行序，j: 纵向列序
    public Vector3 GetTileCenter(int i, int j) {
        if (!IsInRange(i, j)) {
            return new Vector3(TILE_OFFSET, 0, TILE_OFFSET);
        }
        return new Vector3((TILE_SIZE * i) + TILE_OFFSET, 0, (TILE_SIZE * j) + TILE_OFFSET);
    }

    /// <summary>
    /// 判断要指定的位置是否合法
    /// </summary>
    /// <param name="i"> 横坐标 </param>
    /// <param name="j"> 纵坐标 </param>
    /// <returns> 该位置是否位于棋盘区域内 </returns>
    private bool IsInRange(int i, int j) {
        if (!(i >= 0 && j >= 0) || !(i < boardRowCount && j < boardRowCount)) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 重置格位状态
    /// </summary>
    /// <param name="i"> 横坐标 </param>
    /// <param name="j"> 纵坐标 </param>
    private void ResetBoardSlot(int i, int j) {
        if (i < boardOccupiedStatus.GetLength(0) && j < boardOccupiedStatus.GetLength(1)) {
            boardOccupiedStatus[i, j] = 0;
        }
    }

    #region backup field
    /// <summary>
    /// 将购买的棋子置入备战区
    /// </summary>
    /// <param name="prop"> 购买的棋子 </param>
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

    /// <summary>
    /// 将指定棋子添加至备战区指定位置
    /// </summary>
    /// <param name="chess"> 要添加的棋子 </param>
    /// <param name="pos_to"> 添加至目标位置 </param>
    private void InitChessToBackupField(ChessController chess, Vector3 pos_to) {
        if (chess == null) {
            return;
        }

        // 添加至备战区列表内
        if (!backupFieldChessList.Contains(chess)) {
            backupFieldChessList.Add(chess);
        }

        // 备战格位置-1
        boardOccupiedStatus[(int)pos_to.x, (int)pos_to.y] = -1;

        // 移动棋子对象至目标位置
        chess.Position = pos_to;
        chess.transform.position = pos_to;

        // 重置备战区列表内所有棋子的初始状态
        for (int i = 0; i < backupFieldChessList.Count; i++) {
            backupFieldChessList[i].ResetChess();
        }
    }

    /// <summary>
    /// 获取备战区内第一个空闲位置
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// 棋子阵亡后退出战场
    /// </summary>
    /// <param name="_chess"></param>
    public void QuitBattleField(ChessController _chess) {
        // 从相应列表中被移除
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

        // 重置格位状态
        ResetBoardSlot((int)_chess.Position.x, (int)_chess.Position.y);

        // 战斗区棋子数量变化
        OnChessListChanged?.Invoke(battleFieldChess_Self.Count, battleFieldChess_Other.Count);
    }

    /// <summary>
    /// 回合结束后，重置战斗区内存活的棋子状态
    /// </summary>
    private void ResetSurvivedChess() {
        // 将战斗区内仍存活的棋子依次添加至备战区内
        for (int i = 0; i < battleFieldChess_Self.Count; i++) {
            InitChessToBackupField(battleFieldChess_Self[i], GetFirstAvailableFromBackupField());
        }
    }
    #endregion
}
