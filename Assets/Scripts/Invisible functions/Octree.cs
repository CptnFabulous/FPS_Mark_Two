using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OctantType
{
    Branch, // Contains a mixture of empty and full space
    Empty, // Completely empty, nothing to check further
    Leaf, // Not empty but cannot be subdivided further
    Full // No empty space in it or any children
}

public class Octant<T>
{
    public int depth { get; private set; }
    public Vector3Int min { get; private set; }
    public Vector3Int max { get; private set; } // TO DO: delete this? Depth and min are the important ones because everything else can be calculated from those
    public Octant<T>[] children { get; private set; } = new Octant<T>[8];
    public OctantType type { get; private set; } = OctantType.Empty;
    public T leafData => _leafData;

    T _leafData;

    public bool hasChildren => type == OctantType.Branch || type == OctantType.Full;

    public void Refresh(Octree<T> origin, int sizePower)
    {
        // Calculate subdivision depth and size of octant
        depth = sizePower;
        int size = Octree<T>.CalculateGridSize(depth);
        Vector3Int dimensions = new Vector3Int(size, size, size);
        max = min + dimensions;
        bool notEmpty = origin.checkOctant.Invoke(min, max);

        // If no more children can be found, record leaf data and return
        if (!notEmpty)
        {
            Mark(origin, OctantType.Empty);
            return;
        }

        // If size at depth is 1 or less, that means we've subdivided as far as we can go.
        if (size <= 1)
        {
            Mark(origin, OctantType.Leaf);
            return;
        }

        // Set up a value so we can check if this octant is completely full
        bool isFull = true;

        // Check in child octants
        int childSize = size / 2;
        for (int i = 0; i < 8; i++)
        {
            // Calculate check min and max
            Vector3Int offset = SmokeParticleDensityController.neighbourOffsets[i];
            offset.x *= childSize;
            offset.y *= childSize;
            offset.z *= childSize;
            Vector3Int childMin = min + offset;

            // Set up child octant
            if (children[i] == null) children[i] = new Octant<T>();
            children[i].min = childMin;
            // Check data in child octant
            children[i].Refresh(origin, depth - 1);

            // Check if a child octant is completely full (either it's a leaf node or all its children are)
            bool childIsFull = children[i].type == OctantType.Leaf || children[i].type == OctantType.Full;
            // If a single child is not full, then that means this octant is not full.
            isFull &= childIsFull;
        }

        if (isFull)
        {
            // Mark as full, because all children are full
            Mark(origin, OctantType.Full);
        }
        else
        {
            // Mark as branch, because there's a mixture of full and non-full
            Mark(origin, OctantType.Branch);
        }
    }
    public void IterateThrough(Octree<T> origin)
    {
        // Perform function and assign leaf data
        origin.onOctantRefreshed.Invoke(this, ref _leafData);
        // Do the same for all its children
        if (!hasChildren) return;
        for (int i = 0; i < 8; i++)
        {
            children[i].IterateThrough(origin);
        }
    }
    public void DrawGizmos(Octree<T> origin, int sizePower)
    {
        // Size
        int sizeAlongAxis = Octree<T>.CalculateGridSize(sizePower);
        Vector3 size = new Vector3(sizeAlongAxis, sizeAlongAxis, sizeAlongAxis);
        Vector3 centre = min + (size / 2);

        // Draw different gizmos based on each cell type
        switch (type)
        {
            case OctantType.Empty:
                // Colour based on size
                float colourLerp = 1 - ((float)sizePower / (float)origin.subdivisions);
                Gizmos.color = Color.Lerp(Color.white, Color.black, colourLerp);
                // Draw
                Gizmos.DrawWireCube(centre, size);
                break;

            case OctantType.Leaf:
                // Colour the cell differently to show it's the end of the subdivision
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(centre, size);
                break;

            case OctantType.Full:
                // Colour the cell differently to show it's the end of the subdivision
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(centre, size);
                break;

            default:
                // Draw gizmos for each child
                for (int i = 0; i < 8; i++)
                {
                    children[i].DrawGizmos(origin, sizePower - 1);
                }
                break;
        }
    }

    void Mark(Octree<T> origin, OctantType type)
    {
        this.type = type;

        // Clear data (if delegate exists)
        if (origin.onOctantRemoved == null) return;

        // If octant is not meant to have children, clear its data
        // If meant to be empty, clear self
        Empty(origin, type == OctantType.Empty, !hasChildren, true);
    }

    void Empty(Octree<T> origin, bool emptySelf = true, bool emptyChildren = true, bool deleteChildren = true)
    {
        if (emptyChildren)
        {
            for (int i = 0; i < 8; i++)
            {
                if (children[i] == null) continue;

                // Empty all child octants (and their children)
                children[i].Empty(origin, true, true, deleteChildren);
                // Clear this space (if specified)
                if (deleteChildren) children[i] = null;
            }
        }

        // Clear self as well, if specified
        if (emptySelf)
        {
            origin.onOctantRemoved.Invoke(this);
            _leafData = default;
        }
    }
}
public class Octree<T>
{
    public delegate void LeafDataAssignment(Octant<T> octant, ref T data);
    
    public int subdivisions;
    public System.Func<Vector3Int, Vector3Int, bool> checkOctant;
    public LeafDataAssignment onOctantRefreshed;
    public System.Action<Octant<T>> onOctantRemoved;

    Octant<T> _base = new Octant<T>();

    public Octant<T> baseContainer => _base;
    
    public void Refresh()
    {
        _base.Refresh(this, subdivisions);
        if (onOctantRefreshed != null) _base.IterateThrough(this);
    }
    public void DrawGizmos() => _base.DrawGizmos(this, subdivisions);

    public static int CalculateGridSize(int sizePower) => Mathf.RoundToInt(Mathf.Pow(2, sizePower));
}