using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Settings")]
    public float attackRange = 1.5f;
    public int damage = 10;
    public float attackCooldown = 1.5f;

    private NavMeshAgent _agent;
    private Transform _player;
    private float _lastAttackTime;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        _agent.SetDestination(_player.position);

        if (distance <= attackRange)
        {
            if (Time.time >= _lastAttackTime + attackCooldown)
            {
                Attack();
                _lastAttackTime = Time.time;
            }
        }
    }

    private void Attack()
    {
        // Пытаемся нанести урон игроку
        var hp = _player.GetComponent<IDamageable>();
        hp?.TakeDamage(damage);
    }
}