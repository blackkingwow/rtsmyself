using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("移动速度")]
    public float panSpeed = 15f;
    public float edgeScrollThreshold = 30f;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 40f;

    [Header("边界")]
    public float minX = -44f;
    public float maxX = 44f;
    public float minZ = -34f;
    public float maxZ = 34f;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging = false;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        HandleKeyboardMovement();
        HandleEdgeScrolling();
        HandleMouseDrag();
        HandleZoom();
        ClampPosition();
    }

    void HandleKeyboardMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            Vector3 move = new Vector3(h, 0, v) * panSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
        }
    }

    void HandleEdgeScrolling()
    {
        Vector3 move = Vector3.zero;
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;

        if (mouseX < edgeScrollThreshold && mouseX > 0)
            move += Vector3.left;
        if (mouseX > Screen.width - edgeScrollThreshold && mouseX < Screen.width)
            move += Vector3.right;
        if (mouseY < edgeScrollThreshold && mouseY > 0)
            move += Vector3.back;
        if (mouseY > Screen.height - edgeScrollThreshold && mouseY < Screen.height)
            move += Vector3.forward;

        transform.Translate(move * panSpeed * Time.deltaTime, Space.World);
    }

    void HandleMouseDrag()
    {
        // 中键拖拽
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - dragOrigin;
            dragOrigin = Input.mousePosition;

            // 屏幕像素转世界单位
            float worldDelta = delta.magnitude / Screen.height * cam.orthographicSize * 2f;
            Vector3 dir = new Vector3(-delta.x, 0, -delta.y).normalized;

            transform.Translate(dir * worldDelta * 0.5f, Space.World);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - scroll * zoomSpeed,
                minZoom,
                maxZoom
            );
        }
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }
}
