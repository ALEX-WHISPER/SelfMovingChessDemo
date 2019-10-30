
public partial class GameManager: SingletonBase<GameManager>, IWorkFlowExecuter {

    // 游戏开始
    public void EnterStatus_GameStart() {
        _gameProp.OnGameStatusChanged?.Invoke(GameProp.GAME_STATUS.GAME_START);

        _uiManager.EnterStatus_GameStart();
        _boardManager.EnterStatus_GameStart();
    }

    // 准备阶段
    public void EnterStatus_Preparing() {
        _gameProp.OnGameStatusChanged?.Invoke(GameProp.GAME_STATUS.Preparing);

        _uiManager.EnterStatus_Preparing();
        _boardManager.EnterStatus_Preparing();
    }

    // 战斗阶段
    public void EnterStatus_Fighting() {
        _gameProp.OnGameStatusChanged?.Invoke(GameProp.GAME_STATUS.Fighting);

        _uiManager.EnterStatus_Fighting();
        _boardManager.EnterStatus_Fighting();

        BindingFocus(); // AI 配对
    }

    // 回合结束
    public void EnterStatus_RoundFinished() {
        _gameProp.OnGameStatusChanged?.Invoke(GameProp.GAME_STATUS.RoundFinished);

        _uiManager.EnterStatus_RoundFinished();
        _boardManager.EnterStatus_RoundFinished();
    }

    // 游戏结束
    public void EnterStatus_GameFinished() {
        _gameProp.OnGameStatusChanged?.Invoke(GameProp.GAME_STATUS.GameFinished);

        _uiManager.EnterStatus_GameFinished();
        _boardManager.EnterStatus_GameFinished();
    }
}
