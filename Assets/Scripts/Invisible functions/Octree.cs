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
    // TO DO: make these values private, they should only be altered from a refresh calculation
    
    public int depth = 0;
    public Vector3Int min = Vector3Int.zero;
    public Vector3Int max = Vector3Int.zero; // TO DO: delete this? Depth and min are the important ones because everything else can be calculated from those
    public Octant<T>[] children = new Octant<T>[8];
    public OctantType type;
    public T leafData;

    //public Bounds bounds = new BoundsInt(MinAttribute, )
    public bool hasChildren => type == OctantType.Branch || type == OctantType.Full;
}
public class Octree<T>
{
    public int subdivisions;
    public System.Func<Vector3Int, Vector3Int, bool> checkOctant;
    public System.Func<Octant<T>, T> onOctantRefreshed;
    public System.Action<Octant<T>> onOctantRemoved;

    Octant<T> _base = new Octant<T>();

    public Octant<T> baseContainer => _base;
    
    public void Refresh()
    {
        RefreshOctant(_base, subdivisions);
        if (onOctantRefreshed != null) IterateThroughOctant(_base, onOctantRefreshed);
    }
    public void DrawGizmos() => DrawOctantGizmos(_base, subdivisions);

    void RefreshOctant(Octant<T> octant, int sizePower)
    {
        if (octant == null) return;

        // Calculate subdivision depth and size of octant
        octant.depth = sizePower;
        int size = CalculateGridSize(octant.depth);
        Vector3Int dimensions = new Vector3Int(size, size, size);
        octant.max = octant.min + dimensions;
        bool notEmpty = checkOctant.Invoke(octant.min, octant.max);

        // If no more children can be found, record leaf data and return
        if (!notEmpty)
        {
            MarkOctant(octant, OctantType.Empty);
            return;
        }

        // If size at depth is 1 or less, that means we've subdivided as far as we can go.
        if (size <= 1)
        {
            MarkOctant(octant, OctantType.Leaf);
            return;
        }

        // Set up a value so we can check if this octant is completely full
        bool isFull = true;

        // Check in child octants
        int childSize = size / 2;
        //Vector3Int childDimensions = new Vector3Int(childSize, childSize, childSize);
        for (int i = 0; i < 8; i++)
        {
            // Calculate check min and max
            Vector3Int offset = SmokeParticleDensityController.neighbourOffsets[i];
            offset.x *= childSize;
            offset.y *= childSize;
            offset.z *= childSize;
            Vector3Int childMin = octant.min + offset;
            //Vector3Int childMax = childMin + childDimensions;

            // Set up child octant
            if (octant.children[i] == null) octant.children[i] = new Octant<T>();
            octant.children[i].min = childMin;
            // Check data in child octant
            RefreshOctant(octant.children[i], octant.depth - 1);

            // Check if a child octant is completely full (either it's a leaf node or all its children are)
            bool childIsFull = octant.children[i].type == OctantType.Leaf || octant.children[i].type == OctantType.Full;
            // If a single child is not full, then that means this octant is not full.
            isFull &= childIsFull;
        }

        if (isFull)
        {
            // Mark as full, because all children are full
            MarkOctant(octant, OctantType.Full);
        }
        else
        {
            // Mark as branch, because there's a mixture of full and non-full
            MarkOctant(octant, OctantType.Branch);
        }
    }
    void MarkOctant(Octant<T> octant, OctantType type)
    {
        octant.type = type;

        // Clear data (if delegate exists)
        if (onOctantRemoved == null) return;

        // If octant is not meant to have children, clear its data
        if (!octant.hasChildren) ClearOctantChildren(octant);
        // If meant to be empty, clear self
        if (type == OctantType.Empty) onOctantRemoved.Invoke(octant);
    }

    void IterateThroughOctant(Octant<T> octant, System.Func<Octant<T>, T> action)
    {
        // Perform function and assign leaf data
        octant.leafData = action.Invoke(octant);
        // Do the same for all its children
        if (!octant.hasChildren) return;
        for (int i = 0; i < 8; i++)
        {
            IterateThroughOctant(octant.children[i], action);
        }
    }
    void ClearOctantChildren(Octant<T> octant)
    {
        if (octant == null) return;

        for (int i = 0; i < 8; i++)
        {
            // Delete/dismiss all data related to this octant (make sure to get its children too)
            ClearOctantChildren(octant.children[i]);
            onOctantRemoved.Invoke(octant);
            // Clear this space
            octant.children[i] = null;
        }
    }

    void DrawOctantGizmos(Octant<T> octant, int sizePower)
    {
        if (octant == null) return;

        // Size
        int sizeAlongAxis = CalculateGridSize(sizePower);
        Vector3 size = new Vector3(sizeAlongAxis, sizeAlongAxis, sizeAlongAxis);
        Vector3 centre = octant.min + (size / 2);

        switch (octant.type)
        {
            case OctantType.Empty:
                // Colour based on size
                float colourLerp = 1 - ((float)sizePower / (float)subdivisions);
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
                    DrawOctantGizmos(octant.children[i], sizePower - 1);
                }
                break;
        }
    }

    int CalculateGridSize(int sizePower) => Mathf.RoundToInt(Mathf.Pow(2, sizePower));
}