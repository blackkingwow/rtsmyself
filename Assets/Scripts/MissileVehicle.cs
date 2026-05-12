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
                transform.rotation = Quaternion.LookRotation(dir.normalized);
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
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

    public float GetAttackCooldownRemaining()
    {
        return Mathf.Max(0, attackCooldown - (Time.time - lastAttackTime));
    }

}
