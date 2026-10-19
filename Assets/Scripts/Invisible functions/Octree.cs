using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CptnFabulous.MiscUtility;

public enum OctantType
{
    Branch, // Contains a mixture of valid and invalid spaces
    Invalid, // Nothing in this space meets criteria
    Leaf, // Meets criteria, but cannot be subdivided further
    FullyValid, // All children count for criteria
}

public class Octant<T>
{
    public Octant(Octree<T> origin, Octant<T> parent, int subdivisions, Vector3Int boundsMin, string key)
    {
        this.origin = origin;
        this.parent = parent;
        this.subdivisions = subdivisions;
        this.key = key;

        // Create bounds
        int size = Octree<T>.CalculateGridSize(subdivisions);
        Vector3Int dimensions = new Vector3Int(size, size, size);

        BoundsInt newBounds = new BoundsInt();
        newBounds.min = boundsMin;
        newBounds.max = boundsMin + dimensions;
        bounds = newBounds;
    }
    
    public static Octant<T> FromParent(Octant<T> parent, int orderIndex)
    {
        int childSize = parent.size / 2; // Calculate size that each child octant needs to be
        // Calculate check min and max
        Vector3Int offset = VoxelUtility.corners[orderIndex];
        offset.x *= childSize;
        offset.y *= childSize;
        offset.z *= childSize;
        Vector3Int childMin = parent.min + offset;
        return new Octant<T>(parent.origin, parent, parent.subdivisions - 1, childMin, parent.key + orderIndex.ToString());
    }

    public Octree<T> origin { get; private set; }
    public Octant<T> parent { get; private set; }
    public int subdivisions { get; private set; }
    public string key { get; private set; }
    public BoundsInt bounds { get; private set; }
    public Vector3Int min => bounds.min;
    public Vector3Int max => bounds.max;


    public int depth => key.Length;
    public int size => Octree<T>.CalculateGridSize(subdivisions);

    public Octant<T>[] children { get; private set; } = new Octant<T>[8];
    public OctantType type { get; private set; } = OctantType.Invalid;
    public T leafData => _leafData;

    T _leafData;

    public int layer => origin.subdivisions - subdivisions;
    public System.Func<BoundsInt, bool> meetsCriteria => origin.checkOctant;
    public Octree<T>.LeafDataAssignment refreshOctant => origin.refreshOctant;
    public Octree<T>.LeafDataAssignment clearOctant => origin.clearOctant;


    /// <summary>
    /// Check if octant meeting the criteria actually matters. If not, we can treat it as occupied even if it isn't.
    ///<para>If check is not present, treat as true by default.</para>
    /// </summary>
    /// <returns></returns>
    public bool DoesCriteriaMatterWithThisOctant() => (origin.doesCheckMatter == null) || origin.doesCheckMatter.Invoke(this);

    public IEnumerator<Octant<T>> ScanIterator()
    {
        // Check if this octant meets criteria
        bool notEmpty = meetsCriteria.Invoke(bounds);
        yield return this;

        // If no more children can be found, mark as totally invalid
        if (!notEmpty)
        {
            type = OctantType.Invalid;
            yield break;
        }

        // Calculate size
        int size = this.size;

        // If size at depth is 1 or less, that means we've subdivided as far as we can go.
        if (size <= 1)
        {
            type = OctantType.Leaf;
            yield break;
        }

        // Check in child octants
        //int childSize = size / 2; // Calculate size that each child octant needs to be
        bool isFull = true; // Set up a value so we can check if this octant is completely full

        for (int i = 0; i < 8; i++)
        {
            // Set up child octant
            if (children[i] == null)
            {
                /*
                // Calculate check min and max
                Vector3Int offset = SmokeParticleDensityController.neighbourOffsets[i];
                offset.x *= childSize;
                offset.y *= childSize;
                offset.z *= childSize;
                Vector3Int childMin = min + offset;
                children[i] = new Octant<T>(origin, this, subdivisions - 1, childMin, key + i.ToString());
                */
                children[i] = Octant<T>.FromParent(this, i);
            }

            // Calculate data inside child octant
            IEnumerator<Octant<T>> childScanEnumerator = children[i].ScanIterator();
            while (childScanEnumerator.MoveNext()) yield return childScanEnumerator.Current;
            childScanEnumerator.Dispose();

            // Check if this octant can be counted as part of a 'full' parent octant
            bool childIsFull = false;
            childIsFull = childIsFull || children[i].type == OctantType.Leaf;
            childIsFull = childIsFull || children[i].type == OctantType.FullyValid;
            childIsFull = childIsFull || children[i].DoesCriteriaMatterWithThisOctant() == false;

            // If a single child is not full, then that means this octant is not full.
            isFull &= childIsFull;
        }

        if (isFull)
        {
            // Mark as full, because all children are full
            type = OctantType.FullyValid;
        }
        else
        {
            // Mark as branch, because there's a mixture of full and non-full
            type = OctantType.Branch;
        }
    }

