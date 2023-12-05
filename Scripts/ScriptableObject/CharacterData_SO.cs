using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel = 10;
    public int baseExp;
    public int currentExp;
    public float levelBuff;

    [Header("Exp point")]
    public int ExpPoint;

    public float levelMultiplier
    {
        get
        {
            return 1 + (currentLevel - 1) * levelBuff;
        }
    }
    public void UpdateExp(int expPoint)
    {
        currentExp += expPoint;
        if (currentExp >= baseExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel = Mathf.Min(currentLevel + 1, maxLevel);
        currentExp = currentExp - baseExp;
        baseExp = (int)(baseExp * levelMultiplier);
        maxHealth = (int)(maxHealth * levelMultiplier);
        currentHealth = maxHealth;       
    }
}
