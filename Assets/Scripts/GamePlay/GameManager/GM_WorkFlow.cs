
public partial class GameManager: SingletonBase<GameManager>, IWorkFlowExecuter {

    // 游戏开始
    public void EnterStatus_GameStart() {
        _gameProp.UpdateGameStatus?.Invoke(GameProp.GAME_STATUS.GAME_START);

        _uiManager.EnterStatus_GameStart();
        _boardManager.EnterStatus_GameStart();
    }

    // 准备阶段
    public void EnterStatus_Preparing() {
        _gameProp.UpdateGameStatus?.Invoke(GameProp.GAME_STATUS.Preparing);

        _uiManager.EnterStatus_Preparing();
        _boardManager.EnterStatus_Preparing();
    }

    // 战斗阶段
    public void EnterStatus_Fighting() {
        _uiManager.EnterStatus_Fighting();
        _boardManager.EnterStatus_Fighting();

        BindingFocus(); // AI 配对
        _gameProp.UpdateGameStatus?.Invoke(GameProp.GAME_STATUS.Fighting);
    }

    // 回合结束
    public void EnterStatus_RoundFinished() {
        _gameProp.UpdateGameStatus?.Invoke(GameProp.GAME_STATUS.RoundFinished);
        _gameProp.OnRoundFinished?.Invoke();

        _uiManager.EnterStatus_RoundFinished();
        _boardManager.EnterStatus_RoundFinished();
    }

    // 游戏结束
    public void EnterStatus_GameFinished() {
        isGameOver = true;
        _gameProp.UpdateGameStatus?.Invoke(GameProp.GAME_STATUS.GameFinished);

        _uiManager.EnterStatus_GameFinished();
        _boardManager.EnterStatus_GameFinished();
    }
}
