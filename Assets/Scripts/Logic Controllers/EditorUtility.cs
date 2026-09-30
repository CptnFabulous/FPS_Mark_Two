using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public static class RectUtility
{
    public static void RectEncapsulate(ref Rect target, Vector2 position)
    {
        target.min = Vector2.Min(target.min, position);
        target.max = Vector2.Min(target.max, position);
    }
    public static void RectEncapsulate(ref Rect target, Rect newRect)
    {
        target.min = Vector2.Min(target.min, newRect.min);
        target.max = Vector2.Max(target.max, newRect.max);
    }
}


#if UNITY_EDITOR

public static class EditorWindowUtility
{

    public static void DragHandle(ref Vector2 position, Vector2 size, int windowIndex, string name, out Rect r)
    {
        r = new Rect(position, size);
        position = GUI.Window(windowIndex, r, (_) => GUI.DragWindow(), name).position;
    }







    public static float CloserOfTwoValues(float a, float b, float value)
    {
        // BUG: this function always seems to be returning 'a'.

        float disA = a - value;
        float disB = b - value;

        disA = Mathf.Abs(disA);
        disB = Mathf.Abs(disB);

        if (disA < disB) return a;
        if (disA > disB) return b;

        return value;
    }

    public static bool IsVertical(Vector2 direction)
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);
        //if (absX == absY) return r.center;
        return absY > absX;
    }
    public static Vector2 GetRectEdgePoint(Rect r, Vector2 direction, float edgeLerp)
    {
        Vector2 offset = r.center + direction;

        // Figure out if directon is horizontal or vertical
        bool isVertical = IsVertical(direction);

        Vector2 final = new Vector2();
        if (isVertical)
        {
            final.x = Mathf.Lerp(r.xMin, r.xMax, edgeLerp);
            final.y = CloserOfTwoValues(r.yMin, r.yMax, offset.y);
        }
        else
        {
            final.x = CloserOfTwoValues(r.xMin, r.xMax, offset.x);
            final.y = Mathf.Lerp(r.yMin, r.yMax, edgeLerp);
        }

        return final;
    }


    #region Arrows

    public static void DrawArrow(Rect from, Rect to)
    {
        Vector2 difference = to.center - from.center;

        DrawArrow(from, difference, to, difference);
    }
    public static void DrawArrow(Rect from, Vector2 fromDir, Rect to, Vector2 toDir, float edgeLerpFrom = 0.5f, float edgeLerpTo = 0.5f)
    {

        // Turn arrow directions into clean angles
        fromDir.x = Mathf.Round(fromDir.x);
        fromDir.y = Mathf.Round(fromDir.y);
        toDir.x = Mathf.Round(toDir.x);
        toDir.y = Mathf.Round(toDir.y);

        Vector2 startPos = GetRectEdgePoint(from, fromDir, edgeLerpFrom);
        Vector2 endPos = GetRectEdgePoint(to, -toDir, edgeLerpTo);
        DrawArrow(startPos, fromDir, endPos, toDir);
    }
    public static void DrawArrow(Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, float arrowDistance = 5)
    {
        Handles.BeginGUI();

        Vector2 difference = endPos - startPos;
        float p2Dis = difference.magnitude;//IsVertical(startDir) ? difference.y : difference.x;
        float p3Dis = difference.magnitude;//IsVertical(endDir) ? difference.y : difference.x;

        Vector2 differenceDirection = difference.normalized;
        float fromMultiplier = 1 - Vector2.Dot(differenceDirection, startDir);
        float toMultiplier = 1 - Vector2.Dot(differenceDirection, endDir);

        Vector2 startTangent = startPos + (p2Dis * fromMultiplier * startDir);
        Vector2 endTangent = endPos + -(p3Dis * toMultiplier * endDir);

        // Draw line
        Handles.color = Color.white;
        Handles.DrawBezier(startPos, endPos, startTangent, endTangent, Handles.color, null, Handles.lineThickness);

        // Draw arrowhead
        Vector2 sidewaysOffset = arrowDistance * Vector2.Perpendicular(endDir);
        Vector2 backwardOffset = arrowDistance * -endDir;
        Handles.DrawLine(endPos, endPos + backwardOffset + sidewaysOffset);
        Handles.DrawLine(endPos, endPos + backwardOffset + -sidewaysOffset);

        /*
        // Debug tangent lines
        Handles.color = Color.red;
        Handles.DrawLine(startPos, startTangent);
        Handles.color = Color.green;
        Handles.DrawLine(endPos, endTangent);
        */

        Handles.EndGUI();
    }

    #endregion
}

#endif
