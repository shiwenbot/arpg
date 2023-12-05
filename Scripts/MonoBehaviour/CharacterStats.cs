using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> updateHealthBarOnAttack;

    public AttackData_SO attackData;
    public CharacterData_SO templeteData;

    [HideInInspector]
    public CharacterData_SO characterData;
    [HideInInspector]
    public bool isCritical;
    
    public void Awake()
    {
        if(templeteData != null)
        {
            characterData = Instantiate(templeteData);
        }
    }

    #region Read from Data_SO 
    public int MaxHealth
    {
        get { return characterData != null ? characterData.maxHealth : 0; }
        set { characterData.maxHealth = value; }
    }
    public int CurrentHealth
    {
        get { return characterData != null ? characterData.currentHealth : 0; }
        set { characterData.currentHealth = value; }
    }
    public int BaseDefence
    {
        get { return characterData != null ? characterData.baseDefence : 0; }
        set { characterData.baseDefence = value; }
    }
    public int CurrentDefence
    {
        get { return characterData != null ? characterData.currentDefence : 0; }
        set { characterData.currentDefence = value; }
    }
    #endregion

    #region Character Combat
    public void TakeDamage(CharacterStats attacker, CharacterStats defender)
    {       
        int damage = Math.Max(attacker.CurrentDamage() - defender.CurrentDefence, 0);
        CurrentHealth = Math.Max(CurrentHealth - damage, 0);       
        //攻击暴击时受击者会播放受击动画
        if (attacker.isCritical)
        {           
            defender.GetComponent<Animator>().SetTrigger("Hitted");
        }
        //TODO: update UI
        updateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        //TODO: 经验值
        if(CurrentHealth <= 0) {
            attacker.characterData.UpdateExp(characterData.ExpPoint);        
        }
    }

    //这里重载了TakeDamage是因为rock没有CharacterStats
    public void TakeDamage(int damage, CharacterStats defender)
    {
        int currentDamage = Mathf.Max(damage - defender.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);

        updateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        GameManager.Instance.characterStats.characterData.UpdateExp(characterData.ExpPoint);
    }

    private int CurrentDamage()
    {
        float curDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        if (isCritical)
        {
            curDamage *= attackData.criticalMultiplier;
            Debug.Log("暴击了" + curDamage);
        }
        return (int)curDamage;
    }
    #endregion
}