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
    private float remainStandTime;//��������ֵ�����<0������
    private float lastAttackTime;
    private Quaternion guardRotation;

    [Header("Patrol State")]
    private float patrolRange = 8;
    private Vector3 wayPoint; //��������ƶ���Χ�����ѡ��һ�����ƶ�
    private Vector3 guardPos; //������ԭʼ������

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
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.05f);//�����ƶ�
                    }
                }
                break;
            case EnemyStates.PATROL:                
                isChase = false;
                agent.speed = speed * 0.5f; //��׷��״̬�ƶ��ٶ���һ��
                if(Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    //�������б�ʾ�Ѿ��ߵ���Ŀ���
                    isWalk = false;
                    
                    //Ѳ�ߵ�ʱ��ÿ��һ����Ҫվ��ԭ��Ѳ��һ��
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
                //׷player��������뷶Χ�ͻص���һ��״̬������ڹ�����Χ�ھ͹�����������
                if (!findPlayer())
                {
                    isFollow = false;
                    
                    if (remainStandTime > 0)
                    {
                        agent.destination = agent.transform.position;
                        remainStandTime -= Time.deltaTime;
                    }
                    //������һ��״̬
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

                //�ж��Ƿ��ڹ�����Χ��
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;

                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //�����ж�
                        float randomNum = Random.value;                        
                        characterStats.isCritical = randomNum < characterStats.attackData.criticalChance;
                        //ִ�й���
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
        //�ڵ�����Χ�������е�collider
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
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);//����y����Ϊ��ͼ�и߶ȱ仯

        //���������ĵ��ǲ��ɵ�����أ�
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;//1����walkable      
    }

    //������򻭳���
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    void Hit()
    {
        //����Ҫ������ж�����Ϊ���Ĺ������Զ��ģ������������ܿ��˻ᱨ��
        //isFacingTarget�����ж�����Ƿ��ڹ���Ŀǰ�����120�����η�Χ��
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