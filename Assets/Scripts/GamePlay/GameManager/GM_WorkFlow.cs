
public partial class GameManager: SingletonBase<GameManager> {

    private void EnterStatus_GameStart() {

    }

    private void EnterStatus_Preparing() {
        _uiManager.StartTimer(_gameProp.duration_PrepareStage);

        _gameProp.OnGameStatusChanged?.Invoke(GameProp.GAME_STATUS.Preparing);
        OnPreparationProceeded?.Invoke();
    }

    private void EnterStatus_RoundFinished() {

    }

    private void EnterStatus_Fighting() {

    }

    private void EnterStatus_GameFinished() {

    }
}
