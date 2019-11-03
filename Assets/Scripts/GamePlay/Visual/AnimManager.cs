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
            _anim.ResetTrigger("tri_Reset");
            _anim.SetBool("bool_Idling", true);
        };

        MovingToTarget += () => {
            _anim.ResetTrigger("tri_Attack");
            _anim.SetBool("bool_Attacking", false);
            _anim.SetBool("bool_Idling" ,false); // stop idling

            _anim.SetTrigger("tri_Tracing");    // trigger to trace
            _anim.SetBool("bool_Tracing", true); // continously tracing
        };

        StartAttacking += () => {
            _anim.ResetTrigger("tri_Tracing");
            _anim.SetBool("bool_Idling", false); // stop idling
            _anim.SetBool("bool_Tracing", false); // stop tracing

            _anim.SetTrigger("tri_Attack");     // trigger to attack
            _anim.SetBool("bool_Attacking", true); // continously attacking
        };

        TargetChanged += () => {
            _anim.ResetTrigger("tri_Attack");
            _anim.SetBool("bool_Attacking", false); // stop attacking
            _anim.SetBool("bool_Idling", false); // stop idling

            _anim.SetTrigger("tri_Tracing");    // trigger to trace
            _anim.SetBool("bool_Tracing", true); // continously tracing
        };

        AttackFinished += (isVictory) => {
            _anim.SetBool("bool_Idling", false); // stop idling
            _anim.SetBool("bool_Attacking", false); // stop attacking

            if (isVictory) {
                _anim.SetTrigger("tri_Reset");    // start victory idling
                _anim.SetBool("bool_Idling", true); // continously idling

            } else {
                _anim.SetTrigger("tri_Dead");   // trigger to die
            }
        };

        ResetToStart += () => {
            _anim.ResetTrigger("tri_Tracing");
            _anim.ResetTrigger("tri_Attack");
            _anim.ResetTrigger("tri_TargetChanged");
            _anim.ResetTrigger("tri_Dead");
            
            _anim.SetBool("bool_Idling", false);
            _anim.SetBool("bool_Tracing", false);
            _anim.SetBool("bool_Attacking", false);

            _anim.SetTrigger("tri_Reset");
            _anim.SetBool("bool_Idling", true);

            SetReady?.Invoke();
        };
    }

    private void Start() {
        //SetReady?.Invoke();
        ResetToStart?.Invoke();
    }
}
