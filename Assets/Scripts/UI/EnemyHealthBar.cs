using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public Health healthComponent;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0);

    private Transform _cam;

    private void Start()
    {
        _cam = Camera.main.transform;
        
        if (healthComponent)
        {
            healthSlider.maxValue = healthComponent.maxHealth;
            healthSlider.value = healthComponent.maxHealth;
            
            // Subscribe to the existing event in Health.cs
            healthComponent.OnDamageTaken.AddListener(UpdateHealth);
        }
    }

    private void LateUpdate()
    {
        // Billboard effect: Always face the camera
        transform.position = healthComponent.transform.position + offset;
        transform.LookAt(transform.position + _cam.forward);
    }

    private void UpdateHealth(int currentHealth)
    {
        healthSlider.value = currentHealth;
        
        if (currentHealth <= 0)
        {
            Destroy(gameObject); // Remove bar on death
        }
    }
}