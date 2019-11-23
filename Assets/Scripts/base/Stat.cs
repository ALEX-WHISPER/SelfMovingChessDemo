
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat {

    [SerializeField]
    private int baseValue;
    
    /// <summary>
    /// ctor without para
    /// </summary>
    public Stat() {
        this.baseValue = 0;
    }

    /// <summary>
    /// ctor with para: default value
    /// </summary>
    /// <param name="val"></param>
    public Stat(int val) {
        this.baseValue = val;
    }

    /// <summary>
    /// getter
    /// </summary>
    public int GetValue {
        get {
            return baseValue;
        }
    }

    /// <summary>
    /// setter
    /// </summary>
    /// <param name="val"></param>
    public void Set(int val) {
        this.baseValue = val;
    }

    /// <summary>
    /// increase value by 1
    /// </summary>
    public void Increase() {
        Increase(1);
    }

    /// <summary>
    /// decrease value by 1
    /// </summary>
    public void Decrease() {
        Decrease(1);
    }

    /// <summary>
    /// increase value by a specific step
    /// </summary>
    /// <param name="step"></param>
    public void Increase(int step) {
        if (step <= 0) {
            return;
        }
        this.baseValue += step;
    }

    /// <summary>
    /// decrease value by a specific step
    /// </summary>
    /// <param name="step"></param>
    public void Decrease(int step) {
        if (step <= 0) {
            return;
        }
        this.baseValue -= step;
        this.baseValue = Mathf.Clamp(this.baseValue, 0, int.MaxValue);
    }
}
