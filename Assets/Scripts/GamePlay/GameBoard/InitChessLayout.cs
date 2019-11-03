using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardManager : MonoBehaviour {

    private void InitChessLayout() {
        SpawnEnemyChess();
    }

    private void SpawnEnemyChess() {
        var list = _gameProp.RoundNo <= 3 ? chessPrefab_OtherSide_Normal : chessPrefab_OtherSide_Hero;

        // 在非重复位置实例化指定数量的敌人棋子
        for (int i = 0; i < _gameProp.enemyNumInEachRound[_gameProp.RoundNo - 1]; i++) {
            int rowIndex, colIndex;
            var prefabIndex = Random.Range(0, list.Count);

            // 获取非重复、允许放置的位置
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

    private bool GetBattleFieldAvailableSlot_Other(out int rowIndex, out int colIndex) {
        int i = -1, j = -1;

        // 随机范围为敌方的战斗区域
        do {
            i = Random.Range(0, 8);
            j = Random.Range(4, 7);
        } while (boardOccupiedStatus[i, j] != 0);
        
        rowIndex = i;
        colIndex = j;

        if (i == - 1 || j == -1) {
            return false;
        }

        return true;
    }

    private bool GetBattleFieldAvailableSlot_Self(out int rowIndex, out int colIndex) {
        int i = -1, j = -1;

        // 随机范围为己方的战斗区域
        do {
            i = Random.Range(0, 4);
            j = Random.Range(1, 4);
        } while (boardOccupiedStatus[i, j] != 0);

        rowIndex = i;
        colIndex = j;

        if (i == -1 || j == -1) {
            return false;
        }

        return true;
    }
}
