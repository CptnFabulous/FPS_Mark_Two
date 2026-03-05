using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree<T>
{
    class Octant
    {
        Octant parentOctant;
        public Octant[] childOctants = new Octant[8];



        public T leafValue { get; private set; }

        public bool containsLeaf { get; private set; }
        public bool isLeaf { get; private set; }



        public void SetValue(T value, int x, int y, int z)
        {
            this.leafValue = value;

            // If value is null, 
        }
    }


    T[,,] grid;
    Bounds bounds;
    Vector3Int gridSize;
    Octant allOctants;

    public Octree(Bounds newBounds, float minimumNodeSize)
    {
        // Make a 3D grid
        bounds = newBounds;

        //float halfSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;
        //Vector3 extents = Vector3.one * halfSize;
        Vector3 extents = Vector3.one * Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        bounds.SetMinMax(bounds.center - extents, bounds.center + extents);


        Vector3 divided = bounds.size;
        for (int i = 0; i < 2; i++)
        {
            divided[i] /= minimumNodeSize;
        }
        gridSize = Vector3Int.CeilToInt(divided);
        grid = new T[gridSize.x, gridSize.y, gridSize.z];
    }


    public void PopulateGridSpace(T value, int x, int y, int z)
    {

    }



    public void IterateThrough(System.Action<T> criteria) => IterateThrough(allOctants, criteria);
    void IterateThrough(Octant section, System.Action<T> criteria)
    {
        if (section.containsLeaf == false) return;

        if (section.isLeaf)
        {
            criteria.Invoke(section.leafValue);
            return;
        }

        // TO DO: check if this octree contains a child value

        for (int i = 0; i < section.childOctants.Length; i++)
        {

        }
    }
}