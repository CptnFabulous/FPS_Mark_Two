using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelArea : MonoBehaviour
{
    Bounds b;
    bool boundsGenerated = false;


    public Bounds bounds
    {
        get
        {
            if (!boundsGenerated) GenerateBounds();
            return b;
        }
    }

    [ContextMenu("Regenerate bounds")]
    void GenerateBounds()
    {
        Collider[] children = GetComponentsInChildren<Collider>();
        if (children == null) b = new Bounds();

        b = children[0].bounds;
        for (int i = 1; i < children.Length; i++)
        {
            Collider c = children[i];
            if (MiscFunctions.IsLayerInLayerMask(AIGridPoints.Current.environmentMask, c.gameObject.layer) == false) continue;
            b.Encapsulate(c.bounds);
        }

        boundsGenerated = true;
    }

    public bool Contains(Entity e) => Contains(e.transform.position);
    public bool Contains(Vector3 worldPosition) => bounds.Contains(worldPosition);

    public static LevelArea FindAreaOfPosition(Vector3 worldPosition)
    {
        foreach (LevelArea area in FindObjectsOfType<LevelArea>())
        {
            if (area.Contains(worldPosition)) return area;
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
