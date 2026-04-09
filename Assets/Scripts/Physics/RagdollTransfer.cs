using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RagdollTransfer : EditorWindow
{
    public SkinnedMeshRenderer origin;
    public SkinnedMeshRenderer target;
    public Animator animator;
    public AnimationClip targetAnimation;

    [MenuItem("Window/Physics Utility/Ragdoll Transfer Tool")]
    static void ShowWindow()
    {
        RagdollTransfer window = GetWindow<RagdollTransfer>();
        window.titleContent = new GUIContent("Ragdoll Transfer Tool");
    }
    
    public static T ObjectField<T>(string name, T currentValue) where T : UnityEngine.Object
    {
        return (T)EditorGUILayout.ObjectField(new GUIContent(name), currentValue, typeof(T));
    }
    public static bool ActiveSettableGUIButton(string name, string tooltip, string errorMessage, bool isActive)
    {
        if (!isActive)
        {
            EditorGUILayout.LabelField(errorMessage);
        }
        GUI.enabled = isActive;
        bool pressed = GUI.Button(EditorGUILayout.GetControlRect(), new GUIContent(name, tooltip));
        GUI.enabled = true;
        return pressed;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        origin = ObjectField("Origin", origin);
        target = ObjectField("Target", target);
        bool canTransfer = origin != null && target != null && origin != target;
        if (ActiveSettableGUIButton("Transfer", "This will be updated to recognise the origin's equivalent bones on the new skeleton.", "Origin and target need to be valid and separate", canTransfer))
        {
            Transfer(origin, target);
        }

        EditorGUILayout.Space();

        animator = ObjectField("Target Animator", animator);
        targetAnimation = ObjectField("Target Animation Clip", targetAnimation);
        bool canUpdateAnimation = animator != null && targetAnimation != null;
        if (ActiveSettableGUIButton("Transfer Animation", "This animation will be updated to recognise the target's components with the same names as in the animation clip", "Animator and clip need to be valid!", canUpdateAnimation))
        {
            TransferAnimation(animator, targetAnimation);
        }

        EditorGUILayout.EndVertical();
    }
    //void On

    public static void Transfer(SkinnedMeshRenderer origin, SkinnedMeshRenderer target)
    {
        Transform targetTransform = target.rootBone;

        Transform[] allPossibleChildBones = targetTransform.GetComponentsInChildren<Transform>();

        // TO DO: create a copy of the origin's bone array.
        int boneCount = origin.bones.Length;
        Transform[] newBones = new Transform[boneCount];

        string rootBoneName = origin.rootBone.name;
        target.rootBone = allPossibleChildBones.First((t) => t.name == rootBoneName);
        Debug.Log($"Root bone = {rootBoneName}, matching bone found = {target.rootBone != null}");

        for (int i = 0; i < boneCount; i++)
        {
            // For each one, find a child bone in targetTransform that has the same name as the original
            string oldBoneName = origin.bones[i].name;
            newBones[i] = allPossibleChildBones.First((t) => t.name == oldBoneName);
            Debug.Log($"Bone #{i + 1}/{boneCount} = {oldBoneName}, matching bone found = {newBones[i] != null}");
        }

        target.bones = newBones;

        EditorUtility.SetDirty(target);
    }
    public static void TransferAnimation(Animator animator, AnimationClip targetAnimation)
    {
        Transform[] allPossibleChildBones = animator.transform.GetComponentsInChildren<Transform>();

        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(targetAnimation);
        int curveCount = curveBindings.Length;
        AnimationCurve[] curves = new AnimationCurve[curveCount];
        for (int i = 0; i < curveCount; i++)
        {
            EditorCurveBinding binding = curveBindings[i];
            curves[i] = AnimationUtility.GetEditorCurve(targetAnimation, binding);

            //Debug.Log($"Curve #{i + 1}/{curveCount} = {binding.propertyName}, path = {binding.path}");

            // Get name of bone
            string oldPath = binding.path;
            string boneName = oldPath.Substring(oldPath.LastIndexOf('/') + 1);
            // Find equivalent bone in new hierarchy
            Transform newBone = allPossibleChildBones.First((t) => t.name == boneName);
            // Assign a new path based on new bone
            curveBindings[i].path = AnimationUtility.CalculateTransformPath(newBone, animator.transform);

        }

        // Add the same curves but with new bindings
        targetAnimation.ClearCurves();
        AnimationUtility.SetEditorCurves(targetAnimation, curveBindings, curves);

        // Mark that changes have been made
        EditorUtility.SetDirty(targetAnimation);
    }
}