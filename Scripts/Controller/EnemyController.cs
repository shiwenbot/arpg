using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates
{
    GUARD,
    PATROL,
    CHASE,
    DEAD
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent (typeof(CharacterStats))]
public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private NavMeshAgent agent;
    private EnemyStates enemyStates;
    private Animator animator;
    protected CharacterStats characterStats;
    private Collider collider;

    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerIsDead = false;

    [Header("Basic Settings")]
    public float sightRadius = 8;
    protected GameObject AttackTarget;
    private float speed;
    public bool isGuard = true;
    public float standTime = 3.0f;
    private float remainStandTime;//会更新这个值，如果<0会重新
    private float lastAttackTime;
    private Quaternion guardRotation;

    [Header("Patrol State")]
    private float patrolRange = 8;
    private Vector3 wayPoint; //怪物会在移动范围内随机选择一个点移动
    private Vector3 guardPos; //怪物最原始的坐标

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        collider = GetComponent<Collider>();

        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainStandTime = standTime;
    }

    private void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.AddObserver(this);
    }

    private void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {        
        if(characterStats.CurrentHealth == 0)
        {
            isDead = true;
        }
        if (!playerIsDead)
        {
            switchStates();
            switchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
        
    }

    void switchAnimation()
    {
        animator.SetBool("Walk", isWalk);
        animator.SetBool("Chase", isChase);
        animator.SetBool("Follow", isFollow);
        animator.SetBool("Critical", characterStats.isCritical);
        animator.SetBool("Death", isDead);
    }

    void switchStates()
    {
        if (isDead)
        {
            enemyStates = EnemyStates.DEAD;
        }

        else if (findPlayer())
        {
            enemyStates = EnemyStates.CHASE;           
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isFollow = false;
                if(transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    if (Vector3.Distance(transform.position, guardPos) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.05f);//缓慢移动
                    }
                }
                break;
            case EnemyStates.PATROL:                
                isChase = false;
                agent.speed = speed * 0.5f; //非追击状态移动速度慢一点
                if(Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    //进到这行表示已经走到了目标点
                    isWalk = false;
                    
                    //巡逻的时候每到一个点要站在原地巡视一会
                    if (remainStandTime > 0)
                    {
                        
                        remainStandTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                    }                                     
                }
                else
                {                   
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;
            case EnemyStates.CHASE:
                isWalk = false;
                isChase = true;
                agent.speed = speed;
                //追player，如果脱离范围就回到上一个状态，如果在攻击范围内就攻击（动画）
                if (!findPlayer())
                {
                    isFollow = false;
                    
                    if (remainStandTime > 0)
                    {
                        agent.destination = agent.transform.position;
                        remainStandTime -= Time.deltaTime;
                    }
                    //返回上一个状态
                    else if (isGuard)
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else
                    {
                        enemyStates = EnemyStates.PATROL;
                    }                    
                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = AttackTarget.transform.position;
                }

                //判断是否在攻击范围内
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;

                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //暴击判断
                        float randomNum = Random.value;                        
                        characterStats.isCritical = randomNum < characterStats.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }
                }
                break;
            case EnemyStates.DEAD:
                collider.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;
        }
    }

    void Attack()
    {
        transform.LookAt(AttackTarget.transform);
        if (TargetInAttackRange())
        {
            animator.SetTrigger("Attack");
        }

        if (TargetInSkillRange())
        {
            animator.SetTrigger("Skill");
        }
    }

    bool findPlayer()
    {
        //在敌人周围查找所有的collider
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach(var target in colliders){
            if (target.CompareTag("Player"))
            {
                AttackTarget = target.gameObject;
                return true;
            }          
        }
        AttackTarget = null;
        return false;
    }

    bool TargetInAttackRange()
    {
        if (AttackTarget != null)
            return Vector3.Distance(AttackTarget.transform.position, transform.position) < characterStats.attackData.attackRange;
        else
            return false;
    }

    bool TargetInSkillRange()
    {
        if (AttackTarget != null)
            return Vector3.Distance(AttackTarget.transform.position, transform.position) < characterStats.attackData.skillRange;
        else
            return false;
    }

    void GetNewWayPoint()
    {
        remainStandTime = standTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);//保留y，因为地图有高度变化

        //如果随机到的点是不可到达的呢？
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;//1代表walkable      
    }

    //会把区域画出来
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    void Hit()
    {
        //怪物要多这个判断是因为他的攻击是自动的，因此如果人物跑开了会报错
        //isFacingTarget是在判断玩家是否在怪物目前朝向的120度扇形范围内
        if (AttackTarget != null && transform.isFacingTarget(AttackTarget.transform))
        {
            var targetStats = AttackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {        
        animator.SetBool("Win", true);
        playerIsDead = true;
        isChase = false;
        isWalk = false;
        AttackTarget = null;
    }
}