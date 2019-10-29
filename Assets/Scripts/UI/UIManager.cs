using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public GameProp _gameProp;

    [Header("TOP CENTER")]
    public Text txt_TimeRemainning;
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

    private void OnEnable() {
        _gameProp.OnChessCountChanged += (self, other) => {
            bar_SelfChessAlivePercentage.UpdateBar(self, _gameProp.battleFieldMaxChessCount);
            bar_OtherChessAlivePercentage.UpdateBar(other, _gameProp.battleFieldMaxChessCount);
        };
    }

    private void Update() {
        txt_SelfChessAliveCount.text = $"{_gameProp.ChessNo_Self}";
        txt_OtherChessAliveCount.text = $"{_gameProp.ChessNo_Other}";
        txt_RoundNumber.text = $"{_gameProp.RoundNo}";
        txt_CurLevel.text = $"Lv.{_gameProp.Level}";
        txt_CurTreasureAmout.text = $"coins: {_gameProp.TreasureAmount}";

        txt_PlayerName.text = $"{_gameProp.PlayerName}";
        txt_EnemyName.text = $"{_gameProp.EnemyName}";
    }

    public void StartTimer(int duration) {
        CountDown(duration);
    }

    IEnumerator CountDown(int duration) {
        while (duration > 0) {
            yield return new WaitForSeconds(1f);

            txt_TimeRemainning.text = $"{duration}";
            duration--;
        }

        if (_gameProp._status == GameProp.GAME_STATUS.Preparing || _gameProp._status == GameProp.GAME_STATUS.Fighting) {
            GameManager.Instance.OnProcessFinished?.Invoke();
        }
    }
}
