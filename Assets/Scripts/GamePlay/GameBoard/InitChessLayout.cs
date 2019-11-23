using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardManager : MonoBehaviour {
    /// <summary>
    /// 初始化棋盘
    /// </summary>
    private void InitChessLayout() {
        SpawnEnemyChess();
    }

    /// <summary>
    /// 生成敌方棋子
    /// </summary>
    private void SpawnEnemyChess() {
        var list = _gameProp.RoundNo <= 3 ? chessPrefab_OtherSide_Normal : chessPrefab_OtherSide_Hero;

        // 在非重复位置实例化指定数量的敌人棋子
        for (int i = 0; i < _gameProp.enemyNumInEachRound[_gameProp.RoundNo - 1]; i++) {
            int rowIndex, colIndex;
            var prefabIndex = Random.Range(0, list.Count);

            // 获取非重复的空闲位置
            if (GetBattleFieldAvailableSlot_Other(out rowIndex, out colIndex)) {
                boardOccupiedStatus[rowIndex, colIndex] = 2; // set the slot occupied

                var initPos = GetTileCenter(rowIndex, colIndex); // get the exact position of that slot
                var chess = Instantiate(list[prefabIndex], initPos, Quaternion.identity, chessHolder_OtherSide); // instantiate the chess

                var _controller = chess.GetComponent<ChessController>();
                if (_controller != null) {
                    battleFieldChess_Other.Add(_controller);
                    _controller.Position = new Vector2(rowIndex, colIndex);
                }
            } else {
                break;
            }
        }
        OnChessListChanged?.Invoke(battleFieldChess_Self.Count, battleFieldChess_Other.Count);
    }

    /// <summary>
    /// 获取敌方战斗区内非重复的空闲格位
    /// </summary>
    /// <param name="rowIndex"> 返回的格位横坐标 </param>
    /// <param name="colIndex"> 返回的格位纵坐标 </param>
    /// <returns> 是否存在符合上述标准的格位 </returns>
    private bool GetBattleFieldAvailableSlot_Other(out int rowIndex, out int colIndex) {
        return GetBattleFieldAvailableSlot(new RangeInt(0, 8), new RangeInt(4, 4), out rowIndex, out colIndex);
    }

    /// <summary>
    /// 获取己方战斗区内非重复的空闲格位
    /// </summary>
    /// <param name="rowIndex"> 返回的格位横坐标 </param>
    /// <param name="colIndex"> 返回的格位纵坐标 </param>
    /// <returns> 是否存在符合上述标准的格位 </returns>
    private bool GetBattleFieldAvailableSlot_Self(out int rowIndex, out int colIndex) {
        return GetBattleFieldAvailableSlot(new RangeInt(0, 4), new RangeInt(1, 4), out rowIndex, out colIndex);
    }

    /// <summary>
    /// 获取棋盘指定范围内非重复的空闲格位
    /// </summary>
    /// <param name="xRange"> 棋盘水平方向的范围 </param>
    /// <param name="yRange"> 棋盘垂直方向的范围 </param>
    /// <param name="rowIndex"> 返回的格位横坐标 </param>
    /// <param name="colIndex"> 返回的格位纵坐标 </param>
    /// <returns> 是否存在符合上述标准的格位 </returns>
    private bool GetBattleFieldAvailableSlot(RangeInt xRange, RangeInt yRange, out int rowIndex, out int colIndex) {
        Debug.Log($"xRange: [{xRange.start}, {xRange.end}], yRange: [{yRange.start}, {yRange.end}]");
        int i = -1, j = -1;

        // 随机范围为敌方的战斗区域
        do {
            i = Random.Range(xRange.start, xRange.end);
            j = Random.Range(yRange.start, yRange.end);
        } while (boardOccupiedStatus[i, j] != 0);

        rowIndex = i;
        colIndex = j;

        if (i == -1 || j == -1) {
            return false;
        }

        return true;
    }
}
