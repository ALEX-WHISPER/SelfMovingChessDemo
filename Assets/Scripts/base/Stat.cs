
using System.Collections.Generic;

[System.Serializable]
public class Stat {

    [UnityEngine.SerializeField]
    private int baseValue;

    // 本属性点数修改列表，每个元素的值表示其对该属性点数的增量
    private List<int> modifiers;
    
    public Stat() {
        this.baseValue = 100;
        modifiers = new List<int>();
    }

    public Stat(int val) {
        modifiers = new List<int>();
        this.baseValue = val;
    }

    public int GetValue {
        get {
            var totalValue = baseValue;
            for (int i = 0; i < modifiers.Count; i++) {
                totalValue += modifiers[i];
            }
            return totalValue;
            //return baseValue;
        }
    }

    public void AddModifier(int _modifier) {
        if (_modifier != 0) {
            modifiers.Add(_modifier);
        }
    }

    public void RemoveModifier(int _modifier) {
        if (_modifier != 0) {
            modifiers.Remove(_modifier);
        }
    }
}
