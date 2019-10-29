using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class GameManager: SingletonBase<GameManager> {

    [SerializeField]
    private GameProp _gameProp;
    private BoardManager _boardManager;
    private UIManager _uiManager;
    private bool isBonded = false;

    public Action OnProcessFinished;
    public Action OnPreparationProceeded;
    public Action OnPreparationFinished;
    public Action<bool> OnRoundFinished;

    void Awake() {
        _boardManager = GameObject.FindWithTag("GameBoard").GetComponent<BoardManager>();
        _uiManager = GetComponent<UIManager>();
        _gameProp.Init();
    }

    private void OnEnable() {
        StatusControl();

        // 棋子数量发生变化
        _boardManager.OnChessListChanged += (selfCount, otherCount) => {
            _gameProp.OnChessCountChanged?.Invoke(selfCount, otherCount);
        };

        // 本局胜
        _gameProp.OnRoundWin += () => {
            OnRoundFinished?.Invoke(true);
            Debug.Log($"Round {_gameProp.RoundNo}: WIN");
        };

        // 本局败
        _gameProp.OnRoundDefeat += (step) => {
            OnRoundFinished?.Invoke(false);
            Debug.Log($"Round {_gameProp.RoundNo}: DEFEAT");
        };

        // 游戏结束
        _gameProp.OnGameOver += (isWin) => {
            Debug.Log($"GameOver: {isWin}");
        };
    }

    private void StatusControl() {
        OnProcessFinished += () => {
            if (_gameProp._status == GameProp.GAME_STATUS.Preparing) {
                // Enter fight stage...
                _gameProp._status = GameProp.GAME_STATUS.Fighting;
                EnterStatus_Preparing();
            }

            if (_gameProp._status == GameProp.GAME_STATUS.Fighting) {
                // Enter round finished stage...
                _gameProp._status = GameProp.GAME_STATUS.RoundFinished;
            }

            if (_gameProp._status == GameProp.GAME_STATUS.RoundFinished) {
                // Enter preparation stage...
                _gameProp._status = GameProp.GAME_STATUS.Preparing;
            }
        };
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && !isBonded) {
            BindingFocus();
            isBonded = true;
        }
    }

    public bool PurchaseChessToBackup(ChessProp prop) {
        if (prop.cost.GetValue > _gameProp.TreasureAmount) {
            Debug.Log("Out of money!");
            return false;
        }

        _boardManager.SetNewChessToBackupField(prop);
        _gameProp.OnTreasureDecreased(prop.cost.GetValue);

        return true;
    }

    #region AI 战斗关系绑定
    public void BindingFocus() {
        if (_boardManager == null) {
            return;
        }

        var l1 = _boardManager.GetChessList_SelfSide;
        var l2 = _boardManager.GetChessList_OtherSide;

        int m1 = l1.Count - 1;
        int m2 = l2.Count - 1;

        if (!(m1 > 0 && m2 > 0)) {
            return;
        }

        while (m1 >= 0 && m2 >= 0) {
            var _selfChess = SelectNonRepeatedChessRandomly(l1, ref m1);
            var _otherChess = SelectNonRepeatedChessRandomly(l2, ref m2);
            PairingFocus(_selfChess, _otherChess);
        }

        while (m1 >= 0) {
            var _selfChess = SelectNonRepeatedChessRandomly(l1, ref m1);
            var _otherChess = l2[UnityEngine.Random.Range(0, l2.Count)];
            _selfChess.SetFocus(_otherChess);
        }

        while (m2 >= 0) {
            var _otherChess = SelectNonRepeatedChessRandomly(l2, ref m2);
            var _selfChess = l1[UnityEngine.Random.Range(0, l1.Count)];
            _otherChess.SetFocus(_selfChess);
        }
    }

    private ChessController SelectNonRepeatedChessRandomly(List<ChessController> list, ref int max) {
        var r = UnityEngine.Random.Range(0, max + 1);
        var selectedElem = list[r];

        var temp = list[r];
        list[r] = list[max];
        list[max] = temp;

        max--;

        return selectedElem;
    }

    private void PairingFocus(ChessController self, ChessController other) {
        self.SetFocus(other);
        other.SetFocus(self);
    }
    #endregion
}
