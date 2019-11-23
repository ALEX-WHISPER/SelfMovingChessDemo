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
    public int duration_StartStage = 2;
    public int duration_PrepareStage = 25;
    public int duration_FightStage = 45;
    public int duration_RoundFinished = 2;

    [Header("Rules")]
    public List<int> expMaxInEachLevel;
    public List<int> enemyNumInEachRound;
    public int maxRoundNumber = 5;
    public int expUpInterval = 5;
    public int expUpConsumed = 5;
    public int refreshConsumed = 2;
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

    #region Exposed
    /// <summary>
    /// 己方玩家昵称
    /// </summary>
    public string PlayerName { get { return playerName; } }
    /// <summary>
    /// 敌方玩家昵称
    /// </summary>
    public string EnemyName { get { return enemyNameList[RoundNo-1]; } }
    /// <summary>
    /// 生命值
    /// </summary>
    public int Health { get { return _health.GetValue; } }
    /// <summary>
    /// 当前等级
    /// </summary>
    public int Level { get { return _level.GetValue; } }
    /// <summary>
    /// 当前经验值
    /// </summary>
    public int Exp { get { return _exp.GetValue; } }
    /// <summary>
    /// 当前经验值上限
    /// </summary>
    public int ExpMax { get { return _expMax.GetValue; } }
    /// <summary>
    /// 当前回合数
    /// </summary>
    public int RoundNo { get { return _roundNo.GetValue; } }
    /// <summary>
    /// 金币数量
    /// </summary>
    public int TreasureAmount { get { return _treasureAmount.GetValue; } }
    /// <summary>
    /// 人口数
    /// </summary>
    public int MaxChessNum { get { return _level.GetValue; } }
    /// <summary>
    /// 己方战斗区内存活的棋子数量
    /// </summary>
    public int ChessNo_Self { get { return _chessNo_Self.GetValue; } }
    /// <summary>
    /// 敌方战斗区内存活的棋子数量
    /// </summary>
    public int ChessNo_Other { get { return _chessNo_Other.GetValue; } }
    /// <summary>
    /// 胜利回合数
    /// </summary>
    public int KillCount { get { return _kill.GetValue; } }
    /// <summary>
    /// 失败回合数
    /// </summary>
    public int DefeatCount { get { return _defeat.GetValue; } }
    /// <summary>
    /// 本回合是否胜利
    /// </summary>
    public bool IsThisRoundWin { get { return isRoundWin; } }
    #endregion

    #region Events
    /// <summary>
    /// 更新当前游戏状态
    /// </summary>
    public Action<GAME_STATUS> UpdateGameStatus;

    /// <summary>
    /// 回调: 游戏状态更新
    /// </summary>
    public Action<GAME_STATUS> OnGameStatusUpdated;

    /// <summary>
    /// 回调: 等级提升
    /// </summary>
    public Action OnLevelUp;

    /// <summary>
    /// 回调: 经验值增加
    /// </summary>
    public Action<int> OnExpIncreased;
    /// <summary>
    /// 消耗金币，经验值+5
    /// </summary>
    public Action ExpIncreasedByInterval;
    /// <summary>
    /// 回调: 经验值减少
    /// </summary>
    public Action<int> OnExpDecreased;
    /// <summary>
    /// 回调：[当前exp，exp上限] 变化
    /// </summary>
    public Action<int, int> OnExpInfoChanged;

    /// <summary>
    /// 金币增加
    /// </summary>
    public Action<int> IncreaseTreasure;
    /// <summary>
    /// 金币减少
    /// </summary>
    public Action<int> DecreasedTreasure;

    /// <summary>
    /// 回调: 当前回合结束
    /// </summary>
    public Action OnRoundFinished;
    /// <summary>
    /// 回调: 棋子数量([己方，敌方])发生变化
    /// </summary>
    public Action<int, int> OnChessCountChanged;
    /// <summary>
    /// 回调: 当前回合胜负结果判断完毕
    /// </summary>
    public Action<bool> OnRoundResultConfirmed;
    /// <summary>
    /// 回调：回合胜利
    /// </summary>
    public Action OnRoundWin;
    /// <summary>
    /// 回调：回合失败
    /// </summary>
    public Action<int> OnRoundDefeat;
    /// <summary>
    /// 回调：游戏结束
    /// </summary>
    public Action OnGameOver;

    /// <summary>
    /// 回调：刷新待消耗金币大于当前金币
    /// </summary>
    public Action OnTreasureLackForRefresh;
    /// <summary>
    /// 回调：刷新待消耗金币小于等于当前金币
    /// </summary>
    public Action OnTreasureEnoughForRefresh;
    #endregion

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
            if (!curLv_ExpMax.ContainsKey(i + 1)) {
                curLv_ExpMax.Add(i + 1, expMaxInEachLevel[i]);
            }
        }
        _expMax = new Stat(curLv_ExpMax[_level.GetValue]);

        EventsRegister();
    }

    private void EventsRegister() {
        UpdateGameStatus += (newStatus) => {
            _status = newStatus;
            OnGameStatusUpdated?.Invoke(_status);
        };

        OnGameStatusUpdated += (newStatus) => {
            if (newStatus == GAME_STATUS.RoundFinished) {
                OnRoundFinished?.Invoke();
            }
        };

        // 升级
        OnLevelUp += () => {
            _level.Increase(); // 等级/人口提升

            if (curLv_ExpMax.ContainsKey(_level.GetValue)) {
                _exp.Set(0); // 当前经验值归零
                _expMax.Set(curLv_ExpMax[_level.GetValue]); // 经验上限提升
            }
        };
        
        // 经验 +5
        ExpIncreasedByInterval += () => {
            if (_treasureAmount.GetValue < expUpConsumed) {
                return;
            }
            _exp.Increase(expUpInterval);
            _treasureAmount.Decrease(expUpConsumed);

            // 若当前经验值已满足上限，则升级
            if (_exp.GetValue >= _expMax.GetValue) {
                OnLevelUp?.Invoke();
            }
        };

        // 获取金币
        IncreaseTreasure += (step) => {
            _treasureAmount.Increase(step);

            if (_treasureAmount.GetValue >= refreshConsumed) {
                OnTreasureEnoughForRefresh?.Invoke();
            }
        };

        // 消耗金币
        DecreasedTreasure += (step) => {
            if (_treasureAmount.GetValue < step) {
                return;
            }
            _treasureAmount.Decrease(step);

            if (_treasureAmount.GetValue < refreshConsumed) {
                OnTreasureLackForRefresh?.Invoke();
            }
        };

        // 进入下一轮
        OnRoundFinished += () => {
            if (_roundNo.GetValue >= maxRoundNumber) {
                UpdateGameStatus?.Invoke(GAME_STATUS.GameFinished);
                OnGameOver?.Invoke();
                return;
            }
            if (isRoundWin) {
                OnRoundWin?.Invoke();
            } else {
                OnRoundDefeat?.Invoke(0);
            }

            // 第3轮结束后只增加经验，等级不自动提升
            if (RoundNo < 3) {
                OnLevelUp?.Invoke();
            } else {
                OnExpIncreased?.Invoke(1);
            }

            // 金币奖励：每回合基础收入
            int increased = RoundNo <= 3 ? RoundNo + 1 : 5;
            IncreaseTreasure?.Invoke(increased);

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
                OnGameOver?.Invoke();
            }
        };

        // 游戏结束
        OnGameOver += () => {
            // ...
        };
    }
}
