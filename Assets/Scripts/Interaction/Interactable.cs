using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {

    [SerializeField]
    private float radius = 1f;
    public float Radius { get { return radius; } }

    private bool isBeingFocus = false;
    private bool hasInteracted = false;
    private Transform seeker;

    public Action<Transform> GotFocused; // 被锁定
    public Action<Transform> GotDefocused; // 被取消锁定

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void Start() {
        GotFocused += (_seeker) => {
            isBeingFocus = true;
            hasInteracted = false;
            seeker = _seeker;

            Debug.Log($"{_seeker.name} is focusing on {transform.name}");
        };

        GotDefocused += (_seeker) => {
            isBeingFocus = false;
            seeker = null;

            Debug.Log($"{_seeker.name} has defocused on {transform.name}");
        };
    }

    void Update() {
        if (isBeingFocus && !hasInteracted) {
            WaitingSeeker();
        }
    }

    private void WaitingSeeker() {
        if (!isBeingFocus) {
            return;
        }

        var distance = Vector3.Distance(seeker.position, transform.position);
        if (distance <= radius) {
            Interact();
        }
    }

    private void Interact() {
        Debug.Log($"{transform.name} INTERACT with {seeker.name}");
        hasInteracted = true;
    }

    void OnMouseOver() {
        InteractEventsManager.MouseEnterInteractable?.Invoke();
    }

    void OnMouseExit() {
        InteractEventsManager.MouseLeaveInteractable?.Invoke();
    }
}
