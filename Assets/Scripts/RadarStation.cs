using UnityEngine;

public class RadarStation : UnitBase
{
    [Header("雷达站属性")]
    public bool isActiveMode = false;
    public float silentVisionRadius = 3f;
    public float activeVisionRadius = 10f;
    public float modeSwitchCooldown = 0.1f;
    private float lastSwitchTime = -0.1f;

    protected override void Awake()
    {
        base.Awake();
        unitName = "雷达站";
        maxHealth = 300;
        currentHealth = maxHealth;
        visionRadius = silentVisionRadius;
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.size = new Vector3(0.5f, 0.33f, 0.5f);
    }

    void Update()
    {
        if (isDead) return;
        visionRadius = isActiveMode ? activeVisionRadius : silentVisionRadius;
    }

    public bool CanSwitchMode()
    {
        return Time.time - lastSwitchTime >= modeSwitchCooldown;
    }

    public void ToggleMode()
    {
        if (!CanSwitchMode()) return;
        isActiveMode = !isActiveMode;
        lastSwitchTime = Time.time;
    }

    public float GetSwitchCooldownRemaining()
    {
        return Mathf.Max(0, modeSwitchCooldown - (Time.time - lastSwitchTime));
    }
}
