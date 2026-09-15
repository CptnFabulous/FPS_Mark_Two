using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelArea : MonoBehaviour
{
    static List<LevelArea> _current = new List<LevelArea>();
    public static IReadOnlyList<LevelArea> presentInCurrentLevel => _current;
    
    Bounds b;
    bool boundsGenerated = false;

    private void Awake()
    {
        _current.Add(this);
    }
    private void OnDestroy()
    {
        _current.Remove(this);
    }

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
            if (PhysicsUtility.IsLayerInLayerMask(AIGridPoints.Current.environmentMask, c.gameObject.layer) == false) continue;
            b.Encapsulate(c.bounds);
        }

        boundsGenerated = true;
    }

    public bool Contains(Entity e) => Contains(e.transform.position);
    public bool Contains(Vector3 worldPosition) => bounds.Contains(worldPosition);
    public bool Contains(Bounds _bounds) => Contains(_bounds.ClosestPoint(bounds.center));

    public static LevelArea FindAreaOfPosition(Vector3 worldPosition)
    {
        foreach (LevelArea area in presentInCurrentLevel)
        {
            if (area.Contains(worldPosition)) return area;
        }
        return null;
    }
    public static LevelArea FindAreaOfBounds(Bounds bounds)
    {
        foreach (LevelArea area in presentInCurrentLevel)
        {
            if (area.Contains(bounds)) return area;
        }
        return null;
    }
    public static LevelArea FindCurrentArea(Entity e)
    {
        foreach (LevelArea area in presentInCurrentLevel)
        {
            if (area.Contains(e)) return area;
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
