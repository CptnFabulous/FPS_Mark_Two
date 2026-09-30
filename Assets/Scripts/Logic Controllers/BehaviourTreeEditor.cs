using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(BehaviourTree))]
public class BehaviourTreeEditor : Editor
{
    BehaviourTreeBase targetTree => target as BehaviourTreeBase;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        if (GUILayout.Button("Open viewer"))
        {
            //Debug.Log($"Setting target to {}")
            BehaviourTreeEditorWindow.target = targetTree;
            EditorWindow.GetWindow<BehaviourTreeEditorWindow>();
        }
    }
}

[EditorWindowTitle(icon = null, title = "Behaviour Tree", useTypeNameAsIconName = true)]
public class BehaviourTreeEditorWindow : EditorWindow
{



    

    Vector2 nodeSize = new Vector2(160, 20);
    float nodeSpacing = 10;
    float branchBorder = 15;
    float branchSpacing = 25;


    public static BehaviourTreeBase target;
    Vector2 scrollPosition = Vector2.zero;



    int windowsDrawn;


    //GUIStyle branchStyle;

    /*
    Vector3 p0 = new Vector3(0, 0, 0);
    Vector3 p1 = new Vector3(50, 0, 0);
    Vector3 p2 = new Vector3(100, 0, 0);
    Vector3 p3 = new Vector3(150, 0, 0);
    */

    [MenuItem("Window/State Machine Editor")]
    static void ShowEditor() => EditorWindow.GetWindow<BehaviourTreeEditorWindow>();

    private void OnEnable()
    {
        //branchStyle = new GUIStyle(EditorStyles.miniButton);
        //branchStyle.normal.background = null;
        //branchStyle.border = new RectOffset(5, 5, 5, 5);
    }
    private void OnGUI()
    {

        /*
        void DragHandle(ref Vector3 position, int index, string name)
        {
            Vector2 size = new Vector2(50, 50);
            Rect r = new Rect(position, size);
            position = GUI.Window(index, r, (_) => GUI.DragWindow(), name).position;
        }

        DragHandle(ref p0, 0, "P0");
        DragHandle(ref p1, 1, "P1");
        DragHandle(ref p2, 2, "P2");
        DragHandle(ref p3, 3, "P3");

        Handles.BeginGUI();
        Handles.DrawBezier(p0, p1, p2, p3, Color.red, null, 2);
        Handles.EndGUI();
        */

        if (target == null)
        {
            GUI.Label(rootVisualElement.contentRect, "No behaviour tree selected.");
            return;
        }

        Rect fullWindow = rootVisualElement.contentRect;
        fullWindow.size *= 5;
        scrollPosition = GUI.BeginScrollView(rootVisualElement.contentRect, scrollPosition, fullWindow);

        // TO DO: how do I have the scroll window be sized accurately to everything on screen, even though we're only figuring out the sizes of everything after rendering them?

        #region Draw windows

        windowsDrawn = 0;
        BeginWindows();

        // TO DO: draw start
        // TO DO: draw arrow connecting start and main branch

        // Draw initial branch
        DrawBranch(target, 0, new Vector2(50, 50), out Rect wholeSize);

        // TO DO: draw finish
        // TO DO: draw arrow connecting main branch and finish

        EndWindows();

        DrawObjectsWithOrder.DrawQueuedThings();

        #endregion

        GUI.EndScrollView();
    }


    void DrawBranch(BehaviourTreeBase branch, int layerIndex, Vector2 position, out Rect branchRect)
    {
        // TO DO: make different functions for different node types
        
        
        
        
        
        
        
        
        
        
        
        
        
        branchRect = new Rect(position, Vector2.zero);

        Vector2 borderMin = new Vector2(branchBorder, Mathf.Max(branchBorder, 40));
        Vector2 borderMax = new Vector2(branchBorder, branchBorder);

        // Calculate how each node needs to be placed
        bool isVertical = NodeIsVertical(branch);

        Vector2 offsetPerNode = CalculateOffsetForNextNode(nodeSize, nodeSpacing, isVertical);
        Vector2 offsetForSubBranch = CalculateOffsetForNextNode(nodeSize, branchSpacing, !isVertical);

        Vector2 normalDirection = offsetPerNode.normalized;
        Vector2 perpendicular = offsetForSubBranch.normalized;


        int nextLayer = layerIndex + 1;



        


        Vector2 arrowStartPos = Vector2.zero;
        Vector2 arrowStartDir = Vector2.zero;
        Vector2 positionInBranch = position + borderMin;
        for (int i = 0; i < branch.Count; i++)
        {
            BehaviourTreeNode node = branch[i];


            // TO DO: check if the current node is actually another branch, that has already been used further up the tree.
            // In that case it's recursive, we don't want to draw another version of it and create an infinite loop.
            // Draw a line leading from the end of the previous node to the start of that branch.

            


            Rect newRect;

            Vector2 nodePosition;
            Vector2 arrowEnd;
            Vector2 arrowEndDir;
            Vector2 newArrowStart;
            Vector2 newArrowStartDir;

            BehaviourTreeBase twig = node as BehaviourTreeBase;
            bool isTwig = node is BehaviourTreeBase;

            // Determines if the next child node should be offset
            bool offsetNode = isTwig && NodeIsVertical(twig) == isVertical;
            nodePosition = positionInBranch;
            if (offsetNode) nodePosition += offsetForSubBranch;

            // Draw new state and expand the branch rect to include it
            if (isTwig)
            {
                DrawBranch(twig, nextLayer + 1, nodePosition, out newRect);
                positionInBranch += CalculateOffsetForNextNode(newRect.size, nodeSpacing, isVertical);
            }
            else
            {
                newRect = new Rect(positionInBranch, nodeSize);
                BehaviourTreeNode newNode = EditorGUI.ObjectField(newRect, new GUIContent(node.name), node, typeof(BehaviourTreeNode), true) as BehaviourTreeNode;
                positionInBranch += offsetPerNode;
            }
            RectEncapsulate(ref branchRect, newRect);

            // Determine how the arrow should be drawn connecting the current state with the previous one
            if (offsetNode)
            {
                Vector2 arrowOffset = 0.25f * newRect.size * normalDirection;
                arrowEnd = CalculateArrowPoint(newRect, perpendicular) - arrowOffset;
                arrowEndDir = perpendicular;
                newArrowStart = CalculateArrowPoint(newRect, perpendicular) + arrowOffset;
                newArrowStartDir = -perpendicular;
            }
            else
            {
                arrowEnd = CalculateArrowPoint(newRect, normalDirection);
                arrowEndDir = normalDirection;
                newArrowStart = CalculateArrowPoint(newRect, -normalDirection);
                newArrowStartDir = normalDirection;
            }

            if (i > 0)
            {
                DrawArrow(arrowStartPos, arrowStartDir, arrowEnd, arrowEndDir);
                //DrawObjectsWithOrder.QueueThingToDraw(nextLayer, () => DrawArrow(arrowStart, normalDirection, arrowEnd, normalDirection));
            }
            arrowStartPos = newArrowStart;
            arrowStartDir = newArrowStartDir;

        }

        branchRect.max += borderMax;


        DrawButton(layerIndex, branch, branchRect);
    }

