using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce;
    public void KickOff()
    {
        if(AttackTarget != null)
        {
            transform.LookAt(AttackTarget.transform);
            //������ɵķ���
            Vector3 kickDirection = AttackTarget.transform.position - transform.position;
            kickDirection.Normalize();
            
            AttackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            AttackTarget.GetComponent<NavMeshAgent>().velocity = kickForce * kickDirection;
            AttackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }
}