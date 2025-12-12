using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    public int maxHealth = 100;

    [SerializeField] private int currentHealth;

    [Header("References")]
    public Animator animator;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent<int> OnDamageTaken;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"{name} received: {damage}. left: {currentHealth}");

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        ApplyKnockback(attackerPosition);

        OnDamageTaken?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void ApplyKnockback(Vector3 attackerPos)
    {
        if (TryGetComponent<PlayerController>(out var playerParams))
        {
            Vector3 knockbackDir = (transform.position - attackerPos).normalized;
            playerParams.AddImpact(knockbackDir, 60f);
        }

        if (TryGetComponent<EnemyController>(out var enemyParams))
        {
            enemyParams.OnHit(attackerPos);
        }
    }
    private void Die()
    {
        OnDeath?.Invoke();
        if (animator != null) animator.SetBool("IsDead", true);

        if (TryGetComponent<Collider>(out var col)) col.enabled = false;
        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent)) agent.enabled = false;

        if (!gameObject.CompareTag("Player"))
        {
            Destroy(gameObject, 5f);
        }
    }
}