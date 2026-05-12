using UnityEngine;

public class EnemyPlane : UnitBase
{
    [Header("敌机属性")]
    public float flySpeed = 1f;
    public int bombDamage = 100;

    private enum State { Flying, Bombing, Retreating }
    private State state = State.Flying;

    private Vector3 targetLastPosition;
    private UnitBase lockedTarget;
    private LineRenderer linkLine;
    private Renderer[] allRenderers;
    private Collider[] allColliders;
    private bool hasBombed = false;

    private float CurrentSpeed
    {
        get
        {
            float s = flySpeed;
            if (GameManager.Instance != null && GameManager.Instance.isEmpActive)
                s *= GameManager.Instance.empSlowFactor;
            return s;
        }
    }

    protected override void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnEnemyDestroyed();
        base.Die();
    }

    void RetreatDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemiesAliveInWave--;
            if (GameManager.Instance.enemiesAliveInWave <= 0 && GameManager.Instance.isWaveActive)
                GameManager.Instance.EndWave();
        }
        isDead = true;
        Destroy(gameObject);
    }

    protected override void Awake()
    {
        base.Awake();
        unitName = "敌机";
        maxHealth = 150;
        currentHealth = maxHealth;
        visionRadius = 0f;
    }

    protected override void Start()
    {
        base.Start();
        allRenderers = GetComponentsInChildren<Renderer>();
        allColliders = GetComponentsInChildren<Collider>();
        SetupLineRenderer();
        ChooseTarget();
    }

    void SetupLineRenderer()
    {
        linkLine = gameObject.AddComponent<LineRenderer>();
        linkLine.positionCount = 2;
        linkLine.startWidth = 0.1f;
        linkLine.endWidth = 0.1f;
        linkLine.material = new Material(Shader.Find("Sprites/Default"));
        linkLine.startColor = Color.red;
        linkLine.endColor = Color.red;
    }

    void ChooseTarget()
    {
        // 优先选开启主动侦察的雷达站，其次选玩家基地
        RadarStation[] radars = FindObjectsOfType<RadarStation>();
        RadarStation activeRadar = null;
        foreach (var r in radars)
        {
            if (!r.isDead && r.isActiveMode)
            {
                activeRadar = r;
                break;
            }
        }

        if (activeRadar != null)
        {
            lockedTarget = activeRadar;
        }
        else if (GameManager.Instance.playerBase != null && !GameManager.Instance.playerBase.isDead)
        {
            lockedTarget = GameManager.Instance.playerBase;
        }
        else
        {
            // 无目标则直接飞走
            Destroy(gameObject, 2f);
            return;
        }

        targetLastPosition = lockedTarget.transform.position;
    }

    void Update()
    {
        if (isDead || GameManager.Instance.isGameOver) return;

        // 迷雾中隐藏
        bool inFog = FogOfWar.Instance != null && !FogOfWar.Instance.IsPositionRevealed(transform.position);
        foreach (var r in allRenderers)
        {
            if (r != null) r.enabled = !inFog;
        }
        foreach (var c in allColliders)
        {
            if (c != null) c.enabled = !inFog;
        }
        if (linkLine != null) linkLine.enabled = !inFog;

        UpdateLineRenderer();

        if (lockedTarget != null && !lockedTarget.isDead)
        {
            targetLastPosition = lockedTarget.transform.position;
        }

        switch (state)
        {
            case State.Flying:
                FlyToTarget();
                break;
            case State.Bombing:
                Bomb();
                break;
            case State.Retreating:
                Retreat();
                break;
        }
    }

    void FlyToTarget()
    {
        Vector3 dir = targetLastPosition - transform.position;
        dir.y = 0;
        if (dir.magnitude < 1f)
        {
            state = State.Bombing;
        }
        else
        {
            Vector3 moveDir = dir.normalized;
            transform.position += moveDir * CurrentSpeed * Time.deltaTime;
            if (moveDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDir);
        }
    }

    void Bomb()
    {
        if (!hasBombed)
        {
            hasBombed = true;
            if (lockedTarget != null && !lockedTarget.isDead)
            {
                VFX.SpawnHitEffect(lockedTarget.transform.position);
                lockedTarget.TakeDamage(bombDamage);
            }
        }
        state = State.Retreating;
    }

    void Retreat()
    {
        Vector3 retreatDir = Vector3.forward;
        transform.position += retreatDir * CurrentSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(retreatDir);

        GridMap gm = FindObjectOfType<GridMap>();
        float exitZ = gm != null ? gm.MapMaxZ + 5f : 30f;
        if (transform.position.z > exitZ)
        {
            RetreatDestroy();
        }
    }

    void UpdateLineRenderer()
    {
        if (linkLine != null)
        {
            linkLine.SetPosition(0, transform.position + Vector3.up * 0.5f);
            linkLine.SetPosition(1, targetLastPosition);
        }
    }

}
