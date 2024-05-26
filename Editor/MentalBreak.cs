using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;

public class CustomWindow : EditorWindow
{
    static List<Texture2D> frames = new List<Texture2D>();
    static int currentFrame;
    static float frameRate = 16f; // Adjust this to change the speed of the animation
    static float nextFrameTime;
    static string audioPath = "Packages/wtf.buddyworks.editortools/Data/Vibes.mp3";
    static string imagePath = "Packages/wtf.buddyworks.editortools/Data/MentalBreakFrames/";
    private const int minWidth = 400; // Minimum width of the window
    private const int minHeight = 300; // Minimum height of the window

    [MenuItem("BUDDYWORKS/Editor Tools/BreakTime %#&w")] // CTRL + ALT + Shift + W
    private static void ShowWindow()
    {
        GetWindow<CustomWindow>("Time for a break");
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
        StopAllClips();
        PlayClip(clip);
    }

    private void OnEnable()
    {
        LoadFrames();
        minSize = new Vector2(minWidth, minHeight);
    }

    private void OnDestroy()
    {
        StopAllClips();
    }

    private void LoadFrames()
    {
        string[] files = Directory.GetFiles(imagePath, "*.png");

        foreach (string file in files)
        {
            Texture2D frame = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
            if (frame != null)
            {
                frames.Add(frame);
            }
        }

        currentFrame = 0;
        nextFrameTime = Time.realtimeSinceStartup + (1f / frameRate);
    }

    private void Update()
    {
        if (frames.Count > 0 && Time.realtimeSinceStartup >= nextFrameTime)
        {
            currentFrame = (currentFrame + 1) % frames.Count;
            nextFrameTime = Time.realtimeSinceStartup + (1f / frameRate);
            Repaint();
        }
    }

    private void OnGUI()
    {
        // Center content using a vertical layout with a horizontal layout inside
        GUILayout.BeginVertical(); GUILayout.FlexibleSpace();
        
        if (frames.Count > 0) {
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.Label(frames[currentFrame], GUILayout.Width(frames[currentFrame].width), GUILayout.Height(frames[currentFrame].height));
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
        }
        else {
            Debug.Log("How can you vibe if you miss the images? Missing: " + imagePath);
        }

        GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
        GUILayout.Label("Sometimes, you just need a break.", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();

        if (GUILayout.Button("Get back to work", GUILayout.Height(40), GUILayout.Width(200))) {
            StopAllClips();
            this.Close();
        }

        GUILayout.FlexibleSpace(); GUILayout.EndHorizontal(); GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();

            GUILayout.Label("BUDDYWORKS Editor Tools", EditorStyles.boldLabel);
            Rect labelRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL("https://github.com/BUDDYWORKS-VR/editortools");
            }
    }

    private static void PlayClip(AudioClip clip, int startSample = 0, bool loop = true)
    {
         Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
     
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null
        );
 
        //Debug.Log(method);
        method.Invoke(
            null,
            new object[] { clip, startSample, loop }
        );
    }
 
    private static void StopAllClips()
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
 
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "StopAllPreviewClips",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { },
            null
        );
 
        //Debug.Log(method);
        method.Invoke(
            null,
            new object[] { }
        );
    }
}
