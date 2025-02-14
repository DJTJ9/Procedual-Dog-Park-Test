using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class PackageInstaller
    {
        public string PackageFolder;

        private string[] packagePaths;
        private List<string> packageNames = new();
        private List<string> essentialPackages = new();
        private readonly List<bool> packageSelection = new();
        private bool selectAll; // Toggle für "Alle auswählen"

        private bool essentialsLoaded;    // Zustand speichern
        private bool packageFolderLoaded; // Zustand speichern

        private Vector2 packageScroll;

        void OnEnable() {
            LoadPackages(PackageFolder);
        }

        public void Draw() {
            GUILayout.Label("Package Installer", EditorStyles.boldLabel);

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            GUILayout.Label(string.IsNullOrEmpty(PackageFolder) ? "No folder selected" : PackageFolder,
                EditorStyles.textField, GUILayout.Width(260));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select Package Folder", GUILayout.Width(175))) {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Package Folder", PackageFolder, "");
                if (!string.IsNullOrEmpty(selectedPath)) {
                    PackageFolder = selectedPath;
                    SavePackageFolder();
                    LoadPackages(selectedPath);
                }
            }

            // GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Install Essentials", GUILayout.Height(30))) {
                foreach (var package in packageNames) {
                    // Check, ob das Package in der Liste "essentialPackages" ist
                    bool isEssential = essentialPackages.Contains(package);
                    if (isEssential) {
                        if (packagePaths != null)
                            AssetDatabase.ImportPackage(packagePaths[packageNames.IndexOf(package)], false);
                    }
                }
            }

            // GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            // Toggle für "Alle auswählen"
            bool previousSelectAll = selectAll; // Speichert den vorherigen Status von Select All
            selectAll = EditorGUILayout.Toggle(selectAll, GUILayout.Width(15));
            GUILayout.Label("Select All");

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (selectAll != previousSelectAll) // Nur wenn sich "Select All" ändert, aktualisiere die Liste
            {
                for (int i = 0; i < packageSelection.Count; i++) {
                    packageSelection[i] = selectAll;
                }
            }

            // Scrollbereich für die Checkbox-Liste
            if (packagePaths != null && packagePaths.Length > 0) {
                GUILayout.Label("Packages Found:");

                GUILayout.Space(10);

                GUILayout.BeginVertical(); // Beginne eine vertikale Gruppierung
                GUILayout.FlexibleSpace(); // Fügt flexiblen vertikalen Leerraum über dem Button hinzu

                packageScroll = EditorGUILayout.BeginScrollView(packageScroll);

                for (int i = 0; i < packageNames.Count; i++) {
                    EditorGUILayout.BeginHorizontal();

                    // Check, ob das Package in der Liste "essentialPackages" ist
                    bool isEssential = essentialPackages.Contains(packageNames[i]);

                    // GUIStyle für Label definieren
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.normal.textColor = isEssential ? Color.green : Color.white;

                    // Checkbox für Auswahl
                    packageSelection[i] = EditorGUILayout.Toggle(packageSelection[i], GUILayout.Width(20));

                    // Package Name mit Farbe (grün, falls essential)
                    if (GUILayout.Button(packageNames[i], labelStyle, GUILayout.ExpandWidth(true))) {
                        if (!isEssential) {
                            essentialPackages.Add(packageNames[i]);
                            SaveEssentialPackages(); // Speichere geänderte Essentials
                        }
                        else {
                            if (essentialPackages.Contains(packageNames[i])) {
                                essentialPackages.Remove(packageNames[i]);
                                SaveEssentialPackages(); // Speichere geänderte Essentials
                            }
                        }
                    }

                    GUILayout.FlexibleSpace();

                    // // Add to Essentials Button
                    // if (GUILayout.Button("+", GUILayout.Width(20)))
                    // {
                    //     if (!essentialPackages.Contains(packageNames[i]))
                    //     {
                    //         essentialPackages.Add(packageNames[i]);
                    //         SaveEssentialPackages(); // Speichere geänderte Essentials
                    //     }
                    // }
                    //
                    // // Remove from Essentials Button
                    // if (GUILayout.Button("-", GUILayout.Width(20)))
                    // {
                    //     if (essentialPackages.Contains(packageNames[i]))
                    //     {
                    //         essentialPackages.Remove(packageNames[i]);
                    //         SaveEssentialPackages(); // Speichere geänderte Essentials
                    //     }
                    // }

                    // Button um Package zu installieren
                    if (GUILayout.Button("Install", GUILayout.Width(100))) {
                        AssetDatabase.ImportPackage(packagePaths[i], false);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                GUILayout.FlexibleSpace(); // Fügt flexiblen vertikalen Leerraum unter dem Button hinzu
                GUILayout.EndVertical();   // Beende die vertikale Gruppierung
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Install Selected Packages", GUILayout.Height(30))) {
                foreach (var package in packageNames) {
                    if (packageSelection[packageNames.IndexOf(package)]) {
                        if (packagePaths != null)
                            AssetDatabase.ImportPackage(packagePaths[packageNames.IndexOf(package)], false);
                    }
                }
            }

            GUILayout.Space(20);
        }

        private void LoadPackages() {
            packageNames = new List<string> { "com.unity.textmeshpro", "com.unity.cinemachine" };
        }

        public void LoadPackages(string path) {
            packagePaths = Directory.GetFiles(path, "*.unitypackage", SearchOption.AllDirectories);
            Debug.Log("Template Paths:");
            packageNames.Clear();
            packageSelection.Clear();

            foreach (string templatePath in packagePaths) {
                string scriptName = Path.GetFileNameWithoutExtension(templatePath);
                packageNames.Add(scriptName);
                packageSelection.Add(false); // Initially unselected
            }
        }

        public void SaveEssentialPackages() {
            string essentialPackagesString = string.Join(",", essentialPackages);
            EditorPrefs.SetString("EssentialPackages", essentialPackagesString);
            Debug.Log("Saved Essential Packages: " + essentialPackagesString);
        }

        public void LoadEssentialPackages() {
            if (essentialsLoaded) return; // Doppeltes Laden verhindern

            if (EditorPrefs.HasKey("EssentialPackages")) {
                string essentialPackagesString = EditorPrefs.GetString("EssentialPackages");
                essentialPackages = new List<string>(essentialPackagesString.Split(','));
                essentialsLoaded = true; // Markiere Liste als geladen
                Debug.Log("Loaded Essential Packages: " + essentialPackagesString);
            }
            else {
                essentialPackages = new List<string>();
                Debug.Log("No essential packages found, initialized empty list.");
            }
        }

        public void SavePackageFolder() {
            EditorPrefs.SetString("PackageFolderPath", PackageFolder);
            Debug.Log("Saved Package Folder Path: " + PackageFolder);
        }

        public void LoadPackageFolder() {
            if (packageFolderLoaded) return; // Doppeltes Laden verhindern
            
            if (EditorPrefs.HasKey("PackageFolderPath")) {
                PackageFolder = EditorPrefs.GetString("PackageFolderPath");
                packageFolderLoaded = true;
                Debug.Log("Loaded Package Folder Path: " + PackageFolder);
            }
            else {
                Debug.Log("No saved package folder path found. Using default.");
            }
        }
    }
}
