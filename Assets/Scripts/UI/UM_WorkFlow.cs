using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : IWorkFlowExecuter {

    public void EnterStatus_GameStart() {
        isGameOver = false;
        pan_GameStart.SetActive(true);
        StartTimer(_gameProp.duration_StartStage);
        pan_GameStart.GetComponent<AlphaFading>().AutoFadeInAndFadeOut();
    }

    public void EnterStatus_Preparing() {
        pan_GameStart.SetActive(false);
        pan_Preparing.SetActive(true);
        pan_Purchase.SetActive(true);
        pan_Fighting.SetActive(false);

        txt_RefreshConsumed.text = $"×{_gameProp.refreshConsumed}";

        Refresh_Auto();
        StartTimer(_gameProp.duration_PrepareStage);
    }

    public void EnterStatus_RoundFinished() {
        StartTimer(_gameProp.duration_RoundFinished);
    }

    public void EnterStatus_Fighting() {
        pan_Preparing.SetActive(false);
        pan_Purchase.SetActive(false);
        pan_Fighting.SetActive(true);

        StartTimer(_gameProp.duration_FightStage);
    }

    public void EnterStatus_GameFinished() {
        isGameOver = true;

        pan_GameOver.SetActive(true);
        pan_GameOver.GetComponent<AlphaFading>().AlphaFadeTo(1f);
    }

}
