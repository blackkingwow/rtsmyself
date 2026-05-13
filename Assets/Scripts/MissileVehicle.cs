using UnityEngine;

public class MissileVehicle : UnitBase
{
    [Header("导弹车属性")]
    public float moveSpeed = 5f;
    public float attackDamage = 150f;
    public float attackCooldown = 12f;
    private float lastAttackTime = -12f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private SelectableUnit selectable;

    protected override void Awake()
    {
        base.Awake();
        unitName = "导弹车";
        maxHealth = 200;
        currentHealth = maxHealth;
        visionRadius = 3f;
    }

    protected override void Start()
    {
        base.Start();
        selectable = GetComponent<SelectableUnit>();
        targetPosition = transform.position;
    }

    void Update()
    {
        if (isDead) return;

        if (isMoving)
        {
            Vector3 dir = targetPosition - transform.position;
            dir.y = 0;
            if (dir.magnitude > 0.1f)
            {
                Vector3 moveDir = dir.normalized;
                // 避障检测
                moveDir = AvoidObstacles(moveDir);
                transform.rotation = Quaternion.LookRotation(moveDir);
                transform.position += moveDir * moveSpeed * Time.deltaTime;
            }
            else if (dir.magnitude < 0.2f)
            {
                isMoving = false;
                transform.position = targetPosition;
            }
        }
    }

    public void MoveTo(Vector3 destination)
    {
        targetPosition = new Vector3(destination.x, transform.position.y, destination.z);
        isMoving = true;
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    public void Attack(EnemyPlane target)
    {
        if (!CanAttack()) return;
        if (target == null || target.isDead) return;

        lastAttackTime = Time.time;
        SpawnMissile(target);
    }

    void SpawnMissile(EnemyPlane target)
    {
        GameObject obj = new GameObject("导弹");
        obj.transform.position = transform.position + Vector3.up * 1.5f;

        Missile missile = obj.AddComponent<Missile>();
        missile.damage = (int)attackDamage;
        missile.SetTarget(target);
        missile.CreateVisual();
        missile.CreateTrail();
    }

    Vector3 AvoidObstacles(Vector3 moveDir)
    {
        float checkDist = 1.0f;
        float sphereRadius = 0.4f;
        Vector3 origin = transform.position + Vector3.up * 0.3f;

        // 前方无障碍，直行
        RaycastHit hit;
        if (!Physics.SphereCast(origin, sphereRadius, moveDir, out hit, checkDist))
            return moveDir;

        // 前方有障碍，扇形扫描找最佳绕行方向
        float bestScore = -1f;
        Vector3 bestDir = moveDir;
        float[] angles = { 0f, 15f, -15f, 30f, -30f, 45f, -45f, 60f, -60f, 75f, -75f, 90f, -90f };

        foreach (float angle in angles)
        {
            Vector3 testDir = Quaternion.Euler(0, angle, 0) * moveDir;
            RaycastHit testHit;
            float clearDist = checkDist;
            if (!Physics.SphereCast(origin, sphereRadius, testDir, out testHit, checkDist))
                clearDist = checkDist;
            else
                clearDist = testHit.distance;

            // 评分：畅通距离 + 方向一致性（越接近原方向越高）
            float dirScore = 1f - Mathf.Abs(angle) / 90f;
            float score = clearDist / checkDist * 0.6f + dirScore * 0.4f;

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = testDir;
            }
        }

        // 最佳方向也堵死，停下
        if (bestScore <= 0.1f)
        {
            isMoving = false;
            return moveDir;
        }

        return bestDir.normalized;
    }

    public float GetAttackCooldownRemaining()
    {
        return Mathf.Max(0, attackCooldown - (Time.time - lastAttackTime));
    }

}
