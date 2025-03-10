using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class WorldGenerationEditorWindow : EditorWindow
{
    private int dimensions = 10;
    private int numberOfPrefabsToPlace = 3;
    private List<GameObject> prefabs = new List<GameObject>();
    private float snowAmount = 0.0f;
    bool snowEnabled = false;
    
    private Vector2 prefabsScroll;
    private bool prefabSettings = true;

    private const string PrefsKey = "WorldGenSettings"; // Schlüssel zum Speichern der Prefabs

    [MenuItem("Tools/World Generation Tool")]
    public static void ShowWindow() {
        GetWindow<WorldGenerationEditorWindow>("World Generation");
    }

    // Lädt die Einstellungen beim Öffnen
    private void OnEnable() {
        LoadSettings();
    }

    // Speichert die Einstellungen beim Schließen
    private void OnDisable() {
        SaveSettings();
        SetWFCSafeFileSettings();
    }

    private void OnGUI() {
        GUILayout.Label("World Generation Settings", EditorStyles.boldLabel);
        
        GUILayout.Space(30);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Map Size:", GUILayout.Width(150));
        dimensions = EditorGUILayout.IntSlider(dimensions, 10, 50);
        dimensions = Mathf.RoundToInt(dimensions / 4) * 4 + 2;  // Sorgt dafür, dass der Slider in 4er-Schritten funktioniert
        GUILayout.Label( "x " + dimensions, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("10 x 10", GUILayout.Width(100))) dimensions = 10;
        if (GUILayout.Button("30 x 30", GUILayout.Width(100))) dimensions = 30;
        if (GUILayout.Button("50 x 50", GUILayout.Width(100))) dimensions = 50;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        
        prefabSettings = EditorGUILayout.Foldout(prefabSettings, "Prefabs Settings");
        if (prefabSettings) {
            GUILayout.Space(10);
            
            GUILayout.Label("Placeable Prefabs:", EditorStyles.boldLabel);

            GUILayout.BeginVertical();
            prefabsScroll = EditorGUILayout.BeginScrollView(prefabsScroll, GUILayout.ExpandHeight(true));
            for (int i = 0; i < prefabs.Count; i++) {
                GUILayout.BeginHorizontal();

                // Anzeige des Vorschaubildes
                Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabs[i]);
                if (previewTexture != null) {
                    GUILayout.Box(previewTexture, GUILayout.Width(100), GUILayout.Height(50));
                }
                else {
                    GUILayout.Box("No prefab selected", GUILayout.Width(100), GUILayout.Height(50));
                }

                // ObjectField, um ein Prefab zu setzen
                prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);

                // Button, um das Prefab zu entfernen
                if (GUILayout.Button("Remove", GUILayout.Width(100))) {
                    prefabs.RemoveAt(i);
                }

                GUILayout.EndHorizontal();
            }


            // Buttons zur Anpassung der Liste
            GUILayout.Space(10);

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Prefab", GUILayout.Width(250))) {
                prefabs.Add(null); // Leeres Prefab hinzufügen
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Prefabs To Place:", GUILayout.Width(150));
            numberOfPrefabsToPlace = EditorGUILayout.IntSlider(numberOfPrefabsToPlace, 0, 30);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        GUILayout.Space(20);
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Snow Settings:", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Snow Enabled:", GUILayout.Width(100));
        snowEnabled = EditorGUILayout.Toggle(snowEnabled, GUILayout.Width(15));
        if (snowEnabled)
        {
            snowAmount = 1.0f;
            SetSnowSettings();
        }
        else {
            snowAmount = 0.0f;
            SetSnowSettings();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        GUILayout.FlexibleSpace();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Start Generation", GUILayout.Width(250), GUILayout.Height(30))) {
            SetWFCSafeFileSettings();

            // Play-Mode aktivieren
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Stop Generation", GUILayout.Width(250), GUILayout.Height(30))) {
            // Play-Mode deaktivieren
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);
    }

    private void SetSnowSettings() {
        Material material = GameObject.Find("SnowQuad").GetComponent<MeshRenderer>().sharedMaterial;
        material.SetFloat("_Amount", snowAmount);
        
        ParticleSystem.MainModule main = GameObject.Find("Snow").GetComponent<ParticleSystem>().main;
        if (snowEnabled) {
            main.playOnAwake = true;
        }
        else {
            main.playOnAwake = false;
        }
    }
    
    private void SetWFCSafeFileSettings() {
        // WFCSafeFile suchen und aktualisieren
        MapGeneration worldGenerator = FindAnyObjectByType<MapGeneration>();
        if (worldGenerator != null) {
            worldGenerator.Dimensions = dimensions;
            worldGenerator.NumberOfPrefabsToPlace = numberOfPrefabsToPlace;

            // Prefabs-Liste in die PlaygroundPrefabs-Liste im WFCSafeFile übertragen
            worldGenerator.DogParkPrefabs = new List<GameObject>(prefabs);
        }
        else {
            Debug.LogError("No WFCSafeFile script found in scene.");
        }
    }

    private void SaveSettings() {
        // Erstellt eine Liste der Pfade zu den Prefabs
        List<string> prefabPaths = new List<string>();
        foreach (var prefab in prefabs) {
            if (prefab != null) {
                string path = AssetDatabase.GetAssetPath(prefab);
                if (!string.IsNullOrEmpty(path)) {
                    prefabPaths.Add(path);
                }
            }
        }

        // Speichert Prefabs und die zusätzlichen Einstellungen als JSON
        SettingsWrapper wrapper = new SettingsWrapper(dimensions, prefabPaths, numberOfPrefabsToPlace);
        string json = JsonUtility.ToJson(wrapper);
        EditorPrefs.SetString(PrefsKey, json);
    }

    private void LoadSettings() {
        // Lädt die Daten aus EditorPrefs, falls sie existieren
        if (EditorPrefs.HasKey(PrefsKey)) {
            string json = EditorPrefs.GetString(PrefsKey);
            SettingsWrapper wrapper = JsonUtility.FromJson<SettingsWrapper>(json);

            // Konvertiert die gespeicherten Pfade zurück zu Prefabs
            prefabs.Clear();
            foreach (var path in wrapper.prefabPaths) {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                prefabs.Add(prefab);
            }

            dimensions = wrapper.dimensions;
            numberOfPrefabsToPlace = wrapper.numberOfPrefabsToPlace;
        }
        else {
            Debug.Log("No Settings found.");
        }
    }

    // Hilfsklasse zum Speichern der Einstellungen als JSON
    [System.Serializable]
    private class SettingsWrapper
    {
        public int dimensions;
        public List<string> prefabPaths;
        public int numberOfPrefabsToPlace;

        public SettingsWrapper(int dimensions, List<string> paths, int numberOfPrefabsToPlace) {
            this.dimensions = dimensions;
            prefabPaths = paths;
            this.numberOfPrefabsToPlace = numberOfPrefabsToPlace;
        }
    }
}