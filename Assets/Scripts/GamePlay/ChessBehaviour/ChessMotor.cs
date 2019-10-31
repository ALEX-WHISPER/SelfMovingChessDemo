using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ChessMotor : MonoBehaviour {

    public float rotateSpeed = 5f;
    public Action<Transform> OnReachedDestination;

    private Transform target;
    private NavMeshAgent _agent;
    private NavMeshObstacle _obstacle;
    private bool hasCheckedWhetherReach = false;
    private bool freezeAll = false;
    
    private void EventsRegister() {
        InteractEventsManager.MouseDragging += () => {
            if (_agent != null) {
                _agent.enabled = false;
            }
        };

        InteractEventsManager.MouseDoneDrag += () => {
            if (_agent != null) {
                _agent.enabled = true;
            }
        };
    }

    void Awake() {
        _agent = GetComponent<NavMeshAgent>();
        _obstacle = GetComponent<NavMeshObstacle>();
    }

    void Start() {
        EventsRegister();

        _obstacle.enabled = false;
        _agent.enabled = true;
    }

    void Update() {
        if (freezeAll) {
            return;
        }

        if (target != null && !CheckReached()) {
            FaceRotation();
            MoveToward(target.position);
        }

        if (!hasCheckedWhetherReach && target != null) {
            if (CheckReached()) {
                _agent.enabled = false;
                _obstacle.enabled = true;

                OnReachedDestination?.Invoke(target);
                hasCheckedWhetherReach = true;
            }
        }
    }

    public void FreezeMotorFunction() {
        freezeAll = true;

        if (_obstacle != null) _obstacle.enabled = false;
        if (_agent != null) _agent.enabled = false;
    }

    public void LaunchMotorFunction() {
        freezeAll = false;

        if (_obstacle != null) _obstacle.enabled = false;
        if (_agent != null) _agent.enabled = true;
    }

    // move towards a specified point
    public void MoveToward(Vector3 point) {
        _obstacle.enabled = false;
        _agent.enabled = true;
        _agent.SetDestination(point);        
    }
    
    // the hero will start to tracing once the target has been set
    public void SetTracingTarget(ChessController newTarget) {
        _agent.stoppingDistance = newTarget.Radius;
        _agent.updateRotation = false;
        target = newTarget.transform;

        hasCheckedWhetherReach = false;
    }

    // hero will stop tracing anything when target get removed
    public void RemoveTracingTarget() {
        _agent.stoppingDistance = 0f;
        _agent.updateRotation = true;
        target = null;
    }

    // facing to the target while tracing
    private void FaceRotation() {
        if (target == null) {
            return;
        }
        var direction = (target.position - transform.position).normalized;
        var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
    }
    
    // check whether the agent reached the destination
    private bool CheckReached() {
        //if (_agent.pathPending) {
        //    return false;
        //}
        //if (!(_agent.remainingDistance <= _agent.stoppingDistance)) {
        //    return false;
        //}

        //if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f) {
        //    return true;
        //} else {
        //    return false;
        //}
        return (target.position - transform.position).sqrMagnitude <= Mathf.Pow(_agent.stoppingDistance, 2);
    }
}
