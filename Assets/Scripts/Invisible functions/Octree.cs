using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Octant(Octree<T> origin, Octant<T> parent, int subdivisions, string key)
    {
        this.origin = origin;
        this.parent = parent;
        this.subdivisions = subdivisions;
        this.key = key;
    }

    public Octree<T> origin { get; private set; }
    public Octant<T> parent { get; private set; }
    public int subdivisions { get; private set; }
    public string key { get; private set; }
    public BoundsInt bounds { get; private set; }
    public Vector3Int min => bounds.min;
    public Vector3Int max => bounds.max;

    public Octant<T>[] children { get; private set; } = new Octant<T>[8];
    public OctantType type { get; private set; } = OctantType.Empty;
    public T leafData => _leafData;

    T _leafData;

    public int layer => origin.subdivisions - subdivisions;
    public System.Func<BoundsInt, bool> meetsCriteria => origin.checkOctant;
    public Octree<T>.LeafDataAssignment onOctantRefreshed => origin.onOctantRefreshed;
    public System.Action<Octant<T>> onOctantRemoved => origin.onOctantRemoved;


    /// <summary>
    /// Check if octant meeting the criteria actually matters. If not, we can treat it as occupied even if it isn't.
    ///<para>If check is not present, treat as true by default.</para>
    /// </summary>
    /// <returns></returns>
    public bool DoesCriteriaMatterWithThisOctant() => (origin.doesCheckMatter == null) || origin.doesCheckMatter.Invoke(this);

    public void Scan(Vector3Int boundsMin)
    {
        // Calculate subdivision depth and size of octant
        int size = Octree<T>.CalculateGridSize(subdivisions);
        Vector3Int dimensions = new Vector3Int(size, size, size);

        BoundsInt newBounds = new BoundsInt();
        newBounds.min = boundsMin;
        newBounds.max = boundsMin + dimensions;
        bounds = newBounds;

        bool notEmpty = meetsCriteria.Invoke(bounds);

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

        // Check in child octants
        int childSize = size / 2;

        // Set up a value so we can check if this octant is completely full
        bool isFull = true;

        for (int i = 0; i < 8; i++)
        {
            // Set up child octant
            if (children[i] == null)
            {
                children[i] = new Octant<T>(origin, this, subdivisions - 1, key + i.ToString());
            }

            // Calculate check min and max
            Vector3Int offset = SmokeParticleDensityController.neighbourOffsets[i];
            offset.x *= childSize;
            offset.y *= childSize;
            offset.z *= childSize;
            Vector3Int childMin = min + offset;

            // Calculate data inside child octant
            children[i].Scan(childMin);

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

    public void ProcessAllCells()
    {
        IEnumerator enumerator = CellIterator(true);
        while (enumerator.MoveNext()) continue;
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

    IEnumerator<Octant<T>> EmptyIterator(bool processCells, bool emptySelf, bool emptyChildren, bool deleteChildren)
    {
        if (emptyChildren)
        {
            for (int i = 0; i < 8; i++)
            {
                if (children[i] == null) continue;

                // For each child present, yield on its EmptyChildren() as well before proceeding

                IEnumerator<Octant<T>> childEnumerator = children[i].EmptyIterator(processCells, true, true, deleteChildren);
                while (childEnumerator.MoveNext()) yield return childEnumerator.Current;
                // Should work in theory, but seems not to in practice
                //yield return children[i].EmptyIterator(processCells, true, true, deleteChildren);

                // Clear child octant from its array position
                if (deleteChildren) children[i] = null;
            }
        }

        // Yield if this octant needs to be emptied
        if (emptySelf && onOctantRemoved != null)
        {
            if (processCells)
            {
                onOctantRemoved.Invoke(this);
                _leafData = default;
            }
            yield return this;
        }
    }


    public IEnumerator<Octant<T>> CellIterator(bool processCells)
    {
        IEnumerator<Octant<T>> emptyIterator = type switch
        {
            OctantType.Empty => EmptyIterator(processCells, true, true, true), // Completely empty
            OctantType.Leaf => EmptyIterator(processCells, false, true, true), // Retain self but have no children
            OctantType.Full => EmptyIterator(processCells, false, true, false), // Retain self, empty children but don't delete them
            OctantType.Branch => EmptyIterator(processCells, true, false, false), // Empty self but keep children as-is
        };
        while (emptyIterator.MoveNext()) yield return emptyIterator.Current;



        // TO DO: I need to modify the events to run for all kinds of states.
        // E.g. once I make a 3D pathfinding system, I want to use the empty octants as the nodes, not the full ones.
        // Or maybe I should change 'empty' and 'full' to something like 'invalid' and 'valid'?
        // Check if a space is empty.
        // If space is not empty, break it down further to check which parts are empty and which aren't.
        
        
        
        
        
        
        
        
        
        
        switch (type)
        {
            case OctantType.Leaf:
            case OctantType.Full:

                // Yield if this octant needs to be populated
                if (processCells && onOctantRefreshed != null)
                {
                    onOctantRefreshed.Invoke(this, ref _leafData);
                }
                yield return this;

                break;

        }

        if (type != OctantType.Branch) yield break;

        // If non-empty children are present, continue iterating through them.
        for (int i = 0; i < 8; i++)
        {
            IEnumerator<Octant<T>> childEnumerator = children[i].CellIterator(processCells);
            while (childEnumerator.MoveNext()) yield return childEnumerator.Current;
        }
    }
    
}

public class Octree<T>
{
    public delegate void LeafDataAssignment(Octant<T> octant, ref T data);
    
    public int subdivisions;
    public System.Func<BoundsInt, bool> checkOctant;
    public System.Func<Octant<T>, bool> doesCheckMatter;
    public LeafDataAssignment onOctantRefreshed;
    public System.Action<Octant<T>> onOctantRemoved;

    Octant<T> _base;

    public Octant<T> baseContainer => _base ??= new Octant<T>(this, null, subdivisions, "");

    
    public int maxPossibleUniqueCells
    {
        get
        {
            int final = 0;
            for (int i = 0; i < subdivisions; i++)
            {
                final += Mathf.RoundToInt(Mathf.Pow(8, i));
            }
            return final;
        }
    }
    
    public void Refresh()
    {
        Scan();
        if (onOctantRefreshed != null) baseContainer.ProcessAllCells();
    }
    public void Scan() => baseContainer.Scan(Vector3Int.zero);

    public void DrawGizmos() => baseContainer.DrawGizmos();

    public static int CalculateGridSize(int divisions) => Mathf.RoundToInt(Mathf.Pow(2, divisions));

    /// <summary>
    /// Using a numerical key generated from another octant, find one in the same position in this octree.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Octant<T> GetOctantByKey(string key)
    {
        Octant<T> latest = baseContainer;

        // Iterate through each character in the key, parsed into a number.
        // If the string is empty, it returns the base container.
        for (int i = 0; i < key.Length; i++)
        {
            if (MiscFunctions.TryParseCharToInt(key[i], out int index) == false) return latest;

            // Get the child. If no more children are found, return latest valid result.
            Octant<T> child = latest.children[index];
            if (child == null) return latest;

            // Assign child octant so we can check it in the next loop
            latest = child;
        }

        return latest;
    }
    public Octant<T> FindMatchingOctant<T1>(Octant<T1> otherOctant) => GetOctantByKey(otherOctant.key);

}