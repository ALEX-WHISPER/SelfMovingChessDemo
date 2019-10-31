using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimManager : MonoBehaviour {
    private Animator _anim;

    public Action SetReady;
    public Action MovingToTarget;
    public Action StartAttacking;
    public Action TargetChanged;
    public Action<bool> AttackFinished;
    public Action ResetToStart;

    private void Awake() {
        _anim = GetComponent<Animator>();
    }

    private void OnEnable() {
        _anim.applyRootMotion = true;

        SetReady += () => {
            _anim.SetBool("bool_Idling", true);
        };

        MovingToTarget += () => {
            _anim.SetBool("bool_Idling" ,false); // stop idling

            _anim.SetBool("bool_Tracing", true); // continously tracing
            _anim.SetTrigger("tri_Tracing");    // trigger to trace
        };

        StartAttacking += () => {
            _anim.SetBool("bool_Tracing", false); // stop tracing

            _anim.SetBool("bool_Attacking", true); // continously attacking
            _anim.SetTrigger("tri_Attack");     // trigger to attack
        };

        TargetChanged += () => {
            _anim.SetBool("bool_Attacking", false); // stop attacking
            
            _anim.SetBool("bool_Tracing", true); // continously tracing
            _anim.SetTrigger("tri_Tracing");    // trigger to trace
        };

        AttackFinished += (isVictory) => {
            _anim.SetBool("bool_Attacking", false); // stop attacking

            if (isVictory) {
                _anim.SetBool("bool_RoundOver", true); // continously idling
                _anim.SetTrigger("tri_Victory");    // start victory idling
            } else {
                _anim.SetTrigger("tri_Dead");   // trigger to die
            }
        };

        ResetToStart += () => {
            _anim.SetBool("bool_RoundOver", false);

            _anim.SetTrigger("tri_Reset");
            _anim.SetBool("bool_Idling", true);
        };
    }

    private void Start() {
        SetReady?.Invoke();
    }
}
