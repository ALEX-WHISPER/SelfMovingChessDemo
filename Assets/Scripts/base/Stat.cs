
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat {

    [SerializeField]
    private int baseValue;
    
    public Stat() {
        this.baseValue = 0;
    }

    public Stat(int val) {
        this.baseValue = val;
    }

    public int GetValue {
        get {
            return baseValue;
        }
    }

    public void Set(int val) {
        this.baseValue = val;
    }

    public void Increase() {
        Increase(1);
    }

    public void Decrease() {
        Decrease(1);
    }

    public void Increase(int step) {
        if (step <= 0) {
            return;
        }
        this.baseValue += step;
    }

    public void Decrease(int step) {
        if (step <= 0) {
            return;
        }
        this.baseValue -= step;
        this.baseValue = Mathf.Clamp(this.baseValue, 0, int.MaxValue);
    }
}
