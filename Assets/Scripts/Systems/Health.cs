using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    public int maxHealth = 100;

    [SerializeField] private int currentHealth;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent<int> OnDamageTaken;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"{name} received: {damage}. left: {currentHealth}");

        OnDamageTaken?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        
        if (!gameObject.CompareTag("Player")) 
        {
            Destroy(gameObject);
        }
    }
}