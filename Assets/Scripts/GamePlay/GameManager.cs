using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager: SingletonBase<GameManager> {

    public Action OnSelfSideVictory;
    public Action OnOtherSideVictory;

    private BoardManager _boardManager;
    private bool isBonded = false;

    void Awake() {
        _boardManager = GameObject.FindWithTag("GameBoard").GetComponent<BoardManager>();
    }

    private void OnEnable() {
        OnSelfSideVictory += () => {
            Debug.Log("VICTORY!");
        };

        OnOtherSideVictory += () => {
            Debug.Log("DEFEATED!");
        };
    }

    public void BindingFocus() {
        if (_boardManager == null) {
            return;
        }

        var l1 = _boardManager.GetChessList_SelfSide;
        var l2 = _boardManager.GetChessList_OtherSide;

        int m1 = l1.Count - 1;
        int m2 = l2.Count - 1;

        if (!(m1 > 0 && m2 > 0)) {
            return;
        }

        while (m1 >= 0 && m2 >= 0) {
            var _selfChess = SelectNonRepeatedChessRandomly(l1, ref m1);
            var _otherChess = SelectNonRepeatedChessRandomly(l2, ref m2);
            PairingFocus(_selfChess, _otherChess);
        }

        while (m1 >= 0) {
            var _selfChess = SelectNonRepeatedChessRandomly(l1, ref m1);
            var _otherChess = l2[UnityEngine.Random.Range(0, l2.Count)];
            _selfChess.SetFocus(_otherChess);
        }

        while (m2 >= 0) {
            var _otherChess = SelectNonRepeatedChessRandomly(l2, ref m2);
            var _selfChess = l1[UnityEngine.Random.Range(0, l1.Count)];
            _otherChess.SetFocus(_selfChess);
        }
    }

    private ChessController SelectNonRepeatedChessRandomly(List<ChessController> list, ref int max) {
        var r = UnityEngine.Random.Range(0, max + 1);
        var selectedElem = list[r];

        var temp = list[r];
        list[r] = list[max];
        list[max] = temp;

        max--;

        return selectedElem;
    }

    private void PairingFocus(ChessController self, ChessController other) {
        self.SetFocus(other);
        other.SetFocus(self);
    }
}
