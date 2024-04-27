using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BUDDYWORKS.EditorTools
{
public class History : EditorWindow, IHasCustomMenu
{
    private List<Object> clickedItems = new List<Object>();
    private HashSet<Object> protectedItems = new HashSet<Object>(); // Store protected items
    private Vector2 scrollPosition;

    private const int maxHistorySize = 20; // Maximum size of the history list
    private const float iconScaleFactor = 1.00f; // Scale factor for resizing icons

    [MenuItem("BUDDYWORKS/Editor Tools/History")]
    public static void ShowWindow()
    {
        GetWindow(typeof(History));
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {
        // Display the list of clicked items in reverse order
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < clickedItems.Count; i++)
        {
            if (clickedItems[i] != null)
            {
                DrawItem(clickedItems[i]);
                GUILayout.Space(1); // Adjust spacing between list items
            }
        }
        GUILayout.EndScrollView();
        GUILayout.Label("BUDDYWORKS Editor History - Right-click icon to protect from limit.", EditorStyles.boldLabel);
    }

    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Clear"), false, () =>
        {
            clickedItems.Clear();
        });
    }
    void DrawItem(Object item)
    {
        EditorGUILayout.BeginHorizontal();

        // Display the icon with a scaled size
        Texture2D icon = AssetPreview.GetMiniThumbnail(item);
        GUILayout.Label(icon, GUILayout.Width(22 * iconScaleFactor), GUILayout.Height(16 * iconScaleFactor));

        // Display the name with adjusted vertical alignment
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.fixedHeight = 16 * iconScaleFactor; // Adjust height to match icon

        // Change text color if the item is part of a prefab, pinned, or protected
        if (protectedItems.Contains(item)) // Check if the item is protected
        {
            labelStyle.normal.textColor = new Color(0.494f, 0.624f, 0.494f); // #7e9f7e
        }
        else if (IsPartOfPrefab(item))
        {
            labelStyle.normal.textColor = new Color(0.447f, 0.616f, 0.863f); // #729ddc
        }
        else
        {
            labelStyle.normal.textColor = Color.white;
        }

        // Check for right-click on the icon
        Rect iconRect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && iconRect.Contains(Event.current.mousePosition))
        {
            // Right-clicked on the icon, change text color and protect the item
            labelStyle.normal.textColor = new Color(0.494f, 0.624f, 0.494f); // #7e9f7e
            if (protectedItems.Contains(item))
                protectedItems.Remove(item);
            else protectedItems.Add(item);
        }

        if (GUILayout.Button(item.name, labelStyle))
        {
            Selection.activeObject = item;
            EditorGUIUtility.PingObject(item);
        }

        EditorGUILayout.EndHorizontal();
    }

    bool IsPartOfPrefab(Object obj)
    {
        // Check if the object is a GameObject
        GameObject gameObject = obj as GameObject;
        if (gameObject == null)
            return false;

        // Check if the GameObject or any of its parents are part of a prefab
        while (gameObject != null)
        {
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(gameObject);
            if (prefabType != PrefabAssetType.NotAPrefab)
                return true;

            // Honestly no clue what this does, it does however protect a Repaint Loop... so thats neat I guess.
            gameObject = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject : null;
        }

        return false;
    }

    void OnEnable()
    {
        // Subscribe to the mouseDown event in the Scene view
        SceneView.duringSceneGui += OnSceneGUI;
        // Subscribe to the mouseDown event in the Hierarchy window
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        // Subscribe to the mouseDown event in the Project window
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    void OnDisable()
    {
        // Unsubscribe from the mouseDown event in the Scene view
        SceneView.duringSceneGui -= OnSceneGUI;
        // Unsubscribe from the mouseDown event in the Hierarchy window
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemGUI;
        // Unsubscribe from the mouseDown event in the Project window
        EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        // Check if the left mouse button was clicked
        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            // Raycast from the mouse position in the Scene view
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                // Add or move the clicked GameObject to the top of the history list
                MoveToTopOrAdd(hitInfo.collider.gameObject);
                // Repaint the window to reflect the updated history
                Repaint();
            }
        }
    }

    void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
    {
        // Check if the left mouse button was clicked
        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && selectionRect.Contains(currentEvent.mousePosition))
        {
            // Get the GameObject associated with the clicked instanceID
            GameObject clickedObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (clickedObject != null)
            {
                // Add or move the clicked GameObject to the top of the history list
                MoveToTopOrAdd(clickedObject);
                // Repaint the window to reflect the updated history
                Repaint();
            }
        }
    }

    void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        // Check if the left mouse button was clicked
        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && selectionRect.Contains(currentEvent.mousePosition))
        {
            // Get the asset associated with the clicked GUID
            Object clickedAsset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
            if (clickedAsset != null)
            {
                // Add or move the clicked asset to the top of the history list
                MoveToTopOrAdd(clickedAsset);
                // Repaint the window to reflect the updated history
                Repaint();
            }
        }
    }

    void MoveToTopOrAdd(Object item)
    {
        // If the item already exists in the list, remove it
        if (clickedItems.Contains(item))
        {
            clickedItems.Remove(item);
        }

        // Add the item to the top of the list
        clickedItems.Insert(0, item);

        // If the list size exceeds the maximum history size, remove the oldest item
        if (clickedItems.Count > maxHistorySize)
        {
            // Check if the oldest item is protected before removing it
            if (!protectedItems.Contains(clickedItems[maxHistorySize]))
            {
                clickedItems.RemoveAt(maxHistorySize);
            }
        }

    }

}
}