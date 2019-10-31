using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : SimpleHealthBar {

    private ChessController chessController;

    private void Awake() {
        chessController = GetComponentInParent<ChessController>();
    }

    void Start() {
        if (chessController != null) {
            chessController.OnHealthChanged += this.UpdateBar;
        }
    }
}
