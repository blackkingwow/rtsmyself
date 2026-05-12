using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    [Header("设置")]
    public int textureSize = 512;
    public Color fogColor = new Color(0, 0, 0, 0.85f);

    private Texture2D fogTexture;
    private Color[] fogPixels;
    private GridMap gridMap;
    private float mapWidth;
    private float mapHeight;
    private int frameCounter = 0;
    private const int UPDATE_INTERVAL = 4;

    public static FogOfWar Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gridMap = FindObjectOfType<GridMap>();
        if (gridMap != null)
        {
            mapWidth = gridMap.MapMaxX - gridMap.MapMinX;
            mapHeight = gridMap.MapMaxZ - gridMap.MapMinZ;
        }
        else
        {
            mapWidth = 90f;
            mapHeight = 60f;
        }

        fogTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        fogTexture.wrapMode = TextureWrapMode.Clamp;
        fogTexture.filterMode = FilterMode.Bilinear;

        fogPixels = new Color[textureSize * textureSize];
        for (int i = 0; i < fogPixels.Length; i++)
            fogPixels[i] = fogColor;

        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();

        CreateFogPlane();
    }

    void CreateFogPlane()
    {
        // 在世界空间中创建覆盖整个地图的雾效平面
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        float hw = mapWidth / 2f;
        float hh = mapHeight / 2f;

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-hw, 0.05f, -hh),
            new Vector3( hw, 0.05f, -hh),
            new Vector3(-hw, 0.05f,  hh),
            new Vector3( hw, 0.05f,  hh)
        };
        mesh.uv = new Vector2[] {
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 1), new Vector2(1, 1)
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = fogTexture;
        mat.color = Color.white;
        mr.material = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    void LateUpdate()
    {
        if (GameManager.Instance == null) return;

        frameCounter++;
        if (frameCounter < UPDATE_INTERVAL) return;
        frameCounter = 0;

        if (GameManager.Instance.isHacked)
        {
            for (int i = 0; i < fogPixels.Length; i++)
                fogPixels[i] = Color.clear;
        }
        else
        {
            for (int i = 0; i < fogPixels.Length; i++)
                fogPixels[i] = fogColor;

            UnitBase[] allUnits = FindObjectsOfType<UnitBase>();
            foreach (var unit in allUnits)
            {
                if (unit.isDead) continue;
                // 只有友方单位揭示迷雾
                if (unit.CompareTag("PlayerUnit") || unit.CompareTag("Building") || unit is PlayerBase)
                    DrawVisionCircle(unit.transform.position, unit.visionRadius);
            }
        }

        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
    }

    public bool IsPositionRevealed(Vector3 worldPos)
    {
        if (GameManager.Instance != null && GameManager.Instance.isHacked)
            return true;

        UnitBase[] allUnits = FindObjectsOfType<UnitBase>();
        foreach (var unit in allUnits)
        {
            if (unit.isDead) continue;
            if (unit.CompareTag("PlayerUnit") || unit.CompareTag("Building") || unit is PlayerBase)
            {
                float dx = worldPos.x - unit.transform.position.x;
                float dz = worldPos.z - unit.transform.position.z;
                if (dx * dx + dz * dz <= unit.visionRadius * unit.visionRadius)
                    return true;
            }
        }
        return false;
    }

    void DrawVisionCircle(Vector3 worldPos, float radius)
    {
        float halfW = mapWidth / 2f;
        float halfH = mapHeight / 2f;

        int cx = Mathf.RoundToInt((worldPos.x + halfW) / mapWidth * textureSize);
        int cy = Mathf.RoundToInt((worldPos.z + halfH) / mapHeight * textureSize);
        // 分别计算X和Z方向的像素半径，确保世界空间中是正圆
        int rx = Mathf.RoundToInt(radius / mapWidth * textureSize);
        int rz = Mathf.RoundToInt(radius / mapHeight * textureSize);

        for (int y = -rz; y <= rz; y++)
        {
            for (int x = -rx; x <= rx; x++)
            {
                float nx = (float)x / rx;
                float ny = (float)y / rz;
                if (nx * nx + ny * ny <= 1f)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < textureSize && py >= 0 && py < textureSize)
                        fogPixels[py * textureSize + px] = Color.clear;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
