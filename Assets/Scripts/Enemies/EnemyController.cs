using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float patrolRadius = 10f;
    public float detectRadius = 10f;
    public float attackRange = 1.8f;
    public int damage = 10;
    public float attackCooldown = 1.5f;

    [Header("References")]
    public Animator animator;

    private NavMeshAgent _agent;
    private Transform _player;
    private EnemyBaseState _currentState;
    public EnemyIdleState IdleState = new();
    public EnemyChaseState ChaseState = new();
    public EnemyAttackState AttackState = new();
    public EnemyHitState HitState = new();
    public EnemyDeathState DeathState = new();

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) _player = p.transform;

        IdleState.Init(this);
        ChaseState.Init(this);
        AttackState.Init(this);
        HitState.Init(this);
        DeathState.Init(this);

        if (TryGetComponent<Health>(out var health))
        {
            health.OnDeath.AddListener(OnDeath);
        }

        ChangeState(IdleState);
    }

    private void Update()
    {
        if (_currentState != null) _currentState.Update();

        if (animator != null && _agent.enabled)
        {
            animator.SetFloat("Speed", _agent.velocity.magnitude);
        }
    }

    public void ChangeState(EnemyBaseState newState)
    {
        if (_currentState is EnemyDeathState) return;
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    public void OnHit(Vector3 attackerPos)
    {
        if (_currentState is EnemyDeathState) return;
        HitState.attackerPosition = attackerPos;
        ChangeState(HitState);
    }

    public void OnDeath()
    {
        ChangeState(DeathState);
    }
    public NavMeshAgent Agent => _agent;
    public Transform Player => _player;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}