    void DrawButton(int index, BehaviourTreeNode node, Rect rect)
    {
        string name = node.name;
        /*
        if (node is BehaviourTreeBase branch)
        {
            string typeName = Enum.GetName(typeof(BehaviourTree.BehaviourTreeType), branch.type);
            name = $"{node.name} ({typeName})";
        }
        */
        DrawButton(index, rect, name);
    }
    void DrawButton(int index, Rect rect, string name)
    {


        

        GUI.Box(rect, name);
        return;

        // Add to some kind of list

        DrawObjectsWithOrder.QueueThingToDraw(index, () =>
        {
            GUI.Box(rect, name);
            /*
            if (GUI.Button(rect, name))
            {
                
                Editor e = Editor.CreateEditor(node);
                e.OnInspectorGUI();
                DestroyImmediate(e);
                
            }
            */
        });

        
    }
    

    Vector2 CalculateOffsetForNextNode(Vector2 size, float spacing, bool vertical)
    {
        if (vertical)
        {
            return new Vector2(0, size.y + spacing);
        }
        else
        {
            return new Vector2(size.x + spacing, 0);
        }
    }
    Vector2 CalculateArrowPoint(Rect rect, Vector2 direction)
    {
        direction = direction.normalized;
        direction.x = -direction.x;
        direction.y = -direction.y;

        direction.x += 1;
        direction.x *= 0.5f;
        direction.y += 1;
        direction.y *= 0.5f;


        float x = Mathf.Lerp(rect.min.x, rect.max.x, direction.x);
        float y = Mathf.Lerp(rect.min.y, rect.max.y, direction.y);
        return new Vector2(x, y);
    }

    void RectEncapsulate(ref Rect target, Vector2 position)
    {
        target.min = Vector2.Min(target.min, position);
        target.max = Vector2.Min(target.max, position);
    }
    void RectEncapsulate(ref Rect target, Rect newRect)
    {
        target.min = Vector2.Min(target.min, newRect.min);
        target.max = Vector2.Max(target.max, newRect.max);
    }


    bool NodeIsVertical(BehaviourTreeBase branch)
    {
        bool isVertical = branch.type == BehaviourTreeBase.BehaviourTreeType.Troubleshooting;
        isVertical = !isVertical;
        isVertical = true;

        return isVertical;
    }






    void DrawArrow(Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, float arrowDistance = 5)
    {

        Handles.BeginGUI();

        // Draw line
        float multiplier = (endPos - startPos).magnitude * 0.5f;
        Vector2 p2 = startPos + (multiplier * startDir);
        Vector2 p3 = endPos + (multiplier * -endDir);
        Handles.DrawBezier(startPos, endPos, p2, p3, Color.white, null, 2);
        
        // Draw arrowhead
        Vector2 sidewaysOffset = arrowDistance * Vector2.Perpendicular(endDir);
        Vector2 backwardOffset = arrowDistance * -endDir;
        Handles.DrawLine(endPos, endPos + backwardOffset + sidewaysOffset);
        Handles.DrawLine(endPos, endPos + backwardOffset + -sidewaysOffset);

        Handles.EndGUI();
    }
}



public static class DrawObjectsWithOrder
{
    static System.Action[] drawCalls;
    
    static int start = 0;
    static int end = 0;

    public static void QueueThingToDraw(int layerOrder, System.Action drawThing)
    {
        if (drawCalls == null)
        {
            drawCalls = new System.Action[100];
            start = int.MaxValue;
            end = 0;
        }
        else if (drawCalls.Length <= layerOrder)
        {
            System.Action[] oldDrawCalls = drawCalls;
            drawCalls = new System.Action[layerOrder];
            for (int i = 0; i < layerOrder; i++)
            {
                drawCalls[i] = oldDrawCalls[i];
            }
        }

        drawCalls[layerOrder] += drawThing;
        start = Mathf.Min(0, layerOrder);
        end = Mathf.Max(end, layerOrder);
    }

    public static void DrawQueuedThings()
    {
        for (int i = start; i < end; i++)
        {
            drawCalls[i]?.Invoke();
            drawCalls[i] = null;
        }
        start = int.MaxValue;
        end = 0;
    }
}

#endif