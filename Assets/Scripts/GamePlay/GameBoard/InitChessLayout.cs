using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardManager : MonoBehaviour {

    [Header("InitChess")]
    public int chessCount_OtherSide = 3;
    public List<GameObject> chessPrefab_OtherSide;
    public List<GameObject> chessPrefab_SelfSide;

    private void InitChessLayout() {
        InitOtherSide();
    }

    private void InitOtherSide() {
        for (int i = 0; i < chessCount_OtherSide; i++) {
            int rowIndex, colIndex;
            var prefabIndex = Random.Range(0, chessPrefab_OtherSide.Count);

            if (GetAvailableSlot(out rowIndex, out colIndex)) {
                boardOccupiedStatus[rowIndex, colIndex] = 2; // set the slot occupied

                var initPos = GetTileCenter(colIndex, rowIndex); // get the exact position of that slot
                var chess = Instantiate(chessPrefab_OtherSide[prefabIndex], initPos, Quaternion.identity); // instantiate the chess

                var _controller = chess.GetComponent<ChessController>();
                if (_controller != null) {
                    _controller._chessType = ChessType.OTHER_SIDE;
                    otherSideChessList.Add(_controller);
                    _controller.Position = new Vector2(rowIndex, colIndex);
                }
            } else {
                break;
            }
        }

    }

    private bool GetAvailableSlot(out int rowIndex, out int colIndex) {
        int i = -1, j = -1;
        do {
            i = Random.Range(4, 7);
            j = Random.Range(0, 8);
        } while (boardOccupiedStatus[i, j] != 0);
        
        rowIndex = i;
        colIndex = j;

        if (i == - 1 || j == -1) {
            return false;
        }

        return true;
    }
}
