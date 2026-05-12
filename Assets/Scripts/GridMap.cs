using UnityEngine;

public class GridMap : MonoBehaviour
{
    [Header("地图设置")]
    public float mapWidth = 68f;
    public float mapHeight = 46f;
    public float gridSpacing = 2f;
    public Color gridColor = new Color(0.533f, 0.533f, 0.533f);
    public float lineWidth = 0.05f;

    void Start()
    {
        CreateGroundPlane();
        CreateGridLines();
        CreateBoundaryLine();
    }

    void CreateGroundPlane()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "GroundPlane";
        plane.transform.SetParent(transform);
        plane.transform.position = Vector3.zero;
        plane.transform.localScale = new Vector3(mapWidth / 10f, 1, mapHeight / 10f);

        Renderer rend = plane.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.white;
            rend.material = mat;
        }
    }

    void CreateGridLines()
    {
        GameObject gridContainer = new GameObject("GridLines");
        gridContainer.transform.SetParent(transform);

        float halfW = mapWidth / 2f;
        float halfH = mapHeight / 2f;

        for (float x = -halfW; x <= halfW; x += gridSpacing)
        {
            CreateLine(new Vector3(x, 0.01f, -halfH), new Vector3(x, 0.01f, halfH), gridContainer.transform);
        }

        for (float z = -halfH; z <= halfH; z += gridSpacing)
        {
            CreateLine(new Vector3(-halfW, 0.01f, z), new Vector3(halfW, 0.01f, z), gridContainer.transform);
        }
    }

    void CreateBoundaryLine()
    {
        float halfW = mapWidth / 2f;
        GameObject midline = new GameObject("Midline");
        midline.transform.SetParent(transform);
        LineRenderer lr = midline.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(-halfW, 0.02f, 0));
        lr.SetPosition(1, new Vector3(halfW, 0.02f, 0));
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.yellow;
        lr.endColor = Color.yellow;
    }

    void CreateLine(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(parent);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridColor;
        lr.endColor = gridColor;
    }

    public bool IsPlayerArea(Vector3 position)
    {
        return position.z < 0 && Mathf.Abs(position.x) <= mapWidth / 2f && position.z >= -mapHeight / 2f;
    }

    public float MapMinX => -mapWidth / 2f;
    public float MapMaxX => mapWidth / 2f;
    public float MapMinZ => -mapHeight / 2f;
    public float MapMaxZ => mapHeight / 2f;
}
