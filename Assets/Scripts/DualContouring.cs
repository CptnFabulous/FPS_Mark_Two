using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VoxelMesh : MonoBehaviour
{
    static float[] valuesAtCornersOfGridSpace = new float[8];
    static Vector3[] pointsOfDensityChangeAcrossGridSpace = new Vector3[12];
    static Vector3[] directionsOfDensityChangeAcrossGridSpace = new Vector3[12];
    static int[] edgeWhereDensityChangeOccurred = new int[12];







    

    public static void DualContouring(Vector3Int dimensions, System.Func<Vector3Int, float> obtainValueAtPosition, System.Func<Vector3, float> obtainSignedDistanceField)
    {
        // Add faces for each grid position
        MiscFunctions.IterateThroughGrid(dimensions, (coords) =>
        {
            // Get coordinates of grid space, as an easily modifiable vector

            bool success = FindVertexForDualContouringSpace(coords, obtainValueAtPosition, obtainSignedDistanceField, out Vector3 vertexPos, out Vector3 normal);
            if (success == false) return;

            // Get correct index order for new vertex data, and update total vertex count
            int arrayOrder = currentNumberOfVertices;
            vertexDictionary.Add(coords, arrayOrder);
            //createdVertices++;

            vertices[arrayOrder] = vertexPos;

            vertexNormals[arrayOrder] = normal;
        });

        //CalculateDualContouredMeshFromVertexData(dimensions);
    }
    static bool FindVertexForDualContouringSpace(Vector3Int position, System.Func<Vector3Int, float> obtainValueAtPosition, System.Func<Vector3, float> signedDistanceField, out Vector3 vertex, out Vector3 normalAtVertex)
    {
        //Debug.Log($"Checking for vertices at {position} on frame {Time.frameCount}");
        vertex = position;
        normalAtVertex = Vector3.zero;

        float valueAtPoint = obtainValueAtPosition.Invoke(position);

        //Vector3Int coords = new Vector3Int(x, y, z);
        Vector3 vertexSpaceMin = position - globalVertexOffset;
        for (int c = 0; c < 8; c++)
        {
            Vector3 possibleCorner = vertexSpaceMin + corners[c];
            valuesAtCornersOfGridSpace[c] = signedDistanceField.Invoke(possibleCorner);
        }

        int numOfChanges = 0;
        for (int e = 0; e < 12; e++)
        {
            // 12 iterations with 2 sets of coordinates, for the corners we calculated.
            // Each set of 2 corners represents an edge around the grid space.
            int corner1 = edges[e].Item1;
            int corner2 = edges[e].Item2;
            Vector3 cornerPos1 = vertexSpaceMin + corners[corner1];
            Vector3 cornerPos2 = vertexSpaceMin + corners[corner2];
            Debug.DrawLine(cornerPos1, cornerPos2, Color.gray);

            // Compare the values we calculated at those corners.
            float v1 = valuesAtCornersOfGridSpace[corner1];
            float v2 = valuesAtCornersOfGridSpace[corner2];
            float isosurface = 0;
            //float isosurface = valueAtPoint;

            // If one is zero but the other isn't, that means we've reached the edge of the filled spaces, and a face needs to run through here.
            // If not, continue.
            bool fillMismatch = (v1 > 0) != (v2 > 0);
            if (!fillMismatch) continue;

            Vector3 pointOfDensityChange = corners[corner1]; // We could use corner1 or corner2, as the only different value is being overwritten anyway.


            // Should I instead calculate this based on each vertex instead of each grid space, and get the set-in-stone grid space values as the corners?
            // If I want the mesh to shift in size, I need to know what the max possible value is.

            // Just as in the edge array.
            //float difference = v1 - v2;
            //float difference = v2 - v1;

            // Should shift from one side to the other based on the different densities.


            // One side will always be zero. Shift between 0 and 1 based on the other side's magnitude.
            // If the second value is non-zero, shift between 1 and 0 instead, as it means the shape is expanding in the opposite direction.
            // So maximum size of v2 should mean zero.



            // If number on one side is bigger, shift position away from that, as that means the mesh is expanding in that direction.

            // If v1 is 0 and v2 is 1, difference should be zero since v2 is fully expanded and v1 is empty
            // If both values are equal, value should be in the middle


            float difference = (isosurface - v1) / (v2 - v1);
            //float difference = ((float)isosurface - (float)v1) / ((float)v2 - (float)v1);

            //float difference = v1 / (v1 + v2);
            //float difference = 0.5f;
            //float difference = (0 + v1) / (v2 - v1);
            //float difference = v1 / v2;

            //float difference = (v1 > v2) ? v1 : 1 - v2;



            //difference = Mathf.Clamp01(difference);

            // Get the correct axis the edge runs across (X from positions 1-4, Y for positions 5-8, Z for positions 9-12)
            int axisBeingChecked = Mathf.FloorToInt(e / 4f);
            pointOfDensityChange[axisBeingChecked] = difference;

            Vector3 pointToCheck = vertexSpaceMin + pointOfDensityChange;
            Vector3 normal = NormalFromDerivatives(pointToCheck, (pos) => signedDistanceField.Invoke(pos));

            pointsOfDensityChangeAcrossGridSpace[numOfChanges] = pointToCheck;
            directionsOfDensityChangeAcrossGridSpace[numOfChanges] = normal;
            edgeWhereDensityChangeOccurred[numOfChanges] = e;
            numOfChanges++;


            //int filledCorner = startIsFilled ? corner1 : corner2;
            //Debug.DrawLine(vertexSpaceMin + corners[filledCorner], vertexSpaceMin + pointOfDensityChange, Color.blue);
            Debug.DrawLine(cornerPos1, pointToCheck, Color.blue);
            Debug.DrawLine(cornerPos2, pointToCheck, Color.blue);
            //Debug.Log($"{v1}, {v2}, {difference}. {pointToCheck}, {normal}.");



            Debug.DrawRay(pointToCheck, 0.5f * normal, Color.green);
            Debug.DrawLine(pointToCheck + 0.5f * normal, position, Color.yellow);
        }

        //if (numOfChanges > 0) Debug.Log($"Checked vertex {position}, {numOfChanges} edge differences, frame {Time.frameCount}");

        // I'm commenting this out in case it's potentially ditching usable results

        // If there aren't enough changes to show a face needs to be there, cancel.
        // Not sure why there needs to be specifically 2 or more changes.
        if (numOfChanges <= 1) return false;

        // TO DO: figure out how the hell to turn those points and normals into a vertex position within the grid space.




        return false;

    }
    /// <summary>
    /// Another test thing, doesn't work properly but I'm keeping it in case it becomes useful later.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="obtainValueAtPosition"></param>
    /// <param name="vertex"></param>
    /// <returns></returns>
    static bool DualContouringThingExceptUsingSeparateVerticesFromGridPositions(Vector3 position, System.Func<Vector3Int, float> obtainValueAtPosition, out Vector3 vertex)
    {
        Debug.Log($"Checking vertex {position} on frame {Time.frameCount}");
        // TO DO: since some edges are shared between vertices, I think I can reduce the workload by caching the edge data.
        // I think I'd need 3 3D arrays, one for each axis. Each array would need to be 1 unit wider on each side compared to the actual grid.

        //Vector3Int coords = new Vector3Int(x, y, z);
        Vector3 vertexSpaceMin = position - globalVertexOffset;
        for (int c = 0; c < 8; c++)
        {
            Vector3Int gridPos = Vector3Int.RoundToInt(vertexSpaceMin + corners[c]);
            valuesAtCornersOfGridSpace[c] = obtainValueAtPosition.Invoke(gridPos);
        }





        int numOfChanges = 0;
        for (int e = 0; e < 12; e++)
        {
            // 12 iterations with 2 sets of coordinates, for the corners we calculated.
            // Each set of 2 corners represents an edge around the grid space.
            int corner1 = edges[e].Item1;
            int corner2 = edges[e].Item2;
            // Compare the values we calculated at those corners.
            float v1 = valuesAtCornersOfGridSpace[corner1];
            float v2 = valuesAtCornersOfGridSpace[corner2];

            // If one is zero but the other isn't, that means we've reached the edge of the filled spaces, and a face needs to run through here.
            bool fillMismatch = (v1 > 0) != (v2 > 0);

            //Debug.Log($"{position}, {axisBeingChecked}, frame {Time.frameCount}");

            Debug.DrawLine(vertexSpaceMin + corners[corner1], vertexSpaceMin + corners[corner2], Color.gray);
            if (!fillMismatch) continue;

            Vector3 pointOfDensityChange = corners[corner1]; // We could use corner1 or corner2, as the only different value is being overwritten anyway.

            bool startIsFilled = v1 > v2;

            float difference = startIsFilled ? v1 : 1 - v2;

            Debug.Log($"{v1}, {v2}, {difference}");


            // Get the correct axis the edge runs across (X from positions 1-4, Y for positions 5-8, Z for positions 9-12)
            int axisBeingChecked = Mathf.FloorToInt(e / 4f);
            pointOfDensityChange[axisBeingChecked] = difference;

            //Debug.Log($"{position}, {e}, {difference} frame {Time.frameCount}");

            Vector3 pointToCheck = vertexSpaceMin + pointOfDensityChange;
            Vector3 normal = NormalFromDerivatives(pointToCheck, (pos) => MidpointBetweenGridSpaces(pos, obtainValueAtPosition));
            pointsOfDensityChangeAcrossGridSpace[numOfChanges] = pointToCheck;
            directionsOfDensityChangeAcrossGridSpace[numOfChanges] = normal;
            edgeWhereDensityChangeOccurred[numOfChanges] = e;
            numOfChanges++;

            int filledCorner = startIsFilled ? corner1 : corner2;
            Debug.DrawLine(vertexSpaceMin + corners[filledCorner], vertexSpaceMin + pointOfDensityChange, Color.blue);
            Debug.DrawRay(pointToCheck, normal, Color.green);
        }

        //Debug.Log($"{numOfChanges}, frame {Time.frameCount}");

        // If there aren't enough changes to show a face needs to be there, cancel.
        // Not sure why there needs to be specifically 2 or more changes.
        if (numOfChanges <= 1)
        {
            vertex = position;
            return false;
        }

        // TO DO: figure out how the hell to turn those points and normals into a vertex position within the grid space.




        // Create planes based off edge positions


        /*
        for (int i = 0; i < 3; i++)
        {
            int edgeAxisIndex = i * 4;

            Vector3 a = pointsOfDensityChangeAcrossGridSpace[edgeAxisIndex];
            Vector3 b = pointsOfDensityChangeAcrossGridSpace[edgeAxisIndex + 1];
            Vector3 c = pointsOfDensityChangeAcrossGridSpace[edgeAxisIndex + 2];
            Vector3 d = pointsOfDensityChangeAcrossGridSpace[edgeAxisIndex + 3];

            Plane planeA = new Plane(a, b, c);
            Plane planeB = new Plane(c, b, d);
        }
        */


        vertex = position;
        return false;

    }




    /*

    /// <summary>
    /// EXPERIMENTAL THING: checks both sides on each axis simultaneously, does some weird lerpy stuff. Go back to this later once I've finished retrying having the dual contouring check but using the corner vertices
    /// </summary>
    public static void BorkedPseudoDualContouringThing(Vector3Int dimensions, System.Func<Vector3Int, float> obtainValueAtPosition)
    {
        int scale = 2;

        //Vector3Int meshDimensions = dimensions;
        Vector3Int meshDimensions = dimensions * scale;


        float GetSample(Vector3Int coords)
        {
            //return MidpointBetweenGridSpaces((Vector3)coords / scale, obtainValueAtPosition);

            Vector3Int gridCoordsToSample = Vector3Int.FloorToInt((Vector3)coords / scale);
            return obtainValueAtPosition.Invoke(gridCoordsToSample);
        }
        float GetNeighbouringSample(Vector3Int coords, int neighbourIndex)
        {
            return GetSample(coords + adjacencies[neighbourIndex]);
            
            //Vector3Int gridCoordsToSample = coords;
            Vector3Int gridCoordsToSample = Vector3Int.FloorToInt((Vector3)coords / scale);
            gridCoordsToSample += adjacencies[neighbourIndex];
            return obtainValueAtPosition.Invoke(gridCoordsToSample);
        }

        // Add faces for each grid position
        MiscFunctions.IterateThroughGrid(meshDimensions, (coords) =>
        {
            DrawDebugWireCube(((Vector3)coords / scale) - (globalVertexOffset / scale), Vector3.one / scale, Color.gray);

            float spaceValue = GetSample(coords);
            
            // If space has a value of zero, continue (empty space).
            // TO DO! Modify this check so it's only rejected if not only the space itself is empty, but there are no adjacent spaces either.
            if (spaceValue <= 0) return;
            

            // I think I do need to continue with this, but instead I check adjacent grid spaces instead of corners.
            // For each axis, get the value in that direction and the one behind it.

            Vector3 vertexPositionWithinGridSpace = globalVertexOffset;
            bool vertexNeedsToBeCreated = false;
            for (int i = 0; i < 3; i++)
            {
                int minusIndex = i * 2;
                int plusIndex = minusIndex + 1;

                float valueAtMinus = GetNeighbouringSample(coords, minusIndex);
                float valueAtPlus = GetNeighbouringSample(coords, plusIndex);

                bool minusExists = valueAtMinus > 0;
                bool plusExists = valueAtPlus > 0;

                bool adjacentsOnAxis = minusExists || plusExists;
                //bool adjacentsOnAxis = minusExists || plusExists || spaceValue > 0;
                //bool adjacentsOnAxis = (minusExists || plusExists) && spaceValue <= 0;
                //bool adjacentsOnAxis = true;

                // If both minus and plus are greater than zero, it's a centre value and there won't be any edges on this axis.
                if (!adjacentsOnAxis) continue;


                // Shift 'max' point between 0 and 1 based on relative strength of both sides.
                // If minus is filled and plus isn't, shift from 0 to 1.
                // If the opposite way around, shift from 1 to 0.
                // If both are equal, have the start point be in the middle.


                // This method is more promising, once I figure out how to use it.
                // Although if a value is centered and a new value appears adjacent to it, the new point appears 0.5 units away instead of starting off at the same position as the existing value.
                // I could potentially check if the filled adjacent spaces are also 'centred', and adjust the value even further, but that might require some kind of recursive nonsense.

                float minPoint = 0.5f;
                minPoint -= (0.5f * valueAtMinus);
                minPoint += (0.5f * valueAtPlus);
                //minPoint -= (1f * valueAtMinus);
                //minPoint += (1f * valueAtPlus);
                float maxPoint = 0.5f;
                maxPoint += (0.5f * valueAtMinus);
                maxPoint -= (0.5f * valueAtPlus);


                vertexPositionWithinGridSpace[i] = Mathf.Lerp(minPoint, maxPoint, spaceValue);

                vertexNeedsToBeCreated = true;

            }

            if (!vertexNeedsToBeCreated) return;

            Vector3 vertexPos = coords - globalVertexOffset + vertexPositionWithinGridSpace;

            // Get correct index order for new vertex data, and update total vertex count
            int arrayOrder = currentNumberOfVertices;
            vertexDictionary.Add(coords, arrayOrder);
            //createdVertices++;

            vertices[arrayOrder] = vertexPos;

            Vector3 normal = -NormalFromDerivatives(vertexPos, (pos) => MidpointBetweenGridSpaces(pos, GetSample));
            vertexNormals[arrayOrder] = normal;



            // IDEA: Have this code be applied to a 3D grid of vertices, so that if an instantaneous change applies to the smoke then the vertices can be shifted over time towards the desired value.
            // That means I don't have to worry about sharp changes!
            // I adjust the grid of vertices over time, and each frame I determine which ones actually need to be part of the mesh, then make the mesh accordingly.
            // I'll need to try this tomorrow.
        });

        //CalculateDualContouredMeshFromVertexData(dimensions, obtainValueAtPosition);
        CalculateDualContouredMeshFromVertexData(meshDimensions);

        // Divide vertex positions to ensure they fit the grid scale.
        // As long as all the vertices have the same relative positions, and the indices stay the same, the mesh shape will be the same.
        for (int v = 0; v < currentNumberOfVertices; v++)
        {
            vertices[v] /= scale;
            vertices[v] -= globalVertexOffset / scale;
        }
    }

    enum FillStatusOnAxis
    {
        NoNeighbours = 0,
        MinusIsInward = 1,
        PlusIsInward = 2,
        BoxedIn = 3,
    }

    public static Vector3Int GetNeighbourStatuses()
    {
        Vector3Int neighbourStatuses = Vector3Int.zero;

        for (int a = 0; a < 3; a++)
        {
            int minusIndex = a * 2;
            int plusIndex = minusIndex + 1;

            float valueAtMinus = valuesOfNeighbours[minusIndex];
            float valueAtPlus = valuesOfNeighbours[plusIndex];

            bool minusExists = valueAtMinus > 0;
            bool plusExists = valueAtPlus > 0;

            neighbourStatuses[a] = (minusExists, plusExists) switch
            {
                (true, false) => (int)FillStatusOnAxis.MinusIsInward,
                (false, true) => (int)FillStatusOnAxis.PlusIsInward,
                (true, true) => (int)FillStatusOnAxis.BoxedIn,
                _ => (int)FillStatusOnAxis.NoNeighbours,
            };

            bool adjacentsOnAxis = minusExists || plusExists;
            //bool adjacentsOnAxis = minusExists || plusExists || spaceValue > 0;
            //bool adjacentsOnAxis = (minusExists || plusExists) && spaceValue <= 0;
            //bool adjacentsOnAxis = true;

            // If both minus and plus are greater than zero, it's a centre value and there won't be any edges on this axis.
            if (!adjacentsOnAxis) continue;
        }

        return neighbourStatuses;
    }

    /// <summary>
    /// EXPERIMENTAL THING: checks both sides on each axis simultaneously, does some weird lerpy stuff. Go back to this later once I've finished retrying having the dual contouring check but using the corner vertices
    /// </summary>
    public static void BorkedPseudoDualContouringThing2(Vector3Int dimensions, System.Func<Vector3Int, float> obtainValueAtPosition)
    {
        // Add faces for each grid position
        MiscFunctions.IterateThroughGrid(dimensions, (coords) =>
        {
            // Check if there's actually a value here first.
            float spaceValue = obtainValueAtPosition.Invoke(coords);
            if (spaceValue <= 0) return;

            // Get data on neighbouring faces first, so when creating faces we can check multiple neighbours at once
            int numberOfNeighbours = 0;
            for (int d = 0; d < 6; d++)
            {
                Vector3Int neighbour = coords + adjacencies[d];
                float value = obtainValueAtPosition.Invoke(neighbour);
                valuesOfNeighbours[d] = value; // These values are overwritten for each grid space, but the array itself is reused to avoid garbage collection.
                if (value > 0) numberOfNeighbours++;
            }

            Vector3 min = globalVertexOffset;
            Vector3 max = globalVertexOffset;

            Vector3Int neighbourStatuses = Vector3Int.zero;

            for (int a = 0; a < 3; a++)
            {
                int minusIndex = a * 2;
                int plusIndex = minusIndex + 1;

                float valueAtMinus = valuesOfNeighbours[minusIndex];
                float valueAtPlus = valuesOfNeighbours[plusIndex];

                bool minusExists = valueAtMinus > 0;
                bool plusExists = valueAtPlus > 0;

                neighbourStatuses[a] = (minusExists, plusExists) switch
                {
                    (true, false) => (int)FillStatusOnAxis.MinusIsInward,
                    (false, true) => (int)FillStatusOnAxis.PlusIsInward,
                    (true, true) => (int)FillStatusOnAxis.BoxedIn,
                    _ => (int)FillStatusOnAxis.NoNeighbours,
                };

                bool adjacentsOnAxis = minusExists || plusExists;
                //bool adjacentsOnAxis = minusExists || plusExists || spaceValue > 0;
                //bool adjacentsOnAxis = (minusExists || plusExists) && spaceValue <= 0;
                //bool adjacentsOnAxis = true;

                // If both minus and plus are greater than zero, it's a centre value and there won't be any edges on this axis.
                if (!adjacentsOnAxis) continue;


                // Shift 'max' point between 0 and 1 based on relative strength of both sides.
                // If minus is filled and plus isn't, shift from 0 to 1.
                // If the opposite way around, shift from 1 to 0.
                // If both are equal, have the start point be in the middle.


                // This method is more promising, once I figure out how to use it.
                // Although if a value is centered and a new value appears adjacent to it, the new point appears 0.5 units away instead of starting off at the same position as the existing value.
                // I could potentially check if the filled adjacent spaces are also 'centred', and adjust the value even further, but that might require some kind of recursive nonsense.

                min[a] -= (0.5f * valueAtMinus);
                min[a] += (0.5f * valueAtPlus);
                //minPoint -= (1f * valueAtMinus);
                //minPoint += (1f * valueAtPlus);
                max[a] += (0.5f * valueAtMinus);
                max[a] -= (0.5f * valueAtPlus);
            }

            // For each grid space, create up to 8 vertices.


            for (int i = 0; i < 8; i++)
            {
                bool vertexNeedsToBeCreated = false;

                Vector3Int corner = corners[i];

                Vector3 vertexPosInCell = new Vector3();
                for (int a = 0; a < 3; a++)
                {
                    FillStatusOnAxis fillStatus = (FillStatusOnAxis)neighbourStatuses[a];
                    
                    if (fillStatus == FillStatusOnAxis.NoNeighbours)
                    {
                        // If both sides are empty, lerp from centre to whatever the max corner value is.
                        // The min value would be in the centre anyway. Using this also ensures it shifts as the cells change density.
                        vertexPosInCell[a] = Mathf.Lerp(min[a], corner[a], spaceValue);

                        // Mark this point as definitely being on the outside of the shape and needing to be made into a vertex
                        vertexNeedsToBeCreated = true;
                    }
                    else if (fillStatus == FillStatusOnAxis.BoxedIn)
                    {
                        // Boxed in on both sides.
                    }

                    switch (corner[a], fillStatus)
                    {
                        // If the corner's position on this axis is closer to the empty side than the filled side, lerp all the way from min to max.
                        case (0, FillStatusOnAxis.PlusIsInward):
                        case (1, FillStatusOnAxis.MinusIsInward):

                            vertexPosInCell[a] = Mathf.Lerp(min[a], max[a], spaceValue);
                            // Mark this point as definitely being on the outside of the shape and needing to be made into a vertex
                            vertexNeedsToBeCreated = true;
                            break;

                        // If closer to the filled side, only lerp halfway from min to max on that axis.
                        case (0, FillStatusOnAxis.MinusIsInward):
                        case (1, FillStatusOnAxis.PlusIsInward):
                            vertexPosInCell[a] = Mathf.Lerp(min[a], max[a], spaceValue * 0.5f);

                            // Don't mark as needing to be created yet.
                            // If this corner is closer to the filled side on every axis, then that means it won't be on the outside and we don't want any faces to go towards it.
                            break;
                    }
                }

                // If this vertex is not closer to an empty side on any axis, do not create it.
                // So vertexNeedsToBeCreated is only enabled if this is the case.
                if (!vertexNeedsToBeCreated) continue;

                Vector3 vertexPos = coords - globalVertexOffset + vertexPosInCell;
                Vector3Int vertexGridPos = (2 * coords) + corner;


                // Get correct index order for new vertex data, and update total vertex count
                int arrayOrder = currentNumberOfVertices;
                vertexDictionary.Add(vertexGridPos, arrayOrder);
                //createdVertices++;

                vertices[arrayOrder] = vertexPos;

                Vector3 normal = -NormalFromDerivatives(vertexPos, (pos) => MidpointBetweenGridSpaces(pos, obtainValueAtPosition));
                vertexNormals[arrayOrder] = normal;

                gridSpaceNeighbourStatuses[arrayOrder] = neighbourStatuses;
            }
        });

        // Actually generate values
        CalculateDualContouredMeshFromVertexData(dimensions * 2, obtainValueAtPosition);
    }
    */


    static void CalculateDualContouredMeshFromVertexData(Vector3Int dimensions/*, System.Func<Vector3Int, float> obtainValueAtPosition*/)
    {
        MiscFunctions.IterateThroughGrid(dimensions, (coords) =>
        {
            // Check on each axis if adjacent faces need to be created
            Vector3Int coordsMin = coords - Vector3Int.one;

            for (int a = 0; a < 3; a++)
            {
                // If the axes this face goes along aren't at a position greater than 0, then that means there are no previous vertices on those axes to join together. Skip to the next one.
                (int, int) faceAxisInts = a switch
                {
                    0 => (1, 2), // For X, Y and Z
                    1 => (0, 2), // For Y, X and Z
                    2 => (0, 1), // For Z, X and Y
                };
                if (coords[faceAxisInts.Item1] <= 0 || coords[faceAxisInts.Item2] <= 0) continue;

                // Get corner vertices for the faces on different axes
                int faceCornersIndex = (a * 2) + 1;
                // For X, get 1
                // For Y, get 3
                // For Z, get 5

                // Check if there are enough vertices at this point, on this axis, to make a face
                bool enoughVertices = true;
                int[] cornerIndices = faceCornersForAdjacentSquares[faceCornersIndex];
                for (int c = 0; c < 4; c++)
                {
                    Vector3Int cornerPosition = coordsMin + corners[cornerIndices[c]];

                    //bool vertexFound = vertexDictionary.TryGetValue(cornerPosition, out int order);
                    //enoughVertices &= vertexFound;
                    enoughVertices &= vertexDictionary.TryGetValue(cornerPosition, out int order);
                    if (!enoughVertices) break;

                    /*
                    // If value is true, keep it true. If a vertex is not found, mark it as false so the code knows not to try and create a face on this axis.
                    int order = existingVertexPositions.IndexOf(cornerPosition);
                    enoughVertices &= order >= 0;
                    //enoughVertices &= vertexDictionary.TryGetValue(cornerPosition, out int order);
                    if (!enoughVertices) break;
                    */
                    // Assign vertex array order.
                    cornerVertexListPositions[c] = order;
                }
                // Abort if there aren't enough
                if (!enoughVertices) continue;

                throw new System.NotImplementedException();

                /*
                //int order = existingVertexPositions.IndexOf(cornerPosition);
                //int order = vertexDictionary[]
                FillStatusOnAxis fillStatus = (FillStatusOnAxis)gridSpaceNeighbourStatuses[order][a];


                if (fillStatus == FillStatusOnAxis.NoNeighbours || fillStatus == FillStatusOnAxis.BoxedIn) continue;

                bool facingMinus = fillStatus == FillStatusOnAxis.PlusIsInward;
                */
                bool facingMinus = false;

                // Add faces (reorder vertices to flip if necessary)
                if (facingMinus)
                {
                    triangles[createdTriangles] = cornerVertexListPositions[2];
                    triangles[createdTriangles + 1] = cornerVertexListPositions[1];
                    triangles[createdTriangles + 2] = cornerVertexListPositions[0];
                    triangles[createdTriangles + 3] = cornerVertexListPositions[1];
                    triangles[createdTriangles + 4] = cornerVertexListPositions[2];
                    triangles[createdTriangles + 5] = cornerVertexListPositions[3];
                }
                else
                {
                    triangles[createdTriangles] = cornerVertexListPositions[0];
                    triangles[createdTriangles + 1] = cornerVertexListPositions[1];
                    triangles[createdTriangles + 2] = cornerVertexListPositions[2];
                    triangles[createdTriangles + 3] = cornerVertexListPositions[3];
                    triangles[createdTriangles + 4] = cornerVertexListPositions[2];
                    triangles[createdTriangles + 5] = cornerVertexListPositions[1];
                }
                createdTriangles += 6;
            }
        });
    }
}
