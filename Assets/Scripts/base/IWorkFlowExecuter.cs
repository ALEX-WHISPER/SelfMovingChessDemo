
public interface IWorkFlowExecuter {
    void EnterStatus_GameStart();
    void EnterStatus_Preparing();
    void EnterStatus_RoundFinished();
    void EnterStatus_Fighting();
    void EnterStatus_GameFinished();
}
