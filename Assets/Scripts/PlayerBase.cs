using UnityEngine;

public class PlayerBase : UnitBase
{
    [Header("基地特有")]
    public float repairCost = 50f;
    public int repairAmount = 100;
    public float repairCooldown = 5f;
    private float lastRepairTime = -5f;

    protected override void Awake()
    {
        base.Awake();
        unitName = "玩家基地";
        maxHealth = 1000;
        currentHealth = maxHealth;
        visionRadius = 3f;
    }

    protected override void Start()
    {
        base.Start();
        GameManager.Instance.playerBase = this;
    }

    public bool CanRepair()
    {
        return Time.time - lastRepairTime >= repairCooldown;
    }

    public void Repair(UnitBase target)
    {
        if (!CanRepair()) return;
        if (target == null || target.isDead) return;
        if (!GameManager.Instance.SpendGold((int)repairCost)) return;

        lastRepairTime = Time.time;
        target.Heal(repairAmount);
    }

    public float GetRepairCooldownRemaining()
    {
        return Mathf.Max(0, repairCooldown - (Time.time - lastRepairTime));
    }

    public bool CanUpgrade()
    {
        return GameManager.Instance.gold >= GameManager.Instance.GetBaseUpgradeCost();
    }

    protected override void Die()
    {
        base.Die();
        GameManager.Instance.GameOver();
    }
}
