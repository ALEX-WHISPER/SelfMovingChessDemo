using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
//cc6733 9DBE55
public enum ChessCamp { SELF_SIDE, OTHER_SIDE }
public enum ChessType { NONE, SWORD, AXE, KNIGHT, MUTANT, ARCHER }

[RequireComponent(typeof(ChessMotor))]
public class ChessController : MonoBehaviour {
    
    public ChessProp propTemplate;
    private ChessProp chessProp;
    
    public Vector3 radiusOffset = Vector3.zero;
    public float attackDelay = 0.5f;
    
    public bool isPlayer = false;
    public LayerMask layer_Ground;
    public LayerMask layer_Enemy;

    private Color healthBarColor_Self = new Color(0x9d, 0xbe, 0x55);
    private Color healthBarColor_Other = new Color(0xcc, 0x67, 0x33);

    private ChessMotor _motor;
    private BoardManager _boardManager;
    private AnimManager _anim;

    private ChessController targetChess; // 攻击目标
    private List<ChessController> seekerChessList = new List<ChessController>(); // 被攻击来源
    private IEnumerator fightCoroutine;

    private bool isBeingFocus = false;
    private bool hasInteracted = false;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get { return CurrentHealth <= 0; } }

    public Action<ChessController> GotFocused; // 被锁定
    public Action<ChessController> GotDefocused; // 被取消锁定

    public Action<float, float> OnDamageTaken;
    public Action<ChessController> OnChessDied;
    public Action<bool> OnRoundOver;

    #region properties
    public ChessType CharacterType { get { return propTemplate.character; } }
    public ChessCamp Camp { get { return propTemplate.camp; } }
    public float Radius { get { return propTemplate.attackRange; } }
    public Vector2 Position { get { return chessProp.posOnBoard; } set { chessProp.posOnBoard = value; } }

    public ChessController Target { get { return targetChess; } }
    public List<ChessController> SeekerList { get { return seekerChessList; } }
    #endregion

    private void EventsRegister() {
        GotFocused += (_seeker) => {
            isBeingFocus = true;
            hasInteracted = false;

            if (!seekerChessList.Contains(_seeker)) {
                seekerChessList.Add(_seeker);
            }

            if (_seeker != null) {
                Debug.Log($"{_seeker.name} is focusing on {transform.name}");
            }
        };

        GotDefocused += (_seeker) => {
            if (seekerChessList.Contains(_seeker)) {
                seekerChessList.Remove(_seeker);
            }

            if (seekerChessList.Count <= 0) {
                isBeingFocus = false;
            }

            Debug.Log($"{_seeker.name} has defocused on {transform.name}");
        };
        
        _motor.OnReachedDestination += (targetTransform) => {
            if (_anim != null) _anim.StartAttacking?.Invoke();
            Fight();
        };

        GameManager.Instance.OnRoundFinished += (isSelfWin) => {
            _motor.FreezeMotorFunction();

            if (isSelfWin) {
                if (_anim != null) {
                    _anim.AttackFinished?.Invoke(true);
                }
            }
        };
    }

    void OnDrawGizmosSelected() {
        if (chessProp == null) {
            return;
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + radiusOffset, chessProp.attackRange);
    }

    void Awake() {
        _motor = transform.GetComponent<ChessMotor>();
        _boardManager = GameObject.FindWithTag("GameBoard").GetComponent<BoardManager>();

        // create scriptable object
        chessProp = ScriptableObject.CreateInstance<ChessProp>();
        chessProp.Init(propTemplate);
        CurrentHealth = chessProp.maxHealth.GetValue;
    }

    void Start() {
        EventsRegister();

        Instantiate(chessProp._gfx, transform);

        _anim = GetComponentInChildren<AnimManager>();
        if (_anim != null) {
            _anim.SetReady?.Invoke();
        }
    }

    #region path finding
    public void SeekForNextTarget() {
        int seekingRange = 0;
        ChessController newTarget = null;

        if (Camp == ChessCamp.SELF_SIDE) {
            seekingRange = _boardManager.GetChessList_OtherSide.Count;
            if (seekingRange <= 0) {
                return;
            }
            newTarget = _boardManager.GetChessList_OtherSide[UnityEngine.Random.Range(0, seekingRange)];
        }

        if (Camp == ChessCamp.OTHER_SIDE) {
            seekingRange = _boardManager.GetChessList_SelfSide.Count;
            if (seekingRange <= 0) {
                return;
            }
            newTarget = _boardManager.GetChessList_SelfSide[UnityEngine.Random.Range(0, seekingRange)];
        }

        SetFocus(newTarget);
    }

    public void SetFocus(ChessController newTarget) {
        if (targetChess == newTarget || newTarget == null) {
            return;
        }
        
        targetChess = newTarget; // assign new focus
        newTarget.GotFocused?.Invoke(this);    // focus the new target

        if (_anim != null)
            _anim.MovingToTarget?.Invoke();

        _motor.SetTracingTarget(newTarget); // trace the new target
    }

    public void RemoveFocus() {
        if (targetChess != null) {
            targetChess.GotDefocused?.Invoke(this);
        }

        targetChess = null;
        _motor.RemoveTracingTarget();
    }
    #endregion

    #region attack manager
    public void Fight() {
        if (targetChess == null) {
            return;
        }
        fightCoroutine = ContinuouslyAttackEnemy(targetChess.GetComponent<ChessController>());
        StartCoroutine(fightCoroutine);
    }

    IEnumerator ContinuouslyAttackEnemy(ChessController enemyChess) {
        yield return new WaitForSeconds(attackDelay);
        
        if (enemyChess == null || IsDead) {
            yield break;
        }

        while (!enemyChess.IsDead && !IsDead) {
            enemyChess.TakeDamage(this);
            yield return new WaitForSeconds(1.0f / chessProp.attackRate);
        }
    }
    // get damaged by enemy
    public void TakeDamage(ChessController enemyController) {
        if (enemyController == null || IsDead) {
            return;
        }

        var damageAmount = enemyController.chessProp.damageAmout.GetValue;

        if (damageAmount < 0) {
            return;
        }

        if (chessProp.buff.GetValue > 0) {
            damageAmount -= chessProp.buff.GetValue;
            damageAmount = Mathf.Clamp(damageAmount, 0, chessProp.maxHealth.GetValue);
        }

        CurrentHealth -= damageAmount;
        OnDamageTaken?.Invoke(CurrentHealth, chessProp.maxHealth.GetValue);

        if (CurrentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        Debug.Log($"{transform.name} died");
        if (_anim != null) _anim.AttackFinished?.Invoke(false);

        // stop fighting
        if (fightCoroutine != null) {
            StopCoroutine(fightCoroutine);
        }

        // remove focus
        RemoveFocus();

        // reset the slot status
        _boardManager.QuitBattleField(this);
        Position = Vector2.zero;

        // set new target for seekers
        for (int i = 0; i < seekerChessList.Count; i++) {
            var seeker = seekerChessList[i];
            seeker.SeekForNextTarget();
        }

        // got defocus by seekers
        for (int i = 0; i < seekerChessList.Count; i++) {
            GotDefocused?.Invoke(seekerChessList[i]);
        }

        transform.Find("Canvas").gameObject.SetActive(false);
        //gameObject.SetActive(false);

        OnChessDied?.Invoke(this);
    }
    #endregion

    void OnMouseOver() {
        InteractEventsManager.MouseEnterInteractable?.Invoke();
    }

    void OnMouseExit() {
        InteractEventsManager.MouseLeaveInteractable?.Invoke();
    }
}
