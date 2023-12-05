using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates
    {
        HitPlayer, HitEnemy, Stay
    }
    public RockStates rockState;
    private Rigidbody rb;
    [Header("Basic Settings")]
    public float force;
    public int damage;
    public GameObject attackTarget;
    private Vector3 direction;
    public GameObject breakEffect;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rockState = RockStates.HitPlayer;
        flyToTarget();
    }

    //����жϺ�������ص�Ҫ�����������Update
    private void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude < 1f)
        {
            rockState = RockStates.Stay;
        }
    }

    public void flyToTarget()
    {
        //�����up��ʯͷ�����ڿ��з�һ������������������ֱ��������
        direction = (attackTarget.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch(rockState)
        {
            case RockStates.HitPlayer:
                if (collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharacterStats>());
                    rockState = RockStates.Stay;
                }
                break;
            case RockStates.HitEnemy:
                if (collision.gameObject.GetComponent<Golem>())
                {
                    var GolemStates = collision.gameObject.GetComponent<CharacterStats>();
                    GolemStates.TakeDamage(damage, GolemStates);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
        }
    }
}