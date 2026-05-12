using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private Font uiFont;

    void Start()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null)
        {
            uiFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
            if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        CreateMenuUI();
    }

    void CreateMenuUI()
    {
        GameObject canvasGo = new GameObject("MenuCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 标题
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(canvasGo.transform, false);
        RectTransform trt = titleGo.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0, 180);
        trt.sizeDelta = new Vector2(600, 80);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "RTS 塔防";
        titleText.fontSize = 48;
        titleText.font = uiFont;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        // 开始游戏
        CreateButton("开始游戏", canvasGo.transform, new Vector2(0, 40), () =>
        {
            SceneManager.LoadScene("MainScene");
        });

        // 退出游戏
        CreateButton("退出游戏", canvasGo.transform, new Vector2(0, -60), () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    void CreateButton(string label, Transform parent, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300, 70);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.35f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.25f, 0.25f, 0.55f);
        cb.pressedColor = new Color(0.1f, 0.1f, 0.25f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        Text text = textGo.AddComponent<Text>();
        text.text = label;
        text.fontSize = 28;
        text.font = uiFont;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }
}
