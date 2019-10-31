using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameProp", menuName = "Game Properties Containter")]
public class GameProp : ScriptableObject {

    public enum GAME_STATUS { GAME_START, Preparing, Fighting, RoundFinished, GameFinished }

    [Header("Game Status")]
    public GAME_STATUS _status;

    [Header("Base values")]
    public int baseVal_Health = 80;
    public int baseVal_Level = 1;
    public int baseVal_Exp = 0;
    public int baseVal_RoundNo = 1;
    public int baseVal_TreasureAmount;
    public string playerName;
    public List<string> enemyNameList;

    public int battleFieldMaxChessCount = 3;
    public int backupFieldMaxSlot = 8;
    public int duration_PrepareStage = 25;
    public int duration_FightStage = 45;
    public int duration_RoundFinished = 2;

    [Header("Rules")]
    public List<int> expMaxInEachLevel;
    public int expUpInterval = 5;
    private Dictionary<int, int> curLv_ExpMax = new Dictionary<int, int>();

    [Header("Properties")]
    [SerializeField] private Stat _health;
    [SerializeField] private Stat _level;
    [SerializeField] private Stat _exp;
    [SerializeField] private Stat _expMax;
    [SerializeField] private Stat _roundNo;
    [SerializeField] private Stat _treasureAmount;
    [SerializeField] private Stat _chessNo_Self;
    [SerializeField] private Stat _chessNo_Other;
    [SerializeField] private Stat _kill;
    [SerializeField] private Stat _defeat;

    // Exposed
    public string PlayerName { get { return playerName; } }
    public string EnemyName { get { return enemyNameList[RoundNo-1]; } }
    public int Health { get { return _health.GetValue; } }
    public int Level { get { return _level.GetValue; } }
    public int Exp { get { return _exp.GetValue; } }
    public int ExpMax { get { return _expMax.GetValue; } }
    public int RoundNo { get { return _roundNo.GetValue; } }
    public int TreasureAmount { get { return _treasureAmount.GetValue; } } 
    public int MaxChessNum { get { return _level.GetValue; } }
    public int ChessNo_Self { get { return _chessNo_Self.GetValue; } }
    public int ChessNo_Other { get { return _chessNo_Other.GetValue; } }
    public int KillCount { get { return _kill.GetValue; } }
    public int DefeatCount { get { return _defeat.GetValue; } }
    public bool IsThisRoundWin { get { return isRoundWin; } }

    // Events
    public Action<GAME_STATUS> UpdateGameStatus;
    public Action<GAME_STATUS> OnGameStatusUpdated;
    public Action OnLevelUp;
    public Action<int> OnExpIncreased;
    public Action OnExpIncreasedByInterval;
    public Action<int> OnExpDecreased;
    public Action<int, int> OnExpInfoChanged;
    public Action<int> IncreaseTreasure;
    public Action<int> DecreasedTreasure;
    public Action OnRoundFinished;
    public Action<int, int> OnChessCountChanged;
    public Action<bool> OnRoundResultConfirmed;
    public Action OnRoundWin;
    public Action<int> OnRoundDefeat;
    public Action<bool> OnGameOver;

    private bool isRoundWin = false;

    public void Init() {
        _status = GAME_STATUS.GAME_START;

        _health = new Stat(baseVal_Health);
        _level = new Stat(baseVal_Level);
        _exp = new Stat(baseVal_Exp);
        _roundNo = new Stat(baseVal_RoundNo);
        _treasureAmount = new Stat(baseVal_TreasureAmount);
        
        _chessNo_Self = new Stat(0);
        _chessNo_Other = new Stat(0);
        _kill = new Stat(0);
        _defeat = new Stat(0);

        for (int i = 0; i < expMaxInEachLevel.Count; i++) {
            curLv_ExpMax.Add(i + 1, expMaxInEachLevel[i]);
        }
        
        EventsRegister();

        OnExpInfoChanged?.Invoke(_exp.GetValue, _expMax.GetValue);
    }

    private void EventsRegister() {
        UpdateGameStatus += (newStatus) => {
            _status = newStatus;
            OnGameStatusUpdated?.Invoke(_status);
        };

        // 升级
        OnLevelUp += () => {
            _level.Increase(); // 等级/人口提升

            _exp.Set(0); // 当前经验值归零
            _expMax.Set(curLv_ExpMax[_level.GetValue + 1]); // 经验上限提升

            OnExpInfoChanged?.Invoke(_exp.GetValue, _expMax.GetValue);
        };
        
        // 经验 +5
        OnExpIncreasedByInterval += () => {
            _exp.Increase(5);
            _treasureAmount.Decrease(5);

            // 若当前经验值已满足上限，则升级
            if (_exp.GetValue >= _expMax.GetValue) {
                OnLevelUp?.Invoke();
            }

            OnExpInfoChanged?.Invoke(_exp.GetValue, _expMax.GetValue);
        };

        // 获取金币
        IncreaseTreasure += (step) => {
            _treasureAmount.Increase(step);
        };

        // 消耗金币
        DecreasedTreasure += (step) => {
            if (_treasureAmount.GetValue < step) {
                return;
            }
            _treasureAmount.Decrease(step);
        };

        // 进入下一轮
        OnRoundFinished += () => {

            if (isRoundWin) {
                OnRoundWin?.Invoke();
            } else {
                OnRoundDefeat?.Invoke(0);
            }

            // 第3轮结束后只增加经验，等级不自动提升
            if (RoundNo != 3) {
                OnLevelUp?.Invoke();
            } else {
                OnExpIncreased?.Invoke(1);
            }

            // 金币奖励：每回合基础收入(当前回合数+1)
            _treasureAmount.Increase(RoundNo + 1);

            // 回合数 +1
            _roundNo.Increase();
        };

        // 棋子数量变化
        OnChessCountChanged += (selfChessCount, otherChessCount) => {
            _chessNo_Self.Set(selfChessCount);
            _chessNo_Other.Set(otherChessCount);
            
            // 己方棋子被团灭，本局败
            if (_chessNo_Self.GetValue <= 0 && _chessNo_Other.GetValue > 0 && _status == GAME_STATUS.Fighting) {
                isRoundWin = false;
                OnRoundResultConfirmed?.Invoke(false);
            }

            // 敌方棋子被团灭，本局胜
            if (_chessNo_Other.GetValue <= 0 && _chessNo_Self.GetValue > 0 && _status == GAME_STATUS.Fighting) {
                isRoundWin = true;
                OnRoundResultConfirmed?.Invoke(true);
            }
        };
        
        // 本局战胜
        OnRoundWin += () => {
            if (RoundNo > 3) {
                // 金币奖励：获胜奖励 +1
                _treasureAmount.Increase();
            }
        };

        // 本局战败
        OnRoundDefeat += (step) => {
            _health.Decrease(step); // 血量扣除量：敌方剩余单位等级数之和

            if (_health.GetValue <= 0) {
                OnGameOver?.Invoke(false);
            }
        };

        // 游戏结束
        OnGameOver += (isWin) => {
            // ...
        };
    }
}
