using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ChessType { SELF_SIDE, OTHER_SIDE }
public enum ChessHero { SWORD, AXE, KNIGHT, MUTANT, ARCHER }

[RequireComponent(typeof(ChessMotor))]
[RequireComponent(typeof(ChessStat))]
public class ChessController : MonoBehaviour {

    [SerializeField]
    private float radius = 1f;
    public Vector3 radiusOffset = Vector3.zero;
    public int attackCountPerSecond = 2;
    public float attackDelay = 0.5f;

    public ChessType _chessType;
    public ChessHero _chessCharacter;
    public bool isPlayer = false;
    public LayerMask layer_Ground;
    public LayerMask layer_Enemy;

    private ChessMotor _motor;
    private ChessStat _stat;
    private BoardManager _boardManager;
    private AnimManager _anim;

    private ChessController targetChess; // 攻击目标
    private List<ChessController> seekerChessList = new List<ChessController>(); // 被攻击来源
    private IEnumerator fightCoroutine;

    private bool isBeingFocus = false;
    private bool hasInteracted = false;

    public Action<ChessController> GotFocused; // 被锁定
    public Action<ChessController> GotDefocused; // 被取消锁定

    public Vector2 Position;
    public ChessController Target { get { return targetChess; } }
    public List<ChessController> SeekerList { get { return seekerChessList; } }
    public float Radius { get { return radius; } }

    private void EventsRegister() {
        GotFocused += (_seeker) => {
            isBeingFocus = true;
            hasInteracted = false;

            if (!seekerChessList.Contains(_seeker)) {
                seekerChessList.Add(_seeker);
            }

            Debug.Log($"{_seeker.name} is focusing on {transform.name}");
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

        _stat.OnCharacterDie += (stat) => {
            if (_anim != null) _anim.AttackFinished?.Invoke(false);
            OnChessDieCallback();
        };

        _motor.OnReachedDestination += (targetTransform) => {
            if (_anim != null) _anim.StartAttacking?.Invoke();
            Fight();
        };
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + radiusOffset, radius);
    }

    void Awake() {
        _motor = transform.GetComponent<ChessMotor>();
        _stat = transform.GetComponent<ChessStat>();
        _boardManager = GameObject.FindWithTag("GameBoard").GetComponent<BoardManager>();
        _anim = GetComponentInChildren<AnimManager>();
    }

    void Start() {
        EventsRegister();

        if (_anim != null) _anim.SetReady?.Invoke();
    }

    void Update() {
        if (Input.GetMouseButtonDown(1) && isPlayer) {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f, layer_Enemy)) {
                var target = hit.collider.GetComponent<ChessController>();
                if (target != null) {
                    SetFocus(target);
                }
            }
            
            if (Physics.Raycast(ray, out hit, 100f, layer_Ground)) {
                if (_boardManager.IsSelected) {
                    RemoveFocus();
                    _motor.MoveToward(_boardManager.SelectedTilePos);
                }
            }
        }

        //if (isBeingFocus && !hasInteracted) {
        //    WaitingSeeker();
        //}
    }

    #region offensive side
    public void SeekForNextTarget() {
        int seekingRange = 0;
        ChessController newTarget = null;

        if (_chessType == ChessType.SELF_SIDE) {
            seekingRange = _boardManager.GetChessList_OtherSide.Count;
            if (seekingRange <= 0) {
                return;
            }
            newTarget = _boardManager.GetChessList_OtherSide[UnityEngine.Random.Range(0, seekingRange)];
        }

        if (_chessType == ChessType.OTHER_SIDE) {
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

        //if (targetChess != null) {
        //    targetChess.GotDefocused?.Invoke(this); // defocus the previous target
        //}

        targetChess = newTarget; // assign new focus
        newTarget.GotFocused?.Invoke(this);    // focus the new target

        if (_anim != null) _anim.MovingToTarget?.Invoke();
        _motor.SetTracingTarget(newTarget); // trace the new target
    }

    public void RemoveFocus() {
        if (targetChess != null) {
            targetChess.GotDefocused?.Invoke(this);
        }

        targetChess = null;
        _motor.RemoveTracingTarget();
    }

    public void Fight() {
        if (targetChess == null) {
            return;
        }
        fightCoroutine = ContinuouslyAttackEnemy(targetChess.GetComponent<ChessStat>());
        StartCoroutine(fightCoroutine);
    }

    public void OnChessDieCallback() {
        // stop fighting
        if (fightCoroutine != null) {
            StopCoroutine(fightCoroutine);
        }

        // remove focus
        RemoveFocus();

        // reset the slot status
        _boardManager.QuitBoardField(this);
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

        gameObject.SetActive(false);
    }

    IEnumerator ContinuouslyAttackEnemy(ChessStat fightingStat) {
        yield return new WaitForSeconds(attackDelay);

        if (_stat == null || _stat.IsDead) {
            yield break;
        }

        if (fightingStat == null) {
            yield break;
        }

        while (!fightingStat.IsDead && !_stat.IsDead) {
            _stat.Attack(fightingStat);
            yield return new WaitForSeconds(1.0f / attackCountPerSecond);
        }
    }
    #endregion
    
    void OnMouseOver() {
        InteractEventsManager.MouseEnterInteractable?.Invoke();
    }

    void OnMouseExit() {
        InteractEventsManager.MouseLeaveInteractable?.Invoke();
    }
}
