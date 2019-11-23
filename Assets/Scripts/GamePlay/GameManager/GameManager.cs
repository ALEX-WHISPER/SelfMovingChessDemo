using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public partial class GameManager: SingletonBase<GameManager> {

    [SerializeField]
    private GameProp _gameProp;
    private BoardManager _boardManager;
    private UIManager _uiManager;

    public Action OnProcessFinished;
    public Action<bool> OnRoundFinished;
    public Action<bool> OnRoundResultConfirmed;
    
    private bool isGameOver = false;
    private ChessController _toBeSelled;

    void Awake() {
        _boardManager = GameObject.FindWithTag("GameBoard").GetComponent<BoardManager>();
        _uiManager = GetComponent<UIManager>();
        _gameProp.Init();

        //DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() {
        StatusControl();

        // 棋子数量发生变化
        _boardManager.OnChessListChanged += (selfCount, otherCount) => {
            _gameProp.OnChessCountChanged?.Invoke(selfCount, otherCount);
        };

        _gameProp.OnRoundResultConfirmed += (isWin) => { OnRoundResultConfirmed?.Invoke(isWin); };

        // 本局胜
        _gameProp.OnRoundWin += () => {
            Debug.Log($"Round {_gameProp.RoundNo}: WIN");
            OnRoundFinished?.Invoke(true);
        };

        // 本局败
        _gameProp.OnRoundDefeat += (step) => {
            Debug.Log($"Round {_gameProp.RoundNo}: DEFEAT");
            OnRoundFinished?.Invoke(false);
        };

        // 游戏结束
        _gameProp.OnGameOver += () => {
            Debug.Log($"GameOver");
            EnterStatus_GameFinished();
        };
    }

    private void Start() {
        EnterStatus_GameStart();
    }

    private void Update() {
        if (isGameOver) {
            if (Input.GetKeyDown(KeyCode.R)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
            }

            return;
        }

        //if (Input.GetKeyDown(KeyCode.F2)) {
        //    _gameProp._status = GameProp.GAME_STATUS.GAME_START;
        //    OnProcessFinished?.Invoke();
        //}

        if (Input.GetKeyDown(KeyCode.F12)) {
            _gameProp.IncreaseTreasure?.Invoke(100);
        }
    }

    private void StatusControl() {
        OnProcessFinished += () => {
            switch(_gameProp._status) {
                case GameProp.GAME_STATUS.GAME_START:
                    EnterStatus_Preparing();
                    break;
                case GameProp.GAME_STATUS.Preparing:
                    EnterStatus_Fighting();
                    break;
                case GameProp.GAME_STATUS.Fighting:
                    EnterStatus_RoundFinished();
                    break;
                case GameProp.GAME_STATUS.RoundFinished:
                    EnterStatus_Preparing();
                    break;
                case GameProp.GAME_STATUS.GameFinished:
                    EnterStatus_GameFinished();
                    break;
                default:
                    break;
            }
        };
    }

    /// <summary>
    /// 购买棋子，并将其添加至备战区
    /// </summary>
    /// <param name="prop"> 购买的棋子 </param>
    /// <returns></returns>
    public bool PurchaseChessToBackup(ChessProp prop) {
        if (prop.cost.GetValue > _gameProp.TreasureAmount) {
            Debug.Log("Out of money!");
            return false;
        }

        if (_boardManager.GetBackupFieldList.Count >= _gameProp.backupFieldMaxSlot) {
            Debug.Log("slots are full!");
            return false;
        }

        _boardManager.SetNewChessToBackupField(prop);
        _gameProp.DecreasedTreasure(prop.cost.GetValue);

        return true;
    }

    /// <summary>
    /// 选中棋子，准备出售
    /// </summary>
    /// <param name="chess"> 预备出售的棋子 </param>
    public void SelectChessToSell(ChessController chess) {
        if (chess == null) {
            return;
        }

        _toBeSelled = chess;
        _uiManager.OnSelectedSellChess(_toBeSelled.propTemplate.cost.GetValue);
    }

    /// <summary>
    /// 对预备出售的棋子撤销选中
    /// </summary>
    public void DeSelectChessToSell() {
        //_toBeSelled = null;
        _uiManager.OnDeSelectedSellChess();
    }

    /// <summary>
    /// 出售选中的棋子
    /// </summary>
    public void SellSelectedChess() {
        if (_toBeSelled == null) {
            return;
        }
        // increase treasure
        var prop = _toBeSelled.propTemplate;
        _gameProp.IncreaseTreasure?.Invoke(prop.cost.GetValue);

        // destroy the chess and reset the board position
        _boardManager.DestroyBackupFieldChess(_toBeSelled);

        _toBeSelled = null;
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

        if (!(m1 >= 0 && m2 >= 0)) {
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
