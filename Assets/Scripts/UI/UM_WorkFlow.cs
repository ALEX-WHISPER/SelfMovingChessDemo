using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : IWorkFlowExecuter {

    public void EnterStatus_GameStart() {

    }

    public void EnterStatus_Preparing() {
        StartTimer(_gameProp.duration_PrepareStage);
        pan_Purchase.SetActive(true);
    }

    public void EnterStatus_RoundFinished() {

    }

    public void EnterStatus_Fighting() {
        StartTimer(_gameProp.duration_FightStage);
        pan_Purchase.SetActive(false);
    }

    public void EnterStatus_GameFinished() {

    }
}
