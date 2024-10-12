using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AnimationKeyframeMerger : EditorWindow
{
    string sourceFolderPath = "Specify source folder";
    AnimationClip targetClip;
    bool excludeNonAnimatorProperties = false; // Checkbox to exclude non-Animator properties
    float intervalBetweenClips = 1f; // Interval in seconds between merged clips

    [MenuItem("BUDDYWORKS/Editor Tools/Animation Keyframe Merger")]
    public static void ShowWindow()
    {
        GetWindow<AnimationKeyframeMerger>("Animation Keyframe Merger");
    }

    void OnGUI()
    {
       
        GUILayout.Label("Merge Animation Keyframes", EditorStyles.boldLabel);
        Rect r = EditorGUILayout.GetControlRect(false, 1, new GUIStyle() { margin = new RectOffset(0, 0, 4, 4) });
        EditorGUI.DrawRect(r, Color.gray);
        // Source folder selection
        GUILayout.Label("Source Animation Folder:");
        if (GUILayout.Button("Select Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Animation Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                sourceFolderPath = path;
            }
        }
        GUILayout.Label("Selected Folder: " + sourceFolderPath, EditorStyles.miniLabel);
        GUILayout.Space(10); // Add space after the title
        
        // Checkbox for excluding non-Animator properties
        GUILayout.BeginHorizontal();
        excludeNonAnimatorProperties = EditorGUILayout.Toggle(excludeNonAnimatorProperties, GUILayout.Width(14)); // Fixed width for checkbox
        GUILayout.Label("Only include humanoid pose properties.", GUILayout.Width(250)); // Fixed width for label
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        // Target animation clip selection
        GUILayout.Label("Target Animation Clip:");
        targetClip = (AnimationClip)EditorGUILayout.ObjectField(targetClip, typeof(AnimationClip), false);

        

        // Interval setting
        //GUILayout.Label("Interval Between Clips (in seconds):");
        //intervalBetweenClips = EditorGUILayout.FloatField(intervalBetweenClips, GUILayout.Width(200));
        GUILayout.Space(20);
        // Merge button
        if (GUILayout.Button("Merge Animations"))
        {
            if (!string.IsNullOrEmpty(sourceFolderPath) && targetClip != null)
            {
                MergeAnimations();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please specify both the source folder and the target animation clip.", "OK");
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label("BUDDYWORKS Editor Tools - Animation Merger", EditorStyles.boldLabel);
    }

    void MergeAnimations()
    {
        // Get all animation files from the folder
        string[] animationFiles = Directory.GetFiles(sourceFolderPath, "*.anim", SearchOption.AllDirectories);
        List<AnimationClip> sourceClips = new List<AnimationClip>();

        foreach (string file in animationFiles)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(file.Substring(file.IndexOf("Assets")));
            if (clip != null)
            {
                sourceClips.Add(clip);
            }
        }

        if (sourceClips.Count == 0)
        {
            EditorUtility.DisplayDialog("No Animations Found", "No animation files found in the specified folder.", "OK");
            return;
        }

        float currentTime = 0f; // Reset currentTime to 0 for merging
        foreach (AnimationClip sourceClip in sourceClips)
        {
            MergeKeyframes(sourceClip, targetClip, ref currentTime);
            currentTime += intervalBetweenClips; // Increment currentTime by the defined interval
        }

        EditorUtility.SetDirty(targetClip);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Success", "Animations merged successfully!", "OK");
    }

    void MergeKeyframes(AnimationClip sourceClip, AnimationClip targetClip, ref float currentTime)
    {
        var bindings = AnimationUtility.GetCurveBindings(sourceClip);

        foreach (var binding in bindings)
        {
            // If the checkbox is checked, exclude non-Animator properties
            if (excludeNonAnimatorProperties && !IsAnimatorProperty(binding))
            {
                continue; // Skip non-Animator properties
            }

            AnimationCurve sourceCurve = AnimationUtility.GetEditorCurve(sourceClip, binding);
            AnimationCurve targetCurve = AnimationUtility.GetEditorCurve(targetClip, binding);

            if (targetCurve == null)
            {
                targetCurve = new AnimationCurve();
            }

            foreach (Keyframe key in sourceCurve.keys)
            {
                Keyframe newKey = new Keyframe(key.time + currentTime, key.value, key.inTangent, key.outTangent);
                targetCurve.AddKey(newKey);
            }

            AnimationUtility.SetEditorCurve(targetClip, binding, targetCurve);
        }
    }

    // Method to determine if a binding is an Animator property
    bool IsAnimatorProperty(EditorCurveBinding binding)
    {
        // Check if the binding property path is related to an Animator component
        return binding.type == typeof(Animator);
    }
}
