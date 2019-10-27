using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : SimpleHealthBar {

    public CharacterStat chessStat;

    void Start() {
        chessStat.OnDamageTaken += this.UpdateBar;
    }
}
