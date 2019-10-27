using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterStat : MonoBehaviour {

    public int maxHealth = 100;
    public Stat damage; // 攻击力
    public Stat buff; // 减少伤害

    public Action<CharacterStat> OnCharacterDie;
    public Action<float, float> OnDamageTaken;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get { return CurrentHealth <= 0; } }

    private Action<int> OnBuffConsumed;

    void Awake() {
        CurrentHealth = maxHealth;
    }
    
    // attack enemy
    public void Attack(CharacterStat enemy) {
        if (enemy == null) {
            return;
        }
        enemy.TakeDamage(this);
    }
    
    // get damaged by enemy
    public void TakeDamage(CharacterStat enemyStat) {
        if (enemyStat == null || IsDead) {
            return;
        }

        var damageAmount = enemyStat.damage.GetValue;

        if (damageAmount < 0) {
            return;
        }

        if (buff.GetValue > 0) {
            damageAmount -= buff.GetValue;
            damageAmount = Mathf.Clamp(damageAmount, 0, int.MaxValue);

            OnBuffConsumed?.Invoke(damageAmount);
        }

        CurrentHealth -= damageAmount;
        OnDamageTaken?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0) {
            Die();
        }
    }

    protected virtual void Die() {
        Debug.Log($"{transform.name} died");
        OnCharacterDie?.Invoke(this);
    }
}
