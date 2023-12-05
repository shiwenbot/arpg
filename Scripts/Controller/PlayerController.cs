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
        //将读取到的Move返回值赋值给moveVector2 
        Vector2 moveVector2 = inputActions.Player.Move.ReadValue<Vector2>();
        //判断是否有按下对应的Move按键
        if (moveVector2 != Vector2.zero)
        {
            //将获取到的返回值打印出来
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
        StopAllCoroutines();//需关闭所有协程否则人物会坚持执行完所有协程，也就是说点击地板没有作用
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }
   
    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        //正常移动的时候的stoppingDistance为1，如果是去攻击敌人的话就把stoppingDistance改为攻击距离
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
            //这里没有判断石头的状态是什么就强制改成反弹了，好处是如果石头是飞在空中也可以被玩家反弹回去
            if (attackTarget.GetComponent<Rock>())
            {
                attackTarget.GetComponent<Rock>().rockState = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;//这是为了绕过fixedUpdate的判断
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