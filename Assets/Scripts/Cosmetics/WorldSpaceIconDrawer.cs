using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using CptnFabulous.ObjectPool;

public static class WorldSpaceIconDrawer
{
    static BillboardIcon iconPrefab;
    static List<BillboardIcon> activeIcons;
    static bool setupComplete = false;

    public static void DrawIcon(Sprite sprite, Color colour, Transform parent, Vector3 localPosition, Vector2 scale, int renderLayer)
    {
        // First-time setup
        if (setupComplete == false)
        {
            Setup();
            setupComplete = true;
        }

        // Summon renderer, assign sprite and colour
        BillboardIcon newIcon = ObjectPool.RequestObject(iconPrefab);
        SpriteRenderer spriteRenderer = newIcon.spriteRenderer;
        spriteRenderer.gameObject.name = sprite.name;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = colour;

        // Position and size renderer
        Transform t = newIcon.transform;
        t.SetParent(parent);
        t.localPosition = localPosition;
        t.localScale = new Vector3(scale.x, scale.y, 1);
        newIcon.gameObject.layer = renderLayer;

        // Add icon
        activeIcons.Add(newIcon);
    }

    static void Setup()
    {
        // Create icon prefab
        GameObject iconObject = new GameObject("Icon Prefab");
        iconPrefab = iconObject.AddComponent<BillboardIcon>();
        iconPrefab.spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        iconObject.SetActive(false);

        // Create icon list
        activeIcons = new List<BillboardIcon>();
        // Add listener to clear icons once frame is rendered
        RenderPipelineManager.endContextRendering += ClearIconsForNextFrame;
    }
    static void ClearIconsForNextFrame(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (BillboardIcon icon in activeIcons)
        {
            ObjectPool.DismissObject(icon);
        }
        activeIcons.Clear();
    }
}