using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessStat : CharacterStat {

    void Start() {
        // EquipmentManager.instance.OnEquipmentChanged += OnEquipmentChanged;
    }

    /*void OnEquipmentChanged(Equipment newItem, Equipment oldItem) {
        if (newItem != null) {
            buff.AddModifier(newItem.buffModifier);
            damage.AddModifier(newItem.damageModifier);
        }

        if (oldItem != null) {
            buff.RemoveModifier(oldItem.buffModifier);
            damage.RemoveModifier(oldItem.damageModifier);
        }
    }*/
}
