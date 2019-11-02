using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class BoardManager: MonoBehaviour, IWorkFlowExecuter {
    public void EnterStatus_GameStart() {

    }

    public void EnterStatus_Preparing() {
        // enable chess movability
        isChessMovable = true;

        // reset battle field's slot status
        for (int i = 0; i < boardOccupiedStatus.GetLength(0); i++) {
            for (int j = 1; j < boardOccupiedStatus.GetLength(1); j++) {
                boardOccupiedStatus[i, j] = 0;
            }
        }

        // destroy the other-side chess in battle field
        foreach (Transform child in chessHolder_OtherSide.transform) {
            Destroy(child.gameObject);
        }

        // destroy the dead self-side chess in the battle field
        foreach (Transform child in chessHolder_SelfSide.transform) {
            var chess = child.GetComponent<ChessController>();
            
            if (chess == null || (!battleFieldChess_Self.Contains(chess) && !backupFieldChessList.Contains(chess))) {
                Destroy(child.gameObject);
            }
        }
        // recycling the survived self-side chess to backup field
        ResetSurvivedChess();

        battleFieldChess_Self.Clear();
        battleFieldChess_Other.Clear();
    }

    public void EnterStatus_RoundFinished() {

    }

    public void EnterStatus_Fighting() {
        // spawn enemy chess
        SpawnEnemyChess();

        // spawn self chess if battle field is empty
        AutoMarchCheck();

        // disable movability for those chess in the backup field
        isChessMovable = false;

        // disable draggable component of each self-side chess
        foreach (Transform child in chessHolder_SelfSide.transform) {
            var dragComponent = child.GetComponent<Draggable>();
            if (dragComponent != null) {
                dragComponent.IsDraggable = false;
            }
            child.GetComponent<ChessController>().ResetChess();
        }
    }

    public void EnterStatus_GameFinished() {
        isChessMovable = false;
        isBoardInteractable = false;
    }

    private void AutoMarchCheck() {
        isChessMovable = true;

        if (_gameProp._status != GameProp.GAME_STATUS.Fighting || battleFieldChess_Self.Count >= _gameProp.MaxChessNum) {
            return;
        }

        for (int i = 0; i < _gameProp.MaxChessNum; i++) {
            if (backupFieldChessList.Count <= 0) {
                return;
            }

            var marchChess = backupFieldChessList[0];
            if (marchChess == null) {
                break;
            }

            int rowIndex, colIndex;
            if (GetBattleFieldAvailableSlot_Self(out rowIndex, out colIndex)) {
                var newLocation = GetTileCenter(rowIndex, colIndex);
                marchChess.transform.position = newLocation;

                MoveChess(marchChess, marchChess.Position, new Vector2(newLocation.x, newLocation.z));
            }
        }
    }
}
