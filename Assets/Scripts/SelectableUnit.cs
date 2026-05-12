using UnityEngine;

public class SelectableUnit : MonoBehaviour
{
    public bool isSelected = false;

    private Renderer[] renderers;
    private Color[] originalColors;
    private GameObject selectionRing;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                originalColors[i] = renderers[i].material.color;
        }
    }

    public void Select()
    {
        if (isSelected) return;
        isSelected = true;
        CreateSelectionRing();
        HighlightRenderers(true);
    }

    public void Deselect()
    {
        if (!isSelected) return;
        isSelected = false;
        if (selectionRing != null) Destroy(selectionRing);
        HighlightRenderers(false);
    }

    void CreateSelectionRing()
    {
        selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        selectionRing.name = "SelectionRing";
        selectionRing.transform.SetParent(transform);
        selectionRing.transform.localPosition = new Vector3(0, -0.4f, 0);
        selectionRing.transform.localScale = new Vector3(1.3f, 0.05f, 1.3f);
        Destroy(selectionRing.GetComponent<Collider>());

        Renderer r = selectionRing.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.85f, 0f, 0.5f);
        mat.SetFloat("_Glossiness", 0f);
        r.material = mat;
    }

    void HighlightRenderers(bool highlight)
    {
        foreach (var r in renderers)
        {
            if (r != null && r.material.HasProperty("_Color"))
            {
                Color c = r.material.color;
                if (highlight)
                    r.material.color = new Color(c.r * 1.5f, c.g * 1.5f, c.b * 1.5f);
                else
                    r.material.color = new Color(c.r / 1.5f, c.g / 1.5f, c.b / 1.5f);
            }
        }
    }
}
