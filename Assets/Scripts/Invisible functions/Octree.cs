using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Octant<T>
{
    public Vector3Int min = Vector3Int.zero;
    public Octant<T>[] children = new Octant<T>[8];
    public bool isLeaf;
    public T leafData;
}
public class Octree<T>
{
    public int subdivisions;
    public System.Func<Vector3Int, Vector3Int, bool> checkOctant;
    public System.Func<Vector3Int, Vector3Int, T> recordLeafData;

    Octant<T> _base = new Octant<T>();

    public Octant<T> baseContainer => _base;
    
    public void Refresh()
    {
        //SearchOctant(_base, subdivisions);
        RefreshOctant(_base, subdivisions);
    }
    public void DrawGizmos() => DrawOctantGizmos(_base, subdivisions);

    /*
    void SearchOctant(Octant<T> octant, int sizePower)
    {
        if (octant == null) return;

        // The size of the entire octant
        int sizeOfWhole = CalculateGridSize(sizePower);

        // TO DO: if size at depth is 1 or less, that means we've subdivided as far as we can go.
        if (sizeOfWhole <= 1)
        {
            // Record whatever base level data needs to be recorded, and end the function.
            Vector3Int dimensionsOfWhole = new Vector3Int(sizeOfWhole, sizeOfWhole, sizeOfWhole);
            if (recordLeafData != null) octant.leafData = recordLeafData.Invoke(octant.min, octant.min + dimensionsOfWhole);
            return;
        }

        int childSize = sizeOfWhole / 2;
        Vector3Int childDimensions = new Vector3Int(childSize, childSize, childSize);
        
        for (int i = 0; i < 8; i++)
        {
            // Calculate check min and max
            Vector3Int offset = SmokeParticleDensityController.neighbourOffsets[i];
            offset.x *= childSize;
            offset.y *= childSize;
            offset.z *= childSize;
            Vector3Int childMin = octant.min + offset;
            Vector3Int childMax = childMin + childDimensions;

            // Check if this octant meets the criteria
            bool somethingDetected = checkOctant.Invoke(childMin, childMax);

            // Ensure octants are added if something is detected, and existing ones are destroyed if nothing is there
            if (somethingDetected != (octant.children[i] != null))
            {
                octant.children[i] = somethingDetected ? new Octant<T>() : null;
            }

            // No need to continue if there's nothing in this space
            if (!somethingDetected) continue;

            // Refresh child octant, and check recursively.
            octant.children[i].min = childMin;
            SearchOctant(octant.children[i], sizePower - 1);
        }
    }
    */
    void RefreshOctant(Octant<T> octant, int sizePower)
    {
        if (octant == null) return;

        octant.isLeaf = false;

        // The size of the entire octant
        int size = CalculateGridSize(sizePower);
        Vector3Int dimensions = new Vector3Int(size, size, size);
        Vector3Int max = octant.min + dimensions;
        bool possibleChildrenDetected = checkOctant.Invoke(octant.min, max);


        // If no more children can be found, record leaf data and return
        if (!possibleChildrenDetected)
        {
            if (recordLeafData != null) octant.leafData = recordLeafData.Invoke(octant.min, max);
            octant.isLeaf = true;
            // Clear all child slots
            for (int i = 0; i < 8; i++) octant.children[i] = null;
            return;
        }

        // TO DO: if size at depth is 1 or less, that means we've subdivided as far as we can go.
        if (size <= 1)
        {
            // Do nothing?
            return;
        }

        int childSize = size / 2;
        Vector3Int childDimensions = new Vector3Int(childSize, childSize, childSize);

        for (int i = 0; i < 8; i++)
        {
            // Calculate check min and max
            Vector3Int offset = SmokeParticleDensityController.neighbourOffsets[i];
            offset.x *= childSize;
            offset.y *= childSize;
            offset.z *= childSize;
            Vector3Int childMin = octant.min + offset;
            Vector3Int childMax = childMin + childDimensions;

            if (octant.children[i] == null) octant.children[i] = new Octant<T>();

            octant.children[i].min = childMin;
            RefreshOctant(octant.children[i], sizePower - 1);
        }
    }
    void DrawOctantGizmos<T>(Octant<T> octant, int sizePower)
    {
        if (octant == null) return;

        // Size
        int sizeAlongAxis = CalculateGridSize(sizePower);
        Vector3 size = new Vector3(sizeAlongAxis, sizeAlongAxis, sizeAlongAxis);
        Vector3 centre = octant.min + (size / 2);

        if (octant.isLeaf)
        {
            //Gizmos.color = new Color(0, 0.5f, 0.5f);
            float colourLerp = 1 - ((float)sizePower / (float)subdivisions);
            Gizmos.color = Color.Lerp(Color.white, Color.black, colourLerp);
            //Gizmos.DrawCube(centre, size);
            Gizmos.DrawWireCube(centre, size);
            //Gizmos.DrawWireSphere(centre, sizeAlongAxis / 2);
            //return;
        }
        else
        {
            // Colour
            float colourLerp = 1 - ((float)sizePower / (float)subdivisions);
            Gizmos.color = Color.Lerp(Color.white, Color.black, colourLerp);
            // Draw
            Gizmos.DrawWireCube(centre, size);
        }

        

        // Draw gizmos for each child
        for (int i = 0; i < 8; i++)
        {
            DrawOctantGizmos(octant.children[i], sizePower - 1);
        }
    }

    int CalculateGridSize(int sizePower) => Mathf.RoundToInt(Mathf.Pow(2, sizePower));
}