using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("资源")]
    public int gold = 500;
    public int goldPerInterval = 20;
    public float goldInterval = 10f;
    private float goldTimer;

    [Header("波次")]
    public int currentWave = 0;
    public int maxWaves = 20;
    public float waveInterval = 30f;
    public float waveTimer;
    public int enemiesAliveInWave;
    public int enemiesTotalInWave;
    public bool isWaveActive;

    [Header("状态")]
    public bool isGameOver = false;
    public bool isVictory = false;
    public bool isHacked = false;

    private const float HACK_DURATION = 12f;
    private const float HACK_COOLDOWN = 120f;
    private float hackEndTime = -1f;
    private float lastHackTime = -300f;

    public PlayerBase playerBase;

    public System.Action<int> OnGoldChanged;
    public System.Action OnGameOver;
    public System.Action OnVictory;
    public System.Action<bool> OnHackChanged;
    public System.Action OnWaveChanged;

    [Header("升级")]
    public int baseUpgradeCount = 0;

    [Header("EMP电磁波")]
    public bool isEmpActive;
    public float empSlowFactor = 0.15f;
    private float empEndTime;
    private float lastEmpTime = -180f;
    private const float EMP_COOLDOWN = 180f;
    private const float EMP_DURATION = 20f;
    private const int EMP_COST = 200;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        goldTimer = goldInterval;
        waveTimer = 15f;
    }

    void Update()
    {
        if (isGameOver || isVictory) return;

        // 黑客状态
        if (isHacked && Time.time >= hackEndTime)
        {
            isHacked = false;
            OnHackChanged?.Invoke(false);
        }

        // EMP状态
        if (isEmpActive && Time.time >= empEndTime)
        {
            isEmpActive = false;
        }

        // 金币产出
        goldTimer -= Time.deltaTime;
        if (goldTimer <= 0)
        {
            goldTimer = goldInterval;
            gold += goldPerInterval;
            OnGoldChanged?.Invoke(gold);
        }

        // 波次管理
        if (!isWaveActive)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0)
            {
                StartNextWave();
            }
        }
    }

    void StartNextWave()
    {
        currentWave++;
        enemiesTotalInWave = currentWave;
        enemiesAliveInWave = 0;
        isWaveActive = true;
        OnWaveChanged?.Invoke();
    }

    public void OnEnemySpawned()
    {
        enemiesAliveInWave++;
    }

    public void OnEnemyDestroyed()
    {
        AddGold(50); // 摧毁敌人奖励
        enemiesAliveInWave--;
        if (enemiesAliveInWave <= 0 && isWaveActive)
        {
            EndWave();
        }
    }

    public void EndWave()
    {
        isWaveActive = false;
        if (currentWave >= maxWaves)
        {
            Win();
        }
        else
        {
            waveTimer = waveInterval;
            OnWaveChanged?.Invoke();
        }
    }

    void Win()
    {
        isVictory = true;
        OnVictory?.Invoke();
    }

    public float GetWaveCountdown()
    {
        return Mathf.Max(0, waveTimer);
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    public int GetBaseUpgradeCost()
    {
        return 150 + baseUpgradeCount * 100;
    }

    public int GetBaseIncome()
    {
        return 20 + baseUpgradeCount * 20;
    }

    public void UpgradeBase()
    {
        int cost = GetBaseUpgradeCost();
        if (SpendGold(cost))
        {
            baseUpgradeCount++;
            goldPerInterval = GetBaseIncome();
        }
    }

    public void GameOver()
    {
        if (isGameOver || isVictory) return;
        isGameOver = true;
        CancelInvoke();
        OnGameOver?.Invoke();
    }

    public bool CanUseHack()
    {
        return Time.time - lastHackTime >= HACK_COOLDOWN && !isHacked;
    }

    public void ActivateHack()
    {
        if (!CanUseHack()) return;
        isHacked = true;
        hackEndTime = Time.time + HACK_DURATION;
        lastHackTime = Time.time;
        OnHackChanged?.Invoke(true);
    }

    public float GetHackCooldownRemaining()
    {
        return Mathf.Max(0, HACK_COOLDOWN - (Time.time - lastHackTime));
    }

    public float GetHackDurationRemaining()
    {
        if (!isHacked) return 0;
        return Mathf.Max(0, hackEndTime - Time.time);
    }

    public bool CanUseEmp()
    {
        return Time.time - lastEmpTime >= EMP_COOLDOWN && !isEmpActive;
    }

    public void ActivateEmp()
    {
        if (!CanUseEmp()) return;
        if (!SpendGold(EMP_COST)) return;
        isEmpActive = true;
        empEndTime = Time.time + EMP_DURATION;
        lastEmpTime = Time.time;
    }

    public float GetEmpCooldownRemaining()
    {
        return Mathf.Max(0, EMP_COOLDOWN - (Time.time - lastEmpTime));
    }

    public float GetEmpDurationRemaining()
    {
        if (!isEmpActive) return 0;
        return Mathf.Max(0, empEndTime - Time.time);
    }
}
