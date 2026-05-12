using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Vector3 worldOffset = new Vector3(0, 1.2f, 0);

    private Transform target;
    private Image fillImage;
    private Canvas canvas;
    private RectTransform canvasRt;
    private float barWidth = 4f;
    private float barHeight = 0.5f;

    void Awake()
    {
        target = transform.parent;
        SetupCanvas();
        CreateBarElements();
    }

    void Start()
    {
        // 世界空间Canvas必须在Start之后设置worldCamera（因为Camera.main在Awake时可能为null）
        if (canvas != null)
            canvas.worldCamera = Camera.main;
    }

    void SetupCanvas()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasRt = GetComponent<RectTransform>();
        if (canvasRt == null) canvasRt = gameObject.AddComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(barWidth, barHeight);
    }

    void CreateBarElements()
    {
        // 深色背景
        GameObject bgGo = new GameObject("BG");
        bgGo.transform.SetParent(transform, false);
        RectTransform bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // 红色填充条
        GameObject fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(bgGo.transform, false);
        RectTransform fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(1, 1);
        fillRt.pivot = new Vector2(0, 0.5f);
        fillRt.anchoredPosition = Vector2.zero;
        fillRt.sizeDelta = Vector2.zero;
        fillImage = fillGo.AddComponent<Image>();
        fillImage.color = Color.red;

        // 创建纯色纹理作为填充图像的Sprite（Filled模式需要Sprite才能正确渲染）
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        fillImage.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
    }

    void LateUpdate()
    {
        if (target != null && canvasRt != null)
        {
            // 位置跟随目标
            transform.position = target.position + worldOffset;
            // 面朝上方（俯视相机可见）
            transform.forward = Vector3.up;

            // 根据距离缩放，确保远处也不会太大
            if (Camera.main != null)
            {
                float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
                float scale = dist * 0.008f;
                canvasRt.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    public void SetHealth(float percent)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(percent);
    }
}
