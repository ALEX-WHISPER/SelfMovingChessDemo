using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager : MonoBehaviour {
    public GameProp _gameProp;
    public GameObject pan_GameOver;
    public GameObject pan_GameStart;

    [Header("TOP CENTER - Preparation")]
    public GameObject pan_Preparing;

    public Text txt_StageHint;
    public Text txt_TimeRemaining_Prepare;
    public Text txt_RoundNumber_Prepare;

    [Header("PURCHASE")]
    public GameObject pan_Purchase;
    public List<PurchaseSlot> purSlotList;
    public Text txt_RefreshConsumed;
    private bool isPurchaseLocked = false;

    public Button btn_PurchasePanActivate;
    public Button btn_LockPurchase;
    public Sprite sprite_Locked;
    public Sprite sprite_Unlocked;

    public Button btn_RefreshPurchase;
    public Sprite sprite_AbleRefreshing;
    public Sprite sprite_UnableRefreshing;
    private bool isTreasureEnoughForRefresh = false;

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

    [Header("TOP CENTER - Round Finished")]
    public GameObject pan_RoundResult;
    public Sprite img_RoundWin;
    public Sprite img_RoundDefeat;

    [Header("BOTTOM LEFT")]
    public Text txt_CurLevel;
    public Text txt_CurTreasureAmout;
    public HealthBarManager bar_ExpInfo;
    public Text txt_ExpInfo;
    public Text txt_ExpUpConsumed;
    public Button btn_ExpUp;

    private float stageTimeRemaining;
    private bool isGameOver = false;

    private void OnEnable() {
        _gameProp.OnChessCountChanged += (self, other) => {
            bar_SelfChessAlivePercentage.UpdateBar(self, _gameProp.battleFieldMaxChessCount);
            bar_OtherChessAlivePercentage.UpdateBar(other, _gameProp.battleFieldMaxChessCount);
        };

        _gameProp.OnExpInfoChanged += (exp, expMax) => {
            bar_ExpInfo.UpdateBar(exp, expMax);
            bar_ExpInfo.barText.text = $"{exp}/{expMax}";
        };

        _gameProp.OnGameStatusUpdated += (status) => {
            if (status == GameProp.GAME_STATUS.Preparing) {
                EnablePurchasing();
                if (pan_RoundResult != null) pan_RoundResult.SetActive(false);

            } else {
                if (status == GameProp.GAME_STATUS.RoundFinished) {

                    if (_gameProp.IsThisRoundWin) {
                        pan_RoundResult.GetComponent<Image>().sprite = img_RoundWin;
                    } else {
                        pan_RoundResult.GetComponent<Image>().sprite = img_RoundDefeat;
                    }

                    pan_RoundResult.SetActive(true);
                    pan_RoundResult.GetComponent<AlphaFading>().AutoFadeInAndFadeOut();
                }
                DisablePurchasing();
            }
        };

        _gameProp.OnTreasureEnoughForRefresh += () => {
            if (txt_RefreshConsumed != null) {
                txt_RefreshConsumed.color = Color.white;
            }
            isTreasureEnoughForRefresh = true;
        };

        _gameProp.OnTreasureLackForRefresh += () => {
            if (txt_RefreshConsumed != null) {
                txt_RefreshConsumed.color = Color.red;
            }
            isTreasureEnoughForRefresh = false;
        };

        // open/close purchase panel
        btn_PurchasePanActivate.onClick.AddListener(()=> {
            PurchasePanelActivation();
        });

        // lock
        btn_LockPurchase.onClick.AddListener(()=> {
            LockPurchase();
        });

        // refresh
        btn_RefreshPurchase.onClick.AddListener(()=> {
            Refresh_Manually();
        });

        // exp up
        btn_ExpUp.onClick.AddListener(()=> {
            _gameProp.ExpIncreasedByInterval?.Invoke();
        });
    }

    private void Start() {
        pan_GameOver.SetActive(false);

        if (_gameProp.TreasureAmount < _gameProp.refreshConsumed) {
            _gameProp.OnTreasureLackForRefresh?.Invoke();
        } else {
            _gameProp.OnTreasureEnoughForRefresh?.Invoke();
        }
        txt_ExpUpConsumed.text = $"×{_gameProp.expUpConsumed}";
    }

    private void Update() {
        if (isGameOver) {
            return;
        }

        if (txt_CurLevel != null) txt_CurLevel.text = $"Lv.{_gameProp.Level}";
        if (txt_CurTreasureAmout != null) txt_CurTreasureAmout.text = $"{_gameProp.TreasureAmount}";
        if (txt_ExpInfo != null) txt_ExpInfo.text = $"{_gameProp.Exp}/{_gameProp.ExpMax}";

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
        //    GameManager.Instance.OnProcessFinished?.Invoke();
        //}

        //if (_gameProp._status == GameProp.GAME_STATUS.RoundFinished) {
        //    if (_gameProp.RoundNo >= _gameProp.maxRoundNumber) {
        //        _gameProp.UpdateGameStatus?.Invoke(GameProp.GAME_STATUS.GameFinished);
        //    }
        //    GameManager.Instance.OnProcessFinished?.Invoke();
        //}
        GameManager.Instance.OnProcessFinished?.Invoke();
    }

    #region Purchase
    private void EnablePurchasing() {
        foreach (var slot in purSlotList) {
            slot.AllowPurchase(true);
        }
    }

    private void DisablePurchasing() {
        foreach (var slot in purSlotList) {
            slot.AllowPurchase(false);
        }
    }

    private void PurchasePanelActivation() {
        if (pan_Purchase.activeSelf) {
            pan_Purchase.SetActive(false);
        } else {
            pan_Purchase.SetActive(true);
        }
    }
    
    private void Refresh_Manually() {
        if (isPurchaseLocked || !isTreasureEnoughForRefresh) {
            return;
        }

        foreach (var slot in purSlotList) {
            slot.SlotRefresh();
        }

        _gameProp.DecreasedTreasure?.Invoke(_gameProp.refreshConsumed);
    }

    private void Refresh_Auto() {
        if (isPurchaseLocked) {
            return;
        }
        foreach (var slot in purSlotList) {
            slot.SlotRefresh();
        }
    }

    private void LockPurchase() {

        // isLocked
        isPurchaseLocked = !isPurchaseLocked;

        // change lock image
        btn_LockPurchase.image.sprite = isPurchaseLocked ? sprite_Locked : sprite_Unlocked;

        // change refresh status and image
        if (isPurchaseLocked) {
            btn_RefreshPurchase.enabled = false;
            btn_RefreshPurchase.image.sprite = sprite_UnableRefreshing;
        } else {
            btn_RefreshPurchase.enabled = true;
            btn_RefreshPurchase.image.sprite = sprite_AbleRefreshing;
        }
    }
    #endregion
}
