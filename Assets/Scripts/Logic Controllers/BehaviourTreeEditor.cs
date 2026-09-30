using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR

[CustomEditor(typeof(CustomBehaviourTreeBranch))]
public class BehaviourTreeEditor : Editor
{
    BehaviourTreeBranch targetTree => target as BehaviourTreeBranch;
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
    float nodeSpacing = 0;
    float branchBorder = 15;
    float branchSpacing = 25;


    public static BehaviourTreeBranch target;
    Vector2 scrollPosition = Vector2.zero;



    int windowsDrawn;

    //System.Action drawWindows;
    //System.Action drawContent;

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
        EditorUtility.DragHandle(ref p0, 0, "P0", out Rect r0);
        EditorUtility.DragHandle(ref p1, 1, "P1", out Rect r1);
        EditorUtility.DragHandle(ref p2, 2, "P2", out Rect r2);
        EditorUtility.DragHandle(ref p3, 3, "P3", out Rect r3);

        EditorUtility.DrawArrow(r0, r1);

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

        //drawWindows = null;
        //drawContent = null;

        windowsDrawn = 0;
        BeginWindows();

        // TO DO: draw start
        // TO DO: draw arrow connecting start and main branch

        // Draw initial branch
        DrawBranch(target, 0, new Vector2(50, 50), out Rect wholeSize);

        // TO DO: draw finish
        // TO DO: draw arrow connecting main branch and finish

        //drawWindows?.Invoke();
        //drawContent?.Invoke();

        EndWindows();

        DrawObjectsWithOrder.DrawQueuedThings();

        #endregion

        GUI.EndScrollView();
    }


    void DrawBranch(BehaviourTreeBranch branch, int layerIndex, Vector2 position, out Rect branchRect)
    {
        // Initial rect, will be resized to account for everything inside branch
        branchRect = new Rect(position, Vector2.zero);

        // Borders
        Vector2 borderMin = new Vector2(branchBorder, Mathf.Max(branchBorder, 40));
        Vector2 borderMax = new Vector2(branchBorder, branchBorder);

        // Calculate offsets and directions for placing nodes
        bool isVertical = NodeIsVertical(branch);
        Vector2 straightOffset = CalculateOffsetForNextNode(nodeSize, nodeSpacing, isVertical);
        Vector2 perpendicularOffset = CalculateOffsetForNextNode(nodeSize, branchSpacing, !isVertical);
        Vector2 straight = straightOffset.normalized;
        Vector2 perpendicular = perpendicularOffset.normalized;

        // Is this a custom branch? Will determine if this branch can be edited by the user
        CustomBehaviourTreeBranch customBranch = branch as CustomBehaviourTreeBranch;
        bool optionsCanBeAltered = customBranch != null;

        // Initial position, updated each time a new node is drawn to ensure consistent layouts
        Vector2 positionInBranch = position + borderMin;
        for (int i = 0; i < branch.Count; i++)
        {
            BehaviourTreeNode node = branch[i];


            // TO DO: check if the current node is actually another branch, that has already been used further up the tree.
            // In that case it's recursive, we don't want to draw another version of it and create an infinite loop.
            // Draw a line leading from the end of the previous node to the start of that branch.


            // Draw the initial field for the node
            Rect standardNodeRect = new Rect(positionInBranch, nodeSize);
            /*
            drawContent += () =>
            {
                
            };
            */

            BehaviourTreeNode newNode = EditorGUI.ObjectField(standardNodeRect, new GUIContent(node.name), node, typeof(BehaviourTreeNode), true) as BehaviourTreeNode;
            if (optionsCanBeAltered && newNode != node)
            {
                string message = $"{customBranch.name}: changed node #{i} to {newNode}";
                //Debug.Log(message);
                customBranch.nodes[i] = newNode;
                Undo.RecordObject(customBranch, message);
            }




            Rect newRect;
            Vector2 nodePosition = positionInBranch;

            BehaviourTreeBranch twig = node as BehaviourTreeBranch;
            bool isTwig = node is BehaviourTreeBranch;

            bool offsetNode = isTwig;// && NodeIsVertical(twig) == isVertical;
            if (offsetNode) nodePosition += perpendicularOffset;

            // Draw new state and expand the branch rect to include it
            if (isTwig)
            {
                DrawBranch(twig, layerIndex + 1, nodePosition, out newRect);

                // Draw arrow connecting from side of standardNodeRect to newRect
                float topOfRect = standardNodeRect.height * 0.5f / newRect.height;
                /*drawContent += () => */EditorWindowUtility.DrawArrow(standardNodeRect, perpendicular, newRect, perpendicular, 0.5f, topOfRect);

                positionInBranch += CalculateOffsetForNextNode(newRect.size, nodeSpacing, isVertical);

                // Draw arrow connecting from end of branch to start of next node
                Rect nextNodeRect = new Rect(positionInBranch, nodeSize);
                /*drawContent += () => */EditorWindowUtility.DrawArrow(newRect, -perpendicular, nextNodeRect, straight, 0.5f, 0.5f);
            }
            else
            {
                newRect = standardNodeRect;
                positionInBranch += straightOffset;
            }

            RectUtility.RectEncapsulate(ref branchRect, newRect);
        }

        branchRect.max += borderMax;

        GUIStyle branchWindowStyle = new GUIStyle();
        //branchWindowStyle.
        //EditorGUI.Wi

        // Draw background
        // TO DO: have it not render over the text and arrows
        Rect windowRect = branchRect;
        /*drawWindows += () => */GUI.Box(windowRect, branch.name);

        //GUI.BeginClip
        //GUI.Window()
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
    bool NodeIsVertical(BehaviourTreeBranch branch)
    {
        return true;

        bool isVertical = branch.type == BehaviourTreeBranch.BehaviourTreeType.Troubleshooting;
        isVertical = !isVertical;

        return isVertical;
    }


}



public static class DrawObjectsWithOrder
{
    static System.Action[] drawCalls;
    
    static int start = int.MaxValue;
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