    public void ProcessAllCells()
    {
        IEnumerator<Octant<T>> enumerator = CellIterator(true);
        while (enumerator.MoveNext()) continue;
        enumerator.Dispose();
        
    }
    public void DrawGizmos(Color invalid, Color branch, Color leaf, Color full)
    {
        Vector3 centre = bounds.center;
        Vector3 size = bounds.size;

        // Draw different gizmos based on each cell type
        switch (type)
        {
            
            case OctantType.Invalid:

                if (depth > 0) break;

                Gizmos.color = invalid;
                Gizmos.DrawWireCube(centre, size);

                break;
            
            case OctantType.Leaf:
                // Colour the cell differently and draw it full rather than wireframe, to show it's the end of the subdivision
                Gizmos.color = leaf;
                Gizmos.DrawCube(centre, size);
                break;

            case OctantType.FullyValid:
                // Colour the cell differently and draw it full rather than wireframe, to show it's the end of the subdivision
                Gizmos.color = full;
                Gizmos.DrawCube(centre, size);
                break;

            case OctantType.Branch:

                Gizmos.color = depth > 0 ? branch : invalid;
                Gizmos.DrawWireCube(centre, size);

                // Draw gizmos for each child
                for (int i = 0; i < 8; i++)
                {
                    children[i].DrawGizmos(invalid, branch, leaf, full);
                }
                break;
        }
    }

    public IEnumerator<Octant<T>> EmptyIterator(bool processCells, bool emptySelf, bool emptyChildren, bool deleteChildren)
    {
        //if (processCells)  Debug.Log($"Should empty: {type}, {emptySelf}, {emptyChildren}, {deleteChildren}");
        if (emptyChildren || deleteChildren)
        {
            for (int i = 0; i < 8; i++)
            {
                if (children[i] == null) continue;

                // For each child present, yield on its EmptyIterator() as well before proceeding.
                // If this octant needs its children emptied, obviously all their children and so on need to be emptied as well.
                IEnumerator<Octant<T>> childEnumerator = children[i].EmptyIterator(processCells, true, true, deleteChildren);
                while (childEnumerator.MoveNext()) yield return childEnumerator.Current;
                childEnumerator.Dispose();

                // Clear child octant from its array position
                if (processCells && deleteChildren) children[i] = null;
            }
        }

        // Yield if this octant needs to be emptied
        if (emptySelf)
        {
            yield return this;
            if (processCells && clearOctant != null) _leafData = clearOctant.Invoke(this);
        }
    }


    public IEnumerator<Octant<T>> CellIterator(bool processCells)
    {
        /*
        IEnumerator<Octant<T>> emptyIterator = type switch
        {
            //OctantType.Invalid => 
            OctantType.Leaf => EmptyIterator(processCells, false, true, true), // Retain self but have no children
            OctantType.FullyValid => EmptyIterator(processCells, false, true, false), // Retain self, empty children but don't delete them
            OctantType.Branch => EmptyIterator(processCells, true, false, false), // Empty self but keep children as-is
            _ => EmptyIterator(processCells, true, true, true), // If invalid (or anything else), completely empty
        };
        while (emptyIterator.MoveNext()) yield return emptyIterator.Current;
        emptyIterator.Dispose();
        */
        
        bool selfMustBeEmpty = type == OctantType.Invalid || type == OctantType.Branch;
        bool childrenMustBeEmpty = type != OctantType.Branch;
        bool deleteChildren = type == OctantType.Invalid || type == OctantType.Leaf;
        IEnumerator<Octant<T>> emptyIterator = EmptyIterator(processCells, selfMustBeEmpty, childrenMustBeEmpty, deleteChildren);
        while (emptyIterator.MoveNext()) yield return emptyIterator.Current;
        emptyIterator.Dispose();
        
        // Yield if this octant needs to be populated (leaf or fully valid)
        if (!selfMustBeEmpty)
        {
            yield return this;
            if (processCells && refreshOctant != null) _leafData = refreshOctant.Invoke(this);
        }

        if (type != OctantType.Branch) yield break;

        // If non-empty children are present, continue iterating through them.
        for (int i = 0; i < 8; i++)
        {
            IEnumerator<Octant<T>> childEnumerator = children[i].CellIterator(processCells);
            while (childEnumerator.MoveNext()) yield return childEnumerator.Current;
            childEnumerator.Dispose();
        }
    }
    





