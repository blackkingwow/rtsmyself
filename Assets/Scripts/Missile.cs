using UnityEngine;

public class Missile : MonoBehaviour
{
    public static GameObject modelPrefab;

    public float speed = 20f;
    public int damage = 80;
    private EnemyPlane target;
    private Vector3 lastTargetPos;

    public void SetTarget(EnemyPlane enemy)
    {
        target = enemy;
        if (target != null)
            lastTargetPos = target.transform.position;
    }

    public void CreateVisual()
    {
        if (modelPrefab != null)
        {
            GameObject model = Instantiate(modelPrefab, transform);
            model.transform.localPosition = Vector3.zero;
        }
    }

    public void CreateTrail()
    {
        VFX.SpawnTrail(transform, new Color(1f, 0.6f, 0.1f), 0.3f);
    }

    void Start()
    {
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        if (target != null && !target.isDead)
            lastTargetPos = target.transform.position;

        Vector3 dir = lastTargetPos - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        if (dist > 0.01f)
        {
            transform.position += dir.normalized * speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        if (dist < 0.8f)
        {
            if (target != null && !target.isDead)
            {
                VFX.SpawnHitEffect(transform.position);
                target.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        if (target == null || target.isDead)
        {
            if (dist < 2f)
                Destroy(gameObject);
        }
    }
}
