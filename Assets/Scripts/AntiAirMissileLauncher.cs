using UnityEngine;

public class AntiAirMissileLauncher : UnitBase
{
    [Header("防空导弹发射器")]
    public bool isActive = true;
    public float attackRange = 10f;
    public float attackCooldown = 0.1f;

    private float lastAttackTime = -0.1f;

    protected override void Awake()
    {
        base.Awake();
        unitName = "防空导弹发射器";
        maxHealth = 30;
        currentHealth = maxHealth;
        visionRadius = 1f;
    }

    void Update()
    {
        if (isDead || GameManager.Instance == null || GameManager.Instance.isGameOver) return;
        if (!isActive) return;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            EnemyPlane target = FindNearestVisibleEnemy();
            if (target != null)
            {
                Attack(target);
            }
        }
    }

    EnemyPlane FindNearestVisibleEnemy()
    {
        EnemyPlane[] all = FindObjectsOfType<EnemyPlane>();
        EnemyPlane nearest = null;
        float minDist = float.MaxValue;

        foreach (var e in all)
        {
            if (e.isDead) continue;
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist > attackRange) continue;
            // 迷雾中的敌人不可锁定
            if (FogOfWar.Instance != null && !FogOfWar.Instance.IsPositionRevealed(e.transform.position))
                continue;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    void Attack(EnemyPlane target)
    {
        if (target == null || target.isDead) return;

        lastAttackTime = Time.time;
        SpawnMissile(target);

        // 消耗生命作为弹药
        currentHealth--;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void SpawnMissile(EnemyPlane target)
    {
        GameObject obj = new GameObject("导弹");
        obj.transform.position = transform.position + Vector3.up * 1.5f;

        Missile missile = obj.AddComponent<Missile>();
        missile.damage = 30;
        missile.SetTarget(target);
        missile.CreateVisual();
        missile.CreateTrail();
    }

    public bool CanSwitchMode()
    {
        return true;
    }

    public void ToggleMode()
    {
        isActive = !isActive;
    }
}
