using UnityEngine;

public static class VFX
{
    public static void SpawnTrail(Transform parent, Color color, float size = 0.3f)
    {
        GameObject go = new GameObject("Trail");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.4f;
        main.startSpeed = 0.2f;
        main.startSize = size;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.duration = 60f;

        var emission = ps.emission;
        emission.rateOverTime = 30f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 5f;
        shape.radius = 0.05f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
    }

    public static void SpawnExplosion(Vector3 position, float size = 2f)
    {
        GameObject go = new GameObject("Explosion");
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed = size * 3f;
        main.startSize = size * 0.5f;
        main.startColor = new Color(1f, 0.5f, 0f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.burstCount = 1;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 30));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.7f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.2f, 0f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
        Object.Destroy(go, 2f);
    }

    public static void SpawnEnemyExplosion(Vector3 position)
    {
        GameObject go = new GameObject("EnemyExplosion");
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 4f;
        main.startSize = 0.8f;
        main.startColor = new Color(1f, 0.3f, 0f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.duration = 1f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.burstCount = 1;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 25));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
        Object.Destroy(go, 2f);
    }

    public static void SpawnHitEffect(Vector3 position)
    {
        GameObject go = new GameObject("Hit");
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 2f;
        main.startSize = 0.4f;
        main.startColor = Color.yellow;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.duration = 1f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.burstCount = 1;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 15));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
        Object.Destroy(go, 2f);
    }
}
