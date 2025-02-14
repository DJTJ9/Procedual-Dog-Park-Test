using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class ScriptImporter 
    {
        public string templateFolder = "C:/Unity/Assets/Scripts/"; // Variable für den Template-Ordner
        private string templateFolderName;
        private string targetFolder = "Assets/_Project/Scripts";           // Standardzielordner
        private readonly List<string> scriptNames = new();    // Namen der Templates
        private readonly List<bool> templateSelections = new(); // Auswahlstatus für Templates
        private Vector2 scriptScroll;                                      // Scrollansicht für die Liste der Templates
        private bool selectAll = false;                                    // "Select All"-Status für Templates

        // Zeichne die GUI
        public void Draw() {
            GUILayout.Label("Script Importer", EditorStyles.boldLabel);

            // Button zum Auswählen des Template-Ordners
            EditorGUILayout.LabelField("Template Folder Path:");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.IsNullOrEmpty(templateFolder) ? "No folder selected" : templateFolder,
                EditorStyles.textField, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Select Template Folder", GUILayout.Width(180))) {
                string initialPath = "C:/Unity/Assets/Scripts/";
                string selectedPath = EditorUtility.OpenFolderPanel("Select Template Folder", initialPath, "");
                if (!string.IsNullOrEmpty(selectedPath)) {
                    templateFolder = selectedPath;
                    // Nur den letzten Teil des Pfades (Ordnername) extrahieren
                    templateFolderName = Path.GetFileName(templateFolder);
                    LoadTemplates(templateFolder); // Lade Templates aus dem Ordner
                }
            }

            EditorGUILayout.EndHorizontal();

            // Zielordner auswählen
            GUILayout.Label("Target Folder Path:");
            EditorGUILayout.BeginHorizontal();
            targetFolder = EditorGUILayout.TextField(targetFolder);

            if (GUILayout.Button("Select Target Folder", GUILayout.Width(150))) {
                string defaultTargetPath = Path.Combine(Application.dataPath, "_Project/Scripts");
                string selectedTargetPath =
                    EditorUtility.OpenFolderPanel("Select Target Folder", defaultTargetPath, "");

                if (!string.IsNullOrEmpty(selectedTargetPath)) {
                    if (selectedTargetPath.StartsWith(Application.dataPath)) {
                        targetFolder = "Assets" + selectedTargetPath.Substring(Application.dataPath.Length);
                    }
                    else {
                        targetFolder = selectedTargetPath;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{templateFolderName}");
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (scriptNames.Count != 0) {
                GUILayout.BeginHorizontal();
                // Toggle für "Alle auswählen"
                bool previousSelectAll = selectAll; // Speichert den vorherigen Status von Select All
                selectAll = EditorGUILayout.Toggle(selectAll, GUILayout.Width(15));
                GUILayout.Label("Select All");

                if (selectAll != previousSelectAll) {
                    for (int i = 0; i < templateSelections.Count; i++) {
                        templateSelections[i] = selectAll;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // Liste der geladenen Templates anzeigen
            scriptScroll = EditorGUILayout.BeginScrollView(scriptScroll, GUILayout.ExpandHeight(true));
            for (int i = 0; i < scriptNames.Count; i++) {
                EditorGUILayout.BeginHorizontal();
                templateSelections[i] = EditorGUILayout.Toggle(templateSelections[i], GUILayout.Width(20));
                GUILayout.Label(scriptNames[i]);

                if (GUILayout.Button("Import", GUILayout.Width(80))) {
                    string templatePath = Path.Combine(templateFolder, scriptNames[i] + ".txt");
                    string targetPath = Path.Combine(targetFolder, scriptNames[i]);
                    string templateContent = File.ReadAllText(templatePath);
                    Directory.CreateDirectory(targetFolder);
                    File.WriteAllText(targetPath, templateContent);
                    Debug.Log("Script generated: " + targetPath);
                    AssetDatabase.Refresh();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // Button zum Generieren der Skripte
            if (GUILayout.Button("Generate Selected Scripts", GUILayout.Height(30))) {
                GenerateSelectedScripts();
            }

            GUILayout.Space(20);
        }

        // Lade Templates aus einem Ordner
        public void LoadTemplates(string folderPath) {
            var templatePaths = Directory.GetFiles(folderPath, "*.txt");
            scriptNames.Clear();
            templateSelections.Clear();

            foreach (var templatePath in templatePaths) {
                string scriptName = Path.GetFileNameWithoutExtension(templatePath);
                scriptNames.Add(scriptName);
                templateSelections.Add(false); // Alle Templates initial auf "nicht ausgewählt"
            }

            Debug.Log($"Loaded {scriptNames.Count} templates from: {folderPath}");
        }

        // Generiere die ausgewählten Skripte
        private void GenerateSelectedScripts() {
            if (string.IsNullOrEmpty(templateFolder)) {
                Debug.LogError("No template folder selected. Please select a folder containing script templates.");
                return;
            }

            for (int i = 0; i < scriptNames.Count; i++) {
                if (templateSelections[i]) {
                    string templatePath = Path.Combine(templateFolder, scriptNames[i] + ".txt");
                    string targetPath = Path.Combine(targetFolder, scriptNames[i]);

                    if (!File.Exists(templatePath)) {
                        Debug.LogError("Template file not found: " + templatePath);
                        continue;
                    }

                    string templateContent = File.ReadAllText(templatePath);
                    Directory.CreateDirectory(targetFolder);
                    File.WriteAllText(targetPath, templateContent);

                    Debug.Log("Script generated: " + targetPath);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Generated selected scripts in: {targetFolder}");
        }
    }
}