using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager : MonoBehaviour {
    public GameProp _gameProp;

    [Header("TOP CENTER - Preparation")]
    public GameObject pan_Preparing;

    public Text txt_StageHint;
    public Text txt_TimeRemaining_Prepare;
    public Text txt_RoundNumber_Prepare;

    [Header("PURCHASE")]
    public GameObject pan_Purchase;

    [Header("TOP CENTER - Fighting")]
    public GameObject pan_Fighting;

    public Text txt_TimeRemaining;
    public Text txt_PlayerName;
    public Text txt_EnemyName;
    public Text txt_SelfChessAliveCount;
    public Text txt_OtherChessAliveCount;
    public HealthBarManager bar_SelfChessAlivePercentage;
    public HealthBarManager bar_OtherChessAlivePercentage;
    public Text txt_RoundNumber;

    [Header("BOTTOM LEFT")]
    public Text txt_CurLevel;
    public Text txt_CurTreasureAmout;
    public HealthBarManager bar_ExpInfo;

    private float stageTimeRemaining;

    private void OnEnable() {
        _gameProp.OnChessCountChanged += (self, other) => {
            bar_SelfChessAlivePercentage.UpdateBar(self, _gameProp.battleFieldMaxChessCount);
            bar_OtherChessAlivePercentage.UpdateBar(other, _gameProp.battleFieldMaxChessCount);
        };

        _gameProp.OnExpInfoChanged += (exp, expMax) => {
            bar_ExpInfo.UpdateBar(exp, expMax);
            bar_ExpInfo.barText.text = $"{exp}/{expMax}";
        };
    }

    private void Update() {

        if (txt_CurLevel != null) txt_CurLevel.text = $"Lv.{_gameProp.Level}";
        if (txt_CurTreasureAmout != null) txt_CurTreasureAmout.text = $"coins: {_gameProp.TreasureAmount}";

        if (_gameProp._status == GameProp.GAME_STATUS.Preparing) {
            if (txt_StageHint != null) txt_StageHint.text = $"准备阶段";
            if (txt_TimeRemaining_Prepare != null) { txt_TimeRemaining_Prepare.text = $"{stageTimeRemaining}"; txt_TimeRemaining.fontSize = 35; };
            if (txt_RoundNumber_Prepare != null) txt_RoundNumber_Prepare.text = $"第{_gameProp.RoundNo}回合";
        }

        if (_gameProp._status == GameProp.GAME_STATUS.Fighting) {
            if (txt_SelfChessAliveCount != null) txt_SelfChessAliveCount.text = $"{_gameProp.ChessNo_Self}";
            if (txt_OtherChessAliveCount != null) txt_OtherChessAliveCount.text = $"{_gameProp.ChessNo_Other}";
            if (txt_RoundNumber != null) txt_RoundNumber.text = $"第{_gameProp.RoundNo}回合";

            if (txt_PlayerName != null) txt_PlayerName.text = $"{_gameProp.PlayerName}";
            if (txt_EnemyName != null) txt_EnemyName.text = $"{_gameProp.EnemyName}";

            if (txt_TimeRemaining != null) txt_TimeRemaining.text = $"{stageTimeRemaining}";
        }

        if (_gameProp._status == GameProp.GAME_STATUS.RoundFinished) {
            if (txt_TimeRemaining != null) { txt_TimeRemaining.text = $"回合结束"; txt_TimeRemaining.fontSize = 15; }
        }
    }
    
    private void StartTimer(int duration) {
        StartCoroutine(CountDown(duration));
    }

    IEnumerator CountDown(int duration) {
        while (duration > 0) {
            stageTimeRemaining = duration;
            yield return new WaitForSeconds(1f);
            duration--;
        }

        //if (_gameProp._status == GameProp.GAME_STATUS.Preparing || _gameProp._status == GameProp.GAME_STATUS.Fighting) {
        //}
        GameManager.Instance.OnProcessFinished?.Invoke();
    }
}
