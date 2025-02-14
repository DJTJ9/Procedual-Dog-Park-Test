using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class FolderManager
    {
        public List<string> FolderNames = new List<string> { "_Project", "Animation", "Scripts", "Prefabs" };
        private Vector2 folderScroll;
        private bool isEditing = false;        // Bearbeitungsmodus-Flag
        private bool essentialsLoaded = false; // Zustand speichern


        public void Draw() {
            GUILayout.Label("Folder Manager", EditorStyles.boldLabel);

            GUILayout.Space(10);

            // Oben: Edit Folder List Button
            if (GUILayout.Button(isEditing ? "Save Changes" : "Edit Folder List", GUILayout.Width(150))) {
                if (isEditing)
                    SaveFolderNames();  // Änderungen speichern
                isEditing = !isEditing; // Bearbeitungsmodus wechseln
            }

            GUILayout.Space(20);

            // Ordneranzeige oder Bearbeitungsmodus
            folderScroll = EditorGUILayout.BeginScrollView(folderScroll, GUILayout.ExpandHeight(true));

            if (isEditing) {
                // Bearbeitungsliste anzeigen
                for (int i = 0; i < FolderNames.Count; i++) {
                    EditorGUILayout.BeginHorizontal();
                    FolderNames[i] = EditorGUILayout.TextField(FolderNames[i]);
                    if (GUILayout.Button("Remove", GUILayout.Width(70))) {
                        FolderNames.RemoveAt(i);
                        i--;
                    }

                    EditorGUILayout.EndHorizontal();
                }
                
                GUILayout.Space(10);
                
                // Button zum Hinzufügen neuer Ordner
                if (GUILayout.Button("Add New Folder")) {
                    FolderNames.Add("");
                }

                EditorGUILayout.EndScrollView();

                GUILayout.Space(10);
            }
            else {
                // Standard-Anzeige der Ordner
                foreach (var folderName in FolderNames) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(folderName);
                    if (GUILayout.Button("Create", GUILayout.Width(70))) {
                        CreateFolder(folderName);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }


            GUILayout.Space(5); // Platz schaffen

            // Unten: Create All Folders Button
            if (GUILayout.Button("Create All Folders", GUILayout.Height(30))) {
                foreach (string folder in FolderNames) {
                    CreateFolder(folder);
                }
            }

            GUILayout.Space(20); // Platz schaffen
        }

        // Ordner erstellen
        private void CreateFolder(string folderName) {
            string fullPath = Path.Combine(Application.dataPath, folderName);
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"Folder created: {folderName}");
            }

            AssetDatabase.Refresh();
        }

        public void SaveFolderNames() {
            string essentialFolders = string.Join(",", FolderNames);
            EditorPrefs.SetString("EssentialFolders", essentialFolders);
            Debug.Log("Saved Essential Folders: " + essentialFolders);

            // string json = JsonUtility.ToJson(new FolderList { Folders = folderNames }, true);
            // string folderNamesFilePath = Path.Combine(Application.dataPath, "folderNames.json");
            // File.WriteAllText(folderNamesFilePath, json);
            // Debug.Log("Folder names saved to JSON file: " + folderNamesFilePath);
            // AssetDatabase.Refresh();
        }

        public void LoadFolderNames() {
            if (essentialsLoaded) return; // Doppeltes Laden verhindern

            if (EditorPrefs.HasKey("EssentialFolders")) {
                string essentialFoldersString = EditorPrefs.GetString("EssentialFolders");
                FolderNames = new List<string>(essentialFoldersString.Split(','));
                essentialsLoaded = true; // Markiere Liste als geladen
                Debug.Log("Loaded Essential Packages: " + essentialFoldersString);
            }
            else {
                FolderNames = new List<string>();
                Debug.Log("No essential packages found, initialized empty list.");
            }

            // string folderNamesFilePath = Path.Combine(Application.dataPath, "folderNames.json");
            // if (File.Exists(folderNamesFilePath)) {
            //     string json = File.ReadAllText(folderNamesFilePath);
            //     FolderList folderList = JsonUtility.FromJson<FolderList>(json);
            //     folderNames = folderList.Folders;
            //     Debug.Log("Folder names loaded from JSON file.");
            // }
            // else {
            //     Debug.LogWarning($"No folder names JSON file found. Using default list.");
            // }
        }

        [System.Serializable]
        private class FolderList
        {
            public List<string> Folders;
        }
    }
}

