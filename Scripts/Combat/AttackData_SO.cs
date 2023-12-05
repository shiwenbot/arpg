using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Character Stats/Attack")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange;
    public float skillRange;
    public float coolDown;
    public float minDamage;
    public float maxDamage;
    public float criticalChance;
    public float criticalMultiplier;
}
