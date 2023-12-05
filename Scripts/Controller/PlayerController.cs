using Mono.Cecil.Cil;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.InputManagerEntry;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private GameObject attackTarget;
    private float lastAttackTime;
    private CharacterStats characterStats;
    private bool isDead;
    private float stopDistance;

    public GameInputActions inputActions;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
        inputActions = new GameInputActions();
        inputActions.Enable();
    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;

        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void Update()
    {
        getmoveInput();
        isDead = characterStats.CurrentHealth == 0;
        if (isDead) { 
            GameManager.Instance.NotifyObservers(); 
            agent.isStopped = true;
        }
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }

    private void getmoveInput()
    {
        //����ȡ����Move����ֵ��ֵ��moveVector2 
        Vector2 moveVector2 = inputActions.Player.Move.ReadValue<Vector2>();
        //�ж��Ƿ��а��¶�Ӧ��Move����
        if (moveVector2 != Vector2.zero)
        {
            //����ȡ���ķ���ֵ��ӡ����
            Debug.Log(moveVector2);
        }
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
    private void EventAttack(GameObject target)
    {
        if (target != null)
        {           
            characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
            attackTarget = target;
            StartCoroutine(MoveToAttackTarget());
        }
    }
    
    private void SwitchAnimation()
    {
        animator.SetFloat("Speed", agent.velocity.sqrMagnitude);
        animator.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();//��ر�����Э�̷����������ִ��������Э�̣�Ҳ����˵����ذ�û������
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }
   
    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        //�����ƶ���ʱ���stoppingDistanceΪ1�������ȥ�������˵Ļ��Ͱ�stoppingDistance��Ϊ��������
        agent.stoppingDistance = characterStats.attackData.attackRange;

        transform.LookAt(attackTarget.transform);
      
        while(Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;

        if(lastAttackTime < 0)
        {           
            animator.SetBool("Critical", characterStats.isCritical);
            animator.SetTrigger("Attack");
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    //Animation Event
    void Hit()
    {
        if(attackTarget.CompareTag("Attackable")) {
            //����û���ж�ʯͷ��״̬��ʲô��ǿ�Ƹĳɷ����ˣ��ô������ʯͷ�Ƿ��ڿ���Ҳ���Ա���ҷ�����ȥ
            if (attackTarget.GetComponent<Rock>())
            {
                attackTarget.GetComponent<Rock>().rockState = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;//����Ϊ���ƹ�fixedUpdate���ж�
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStates = attackTarget.GetComponent<CharacterStats>();
            targetStates.TakeDamage(characterStats, targetStates);
        }       
    }
}