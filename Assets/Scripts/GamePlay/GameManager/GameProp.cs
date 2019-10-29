using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameProp", menuName = "Game Properties Containter")]
public class GameProp : ScriptableObject {

    public enum GAME_STATUS { GAME_START, Preparing, Fighting, RoundFinished, GameFinished }

    [Header("Game State")]
    public GAME_STATUS _status;

    [Header("Base values")]
    public int baseVal_Health = 80;
    public int baseVal_Level = 1;
    public int baseVal_Exp = 0;
    public int baseVal_RoundNo = 1;
    public int baseVal_TreasureAmount;
    public string playerName;
    public string enemyName;

    public int battleFieldMaxChessCount = 3;
    public int backupFieldMaxSlot = 8;
    public int duration_PrepareStage = 25;
    public int duration_FightStage = 45;

    [Header("Rules")]
    public List<int> expTotalInEachLevel;
    private Dictionary<int, int> curLv_TotalExp;

    [Header("Properties")]
    [SerializeField] private Stat _health;
    [SerializeField] private Stat _level;
    [SerializeField] private Stat _exp;
    [SerializeField] private Stat _roundNo;
    [SerializeField] private Stat _treasureAmount;
    [SerializeField] private Stat _chessNo_Self;
    [SerializeField] private Stat _chessNo_Other;
    [SerializeField] private Stat _kill;
    [SerializeField] private Stat _defeat;

    // Exposed
    public string PlayerName { get { return playerName; } }
    public string EnemyName { get { return enemyName; } }
    public int Health { get { return _health.GetValue; } }
    public int Level { get { return _level.GetValue; } }
    public int Exp { get { return _exp.GetValue; } }
    public int RoundNo { get { return _roundNo.GetValue; } }
    public int TreasureAmount { get { return _treasureAmount.GetValue; } } 
    public int MaxChessNum { get { return _level.GetValue; } }
    public int ChessNo_Self { get { return _chessNo_Self.GetValue; } }
    public int ChessNo_Other { get { return _chessNo_Other.GetValue; } }
    public int KillCount { get { return _kill.GetValue; } }
    public int DefeatCount { get { return _defeat.GetValue; } }

    // Events
    public Action<GAME_STATUS> OnGameStatusChanged;
    public Action OnLevelUp;
    public Action<int> OnExpIncreased;
    public Action<int> OnExpDecreased;
    public Action<int> OnTreasureIncreased;
    public Action<int> OnTreasureDecreased;
    public Action OnEnterNextRound;
    public Action<int, int> OnChessCountChanged;
    public Action OnRoundWin;
    public Action<int> OnRoundDefeat;
    public Action<bool> OnGameOver;

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

        EventsRegister();
    }

    private void EventsRegister() {
        OnGameStatusChanged += (newStatus) => {
            _status = newStatus;
        };

        // 升级
        OnLevelUp += () => {
            _level.Increase(); // 等级/人口提升
        };

        // 经验提升
        OnExpIncreased += (step) => {
            _exp.Increase(step);

            // ...
        };

        // 获取金币
        OnTreasureIncreased += (step) => {
            _treasureAmount.Increase(step);
        };

        // 消耗金币
        OnTreasureDecreased += (step) => {
            if (_treasureAmount.GetValue < step) {
                return;
            }
            _treasureAmount.Decrease(step);
        };

        // 进入下一轮
        OnEnterNextRound += () => {
            _exp.Increase(); // 经验值 +1

            // 金币奖励：每回合基础收入
            // ...
        };

        // 棋子数量变化
        OnChessCountChanged += (selfChessCount, otherChessCount) => {
            _chessNo_Self.Set(selfChessCount);
            _chessNo_Other.Set(otherChessCount);
            
            // 己方棋子被团灭，本局败
            if (_chessNo_Self.GetValue <= 0 && _status != GAME_STATUS.GAME_START) {
                OnRoundDefeat?.Invoke(0);
            }

            // 敌方棋子被团灭，本局胜
            if (_chessNo_Other.GetValue <= 0 && _status != GAME_STATUS.GAME_START) {
                OnRoundWin?.Invoke();
            }
        };
        
        // 本局战胜
        OnRoundWin += () => {

            // 金币奖励：获胜奖励
            // ...
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
