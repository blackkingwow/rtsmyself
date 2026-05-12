using UnityEngine;
using UnityEngine.Events;

public class UnitBase : MonoBehaviour
{
    [Header("基本属性")]
    public string unitName = "单位";
    public int maxHealth = 100;
    public int currentHealth;
    [HideInInspector] public bool isDead = false;

    [Header("视野")]
    public float visionRadius = 5f;

    public UnityAction<UnitBase> OnDeath;
    public UnityAction<int, int> OnHealthChanged;

    protected HealthBar healthBar;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar == null)
        {
            GameObject hbGo = new GameObject("HealthBar");
            hbGo.transform.SetParent(transform, false);
            healthBar = hbGo.AddComponent<HealthBar>();
        }
        UpdateHealthBar();
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        UpdateHealthBar();
    }

    public virtual void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateHealthBar();
    }

    protected virtual void Die()
    {
        isDead = true;
        VFX.SpawnExplosion(transform.position, 2f);
        OnDeath?.Invoke(this);
        Destroy(gameObject, 0.1f);
    }

    public void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.SetHealth((float)currentHealth / maxHealth);
    }
}
