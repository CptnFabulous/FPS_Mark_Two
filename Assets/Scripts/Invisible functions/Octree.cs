using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public enum OctantType
{
    Branch, // Contains a mixture of empty and full space
    Empty, // Completely empty, nothing to check further
    Leaf, // Not empty but cannot be subdivided further
    Full // No empty space in it or any children
}

public class Octant<T>// : IReadOnlyList<Octant<T>>
{
    public Octant(Octree<T> origin, Octant<T> parent, int subdivisions, int order)
    {
        this.origin = origin;
        this.parent = parent;
        this.subdivisions = subdivisions;
        this.order = order;
    }

    public Octree<T> origin { get; private set; }
    public Octant<T> parent { get; private set; }
    public int subdivisions { get; private set; }
    public int order { get; private set; }
    public BoundsInt bounds { get; private set; }
    public Vector3Int min => bounds.min;
    public Vector3Int max => bounds.max;

    public Octant<T>[] children { get; private set; } = new Octant<T>[8];
    public OctantType type { get; private set; } = OctantType.Empty;
    public T leafData => _leafData;

    T _leafData;

    public int layer => origin.subdivisions - subdivisions;

    //public bool hasChildren => type == OctantType.Branch || type == OctantType.Full;

    /// <summary>
    /// Check if octant meeting the criteria actually matters. If not, we can treat it as occupied even if it isn't.
    ///<para>If check is not present, treat as true by default.</para>
    /// </summary>
    /// <returns></returns>
    public bool DoesCriteriaMatterWithThisOctant() => (origin.doesCheckMatter == null) || origin.doesCheckMatter.Invoke(bounds);

    public void Refresh(Vector3Int boundsMin)
    {
        // Calculate subdivision depth and size of octant
        //this.subdivisions = subdivisions;
        int size = Octree<T>.CalculateGridSize(this.subdivisions);
        Vector3Int dimensions = new Vector3Int(size, size, size);

        BoundsInt newBounds = new BoundsInt();
        newBounds.min = boundsMin;
        newBounds.max = boundsMin + dimensions;
        bounds = newBounds;

        bool notEmpty = origin.checkOctant.Invoke(bounds);

        //Debug.Log($"Subdivisions = {subdivisions}, layer = {layer}, size = {size}");


        // If no more children can be found, record leaf data and return
        if (!notEmpty)
        {
            type = OctantType.Empty;
            return;
        }

        // If size at depth is 1 or less, that means we've subdivided as far as we can go.
        if (size <= 1)
        {
            type = OctantType.Leaf;
            return;
        }

        // Set up a value so we can check if this octant is completely full
        bool isFull = true;

        // Check in child octants
        int childSize = size / 2;
        for (int i = 0; i < 8; i++)
        {
            // Set up child octant
            if (children[i] == null)
            {
                children[i] = new Octant<T>(this.origin, this, this.subdivisions - 1, i);
            }

            // Calculate check min and max
            Vector3Int offset = SmokeParticleDensityController.neighbourOffsets[i];
            offset.x *= childSize;
            offset.y *= childSize;
            offset.z *= childSize;
            Vector3Int childMin = min + offset;
            //children[i].min = childMin;

            // Calculate data inside child octant
            children[i].Refresh(childMin);

            // Check if this octant can be counted as part of a 'full' parent octant
            bool childIsFull = false;
            childIsFull = childIsFull || children[i].type == OctantType.Leaf;
            childIsFull = childIsFull || children[i].type == OctantType.Full;
            childIsFull = childIsFull || children[i].DoesCriteriaMatterWithThisOctant() == false;

            // If a single child is not full, then that means this octant is not full.
            isFull &= childIsFull;
        }

        if (isFull)
        {
            // Mark as full, because all children are full
            type = OctantType.Full;
        }
        else
        {
            // Mark as branch, because there's a mixture of full and non-full
            type = OctantType.Branch;
        }
    }
    public void IterateThrough()
    {
        // Determine different outcomes for different types
        switch (type)
        {
            case OctantType.Empty:

                // Should be completely empty, scrub everything.
                // Empty self
                // Delete children
                Empty(origin, true, true, true);

                break;

            case OctantType.Leaf:

                // Should have data in self but not in children
                // Populate self
                // Delete children
                Empty(origin, false, true, true);
                origin.onOctantRefreshed.Invoke(this, ref _leafData);

                break;

            case OctantType.Full:

                // Needs to have data for itself.
                // Children should not have data (because it's covered by this), but still need to be present to prove this octant should be full.
                // Populate self
                // Empty children (but do not delete, the child data is necessary to prove this one is full)
                Empty(origin, false, true, false);
                origin.onOctantRefreshed.Invoke(this, ref _leafData);

                break;

            case OctantType.Branch:

                // Empty self
                // Iterate through children
                Empty(origin, true, false, false);
                for (int i = 0; i < 8; i++) children[i].IterateThrough();

                break;
        }
    }
    public void DrawGizmos()
    {
        // Size
        int sizeAlongAxis = Octree<T>.CalculateGridSize(subdivisions);
        Vector3 size = new Vector3(sizeAlongAxis, sizeAlongAxis, sizeAlongAxis);
        Vector3 centre = min + (size / 2);

        // Draw different gizmos based on each cell type
        switch (type)
        {
            case OctantType.Empty:
                // Colour based on size
                float colourLerp = 1 - ((float)subdivisions / (float)origin.subdivisions);
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
                for (int i = 0; i < 8; i++) children[i].DrawGizmos();
                break;
        }
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
                // Delete this octant (if specified)
                if (deleteChildren) children[i] = null;
            }
        }

        // Clear data from self (if clearing function has been assigned)
        if (emptySelf && origin.onOctantRemoved != null)
        {
            origin.onOctantRemoved.Invoke(this);
            _leafData = default;
        }
    }






    /*
    public int Count
    {
        get
        {
            // Add together the counts of each non-null octant
        }
    }

    public Octant<T> this[int index]
    {
        get
        {

        }
    }

    public IEnumerator<Octant<T>> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
    */
}

public class Octree<T>
{
    public delegate void LeafDataAssignment(Octant<T> octant, ref T data);
    
    public int subdivisions;
    public System.Func<BoundsInt, bool> checkOctant;
    public System.Func<BoundsInt, bool> doesCheckMatter;
    public LeafDataAssignment onOctantRefreshed;
    public System.Action<Octant<T>> onOctantRemoved;

    Octant<T> _base;

    public Octant<T> baseContainer => _base ??= new Octant<T>(this, null, subdivisions, 0);
    
    public void Refresh()
    {
        baseContainer.Refresh(Vector3Int.zero);
        if (onOctantRefreshed != null) baseContainer.IterateThrough();
    }
    public void DrawGizmos() => baseContainer.DrawGizmos();

    public static int CalculateGridSize(int divisions) => Mathf.RoundToInt(Mathf.Pow(2, divisions));







}