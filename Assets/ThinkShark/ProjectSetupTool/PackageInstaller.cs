using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class PackageInstaller
    {
        public string PackageFolder;

        private string[] packagePaths;
        private List<string> packageNames = new();
        private List<string> packageManagerPackages = new();
        private List<string> essentialPackages = new();
        private readonly List<bool> packageSelection = new();
        private bool isEditing = false; // Bearbeitungsmodus-Flag
        private bool selectAll;         // Toggle für "Alle auswählen"

        private bool essentialsLoaded;             // Zustand speichern
        private bool packageFolderLoaded;          // Zustand speichern
        private bool packageManagerPackagesLoaded; // Zustand speichern
        
        private string newPackageName = ""; // Platzhalter für den neuen Paketnamen

        private Vector2 scroll;
        private Vector2 packageScroll;

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
                    LoadPackageManagerPackagesFromTxt();
                    LoadEssentialPackages();
                }
            }

            // GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            
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
                foreach (var package in packageManagerPackages) {
                    // Check, ob das Package in der Liste "essentialPackages" ist
                    bool isEssential = essentialPackages.Contains(package);

                    if (isEssential) {
                        Packages.InstallPackages(new[] { package });
                    }
                }
            }

            // GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            // Oben: Edit Folder List Button
            if (GUILayout.Button(isEditing ? "Save Changes" : "Edit Packages", GUILayout.Width(150))) {
                // Einfachen Moduswechsel umsetzen
                isEditing = !isEditing;

                // Nur wenn Änderungen gespeichert werden sollen (beim Verlassen des Bearbeitungsmodus)
                if (!isEditing) {
                    SavePackageManagerPackagesToTxt(); // Änderungen speichern
                    Debug.Log("Changes saved and switched to normal mode.");
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            if (!isEditing) {
                GUILayout.BeginHorizontal(); // Beginne eine horizontalen Gruppierung

                // Ordneranzeige oder Bearbeitungsmodus
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(110));

                // Bearbeitungsliste anzeigen
                for (int i = 0; i < packageManagerPackages.Count; i++) {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Check, ob das Package in der Liste "essentialPackages" ist
                    bool isEssential = essentialPackages.Contains(packageManagerPackages[i]);

                    // GUIStyle für Label definieren
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.normal.textColor = isEssential ? Color.green : Color.white;
                 
                    // Package Name mit Farbe (grün, falls essential)
                    if (GUILayout.Button(packageManagerPackages[i], labelStyle, GUILayout.ExpandWidth(true))) {
                        if (!isEssential) {
                            essentialPackages.Add(packageManagerPackages[i]);
                            SaveEssentialPackages(); // Speichere geänderte Essentials
                        }
                        else {
                            if (essentialPackages.Contains(packageManagerPackages[i])) {
                                essentialPackages.Remove(packageManagerPackages[i]);
                                SaveEssentialPackages(); // Speichere geänderte Essentials
                            }
                        }
                    }

                    if (GUILayout.Button("Install", GUILayout.Width(70))) {
                        Packages.InstallPackages(new[] { packageManagerPackages[i] });
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(10);
                
                EditorGUILayout.EndScrollView();

                GUILayout.EndHorizontal();
            }
            else {
                GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true)); // Beginne eine horizontale Gruppierung

                // Scrollbare Ansicht der Pakete
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(110));

                for (int i = 0; i < packageManagerPackages.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    


                    // Textfeld zur Bearbeitung eines Eintrags
                    packageManagerPackages[i] = EditorGUILayout.TextField(packageManagerPackages[i]);

                    // Button zum Entfernen eines Pakets
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        string packageToRemove = packageManagerPackages[i]; // Den zu entfernenden String speichern
                        packageManagerPackages.RemoveAt(i);                 // Paket aus der Liste entfernen
                        SavePackageManagerPackagesToTxt();                  // Änderungen in packages.txt speichern
                        Debug.Log($"Removed package: {packageToRemove} from packages.txt");
                        i--; // Schleifenindex zurücksetzen, um Überspringen zu vermeiden
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                GUILayout.EndHorizontal();

                GUILayout.Space(20);

                // Neues Paket hinzufügen
                GUILayout.Label("Add a new package:");
                newPackageName = EditorGUILayout.TextField(newPackageName); // TextField für neue Paketnamen

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Add New Package", GUILayout.Width(150)))
                {
                    if (!string.IsNullOrEmpty(newPackageName)) // Stelle sicher, dass der String nicht leer ist
                    {
                        packageManagerPackages.Add(newPackageName); // Paket hinzufügen
                        SavePackageManagerPackagesToTxt();          // Änderungen in die Datei speichern
                        newPackageName = "";                        // Textfeld resetten
                    }
                    else
                    {
                        Debug.LogWarning("Package name is empty. Cannot add an empty package.");
                    }
                }

                GUILayout.EndHorizontal();

                // Button zum Speichern der Liste
                if (GUILayout.Button("Save to packages.txt", GUILayout.Height(30)))
                {
                    SavePackageManagerPackagesToTxt();
                }
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            // Toggle für "Alle auswählen"
            bool previousSelectAll = selectAll; // Speichert den vorherigen Status von Select All
            selectAll = EditorGUILayout.Toggle(selectAll, GUILayout.Width(15));
            GUILayout.Label("Select All");

            if (GUILayout.Button("Save Essentials", GUILayout.Width(150))) {
                SaveEssentialPackages();
            }

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

        public void SavePackageManagerPackageNames() {
            string packageManagerPackagesString = string.Join(",", packageManagerPackages);
            EditorPrefs.SetString("PackageManagerPackages", packageManagerPackagesString);
            Debug.Log("Saved Package Manager Packages: " + packageManagerPackagesString);
        }

        public void LoadPackageManagerPackageNames() {
            if (packageManagerPackagesLoaded) return;

            if (EditorPrefs.HasKey("PackageManagerPackages")) {
                string packageManagerPackagesString = EditorPrefs.GetString("PackageManagerPackages");
                packageManagerPackages = new List<string>(packageManagerPackagesString.Split(','));
                packageManagerPackagesLoaded = true; // Markiere Liste als geladen
                Debug.Log("Loaded Essential Packages: " + packageManagerPackagesString);
            }
            else {
                packageManagerPackages = new List<string>();
                packageManagerPackages.Add("Error");
                Debug.Log("No essential packages found, initialized empty list.");
            }
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
            if (essentialsLoaded) return;

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
            if (packageFolderLoaded) return;

            if (EditorPrefs.HasKey("PackageFolderPath")) {
                PackageFolder = EditorPrefs.GetString("PackageFolderPath");
                packageFolderLoaded = true;
                Debug.Log("Loaded Package Folder Path: " + PackageFolder);
            }
            else {
                Debug.Log("No saved package folder path found. Using default.");
            }
        }
        
        public void SavePackageManagerPackagesToTxt()
        {
            string txtFilePath = Path.Combine(PackageFolder, "packages.txt");

            try
            {
                File.WriteAllLines(txtFilePath, packageManagerPackages);
                Debug.Log("Saved packages to packages.txt:");
                foreach (var package in packageManagerPackages)
                {
                    Debug.Log(package);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError("Error saving packages.txt: " + ex.Message);
            }
        }
        
        public void LoadPackageManagerPackagesFromTxt()
        {
            // Pfad zur .txt-Datei im Package Folder
            string txtFilePath = Path.Combine(PackageFolder, "Packages.txt");

            if (File.Exists(txtFilePath)) // Prüfen, ob die Datei existiert
            {
                try
                {
                    // Inhalt der Datei lesen und in die Liste speichern
                    packageManagerPackages = new List<string>(File.ReadAllLines(txtFilePath));
            
                    Debug.Log("Loaded packages from packages.txt:");
                    foreach (var package in packageManagerPackages)
                    {
                        Debug.Log(package);
                    }

                    packageManagerPackagesLoaded = true; // Markiere die Liste als geladen
                }
                catch (IOException ex)
                {
                    Debug.LogError("Error reading packages.txt: " + ex.Message);
                }
            }
            else
            {
                Debug.LogWarning("packages.txt not found in: " + PackageFolder);
                packageManagerPackages = new List<string>(); // Initialisiere leere Liste
            }
        }
    }

    static class Packages {
        static AddRequest Request;
        static Queue<string> PackagesToInstall = new();

        public static void InstallPackages(string[] packages) {
            foreach (var package in packages) {
                PackagesToInstall.Enqueue(package);
            }

            // Start the installation of the first package
            if (PackagesToInstall.Count > 0) {
                Request = Client.Add(PackagesToInstall.Dequeue());
                EditorApplication.update += Progress;
            }
        }

        static async void Progress() {
            if (Request.IsCompleted) {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;

                // If there are more packages to install, start the next one
                if (PackagesToInstall.Count > 0) {
                    // Add delay before next package install
                    await Task.Delay(1000);
                    Request = Client.Add(PackagesToInstall.Dequeue());
                    EditorApplication.update += Progress;
                }
            }
        }
    }
}