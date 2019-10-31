using UnityEngine;

public partial class BoardManager: IWorkFlowExecuter {
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

        // recycling the survived self-side chess to backup field
        ResetSurvivedChess();

        selfSideChessList.Clear();
        otherSideChessList.Clear();
    }

    public void EnterStatus_RoundFinished() {

    }

    public void EnterStatus_Fighting() {
        // spawn enemy chess
        SpawnEnemyChess();

        // disable movability for those chess in the backup field
        isChessMovable = false;

        // disable draggable component of each self-side chess
        foreach (Transform child in chessHolder_SelfSide.transform) {
            var dragComponent = child.GetComponent<Draggable>();
            if (dragComponent != null) {
                dragComponent.IsDraggable = false;
            }
        }
    }

    public void EnterStatus_GameFinished() {

    }
}