    /// <summary>
    /// Scans and refreshes octants at the same time before moving onto the next. Almost like turning it into multiple octrees for different sections.
    /// <para>Ensures less delay between scanning and processing even with asynchronous operation, but means a minimum of (8^<paramref name="startDepth"/>) cells to update.</para>
    /// </summary>
    /// <param name="startDepth"></param>
    /// <returns></returns>
    public IEnumerator<Octant<T>> FullRefreshIterator(int startDepth)
    {
        // Once we're deep enough to scan properly, start iterating through this octant like it's a base
        if (depth >= startDepth)
        {
            // Scan
            IEnumerator<Octant<T>> scanEnumerator = ScanIterator();
            while (scanEnumerator.MoveNext()) yield return scanEnumerator.Current;
            scanEnumerator.Dispose();
            // Process
            IEnumerator<Octant<T>> processEnumerator = CellIterator(true);
            while (processEnumerator.MoveNext()) yield return processEnumerator.Current;
            processEnumerator.Dispose();

            yield break;
        }





        // TO DO: mark this octant as a branch
        // TO DO: delete cell contents?
        // TO DO: make sure irrelevant octants aren't drawn as gizmos?


        type = OctantType.Branch;
        IEnumerator<Octant<T>> emptyIterator = EmptyIterator(true, true, false, false);
        while (emptyIterator.MoveNext()) yield return emptyIterator.Current;
        emptyIterator.Dispose();




        // Check in child octants
        //int childSize = size / 2;
        for (int i = 0; i < 8; i++)
        {
            // TO DO: Set up child octant
            if (children[i] == null) children[i] = Octant<T>.FromParent(this, i);

            // Keep iterating down until we're deep enough to actually start scanning and processing.
            IEnumerator<Octant<T>> childEnumerator = children[i].FullRefreshIterator(startDepth);
            while (childEnumerator.MoveNext()) yield return childEnumerator.Current;
            childEnumerator.Dispose();
        }
    }
}

public class Octree<T>
{
    public delegate T LeafDataAssignment(Octant<T> octant);
    
    public int subdivisions;
    public System.Func<BoundsInt, bool> checkOctant;
    public System.Func<Octant<T>, bool> doesCheckMatter;
    public LeafDataAssignment refreshOctant;
    public LeafDataAssignment clearOctant;

    Octant<T> _base;

    public Octant<T> baseContainer
    {
        get
        {
            if (_base == null || subdivisions != _base.subdivisions)
            {
                if (_base != null)
                {
                    // If _base exists but has wrong subdivisions, fully clear it
                    IEnumerator<Octant<T>> clearEntirely = _base.EmptyIterator(true, true, true, true);
                    while (clearEntirely.MoveNext()) continue;
                    clearEntirely.Dispose();
                }

                // Create new base octant with correct values
                _base = new Octant<T>(this, null, subdivisions, Vector3Int.zero, "");
            }

            return _base;
        }
    }

    
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
        if (refreshOctant != null) baseContainer.ProcessAllCells();
    }
    public void Scan()
    {
        IEnumerator<Octant<T>> enumerator = ScanIterator();
        while (enumerator.MoveNext()) continue;
        enumerator.Dispose();
    }
    public IEnumerator<Octant<T>> ScanIterator() => baseContainer.ScanIterator();

    public void DrawGizmos() => DrawGizmos(Color.white, Color.grey, new Color(0.25f, 0.25f, 0.25f), new Color(1f, 0.1f, 0));
    public void DrawGizmos(Color whole, Color branch, Color leaf, Color full)
    {
        baseContainer.DrawGizmos(whole, branch, leaf, full);
    }

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