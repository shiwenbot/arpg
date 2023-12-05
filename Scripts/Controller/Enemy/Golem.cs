using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 25;
    public GameObject rockPrefab;
    public Transform handPos;

    //Animation event
    public void KickOff()
    {
        if (AttackTarget != null && transform.isFacingTarget(AttackTarget.transform))
        {
            var targetStats = AttackTarget.GetComponent<CharacterStats>();

            Vector3 kickDirection = AttackTarget.transform.position - transform.position;
            kickDirection.Normalize();
            AttackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            AttackTarget.GetComponent<NavMeshAgent>().velocity = kickForce * kickDirection;

            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    //Animation event
    public void throwRock()
    {
        if(AttackTarget != null)
        {
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().attackTarget = AttackTarget;
        }
    }
}
