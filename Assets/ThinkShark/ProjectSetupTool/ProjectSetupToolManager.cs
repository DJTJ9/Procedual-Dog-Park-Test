using UnityEditor;
using UnityEngine;

namespace ThinkShark.ProjectSetupTool
{
    public class ProjectSetupToolManager : EditorWindow
    {
        private enum Tab
        {
            FolderManager,
            PackageInstaller,
            ScriptImporter
        }

        private Tab selectedTab = Tab.FolderManager; // Folder Manager als erster Tab

        private readonly FolderManager folderManager = new();
        private readonly PackageInstaller packageInstaller = new();
        private readonly ScriptImporter scriptImporter = new();

        [MenuItem("Tools/Project Setup Tool")]
        public static void ShowWindow() {
            ProjectSetupToolManager window = GetWindow<ProjectSetupToolManager>("Project Setup Tool");
            
            // Fenstergröße festlegen
            window.minSize = new Vector2(500, 500); // Mindestgröße
        }

        private void OnEnable() {
            // Lade Ordnernamen, wenn das Fenster geöffnet wird
            folderManager.LoadFolderNames();
            packageInstaller.LoadPackageFolder();
            packageInstaller.LoadEssentialPackages(); // Essentials laden
            packageInstaller.LoadPackages(packageInstaller.PackageFolder);
            scriptImporter.LoadTemplates(scriptImporter.templateFolder);
        }

        private void OnDisable() {
            // Speichere Ordnernamen, wenn das Fenster geschlossen wird
            folderManager.SaveFolderNames();
            packageInstaller.SavePackageFolder();
            packageInstaller.SaveEssentialPackages(); // Essentials speichern
        }

        private void OnGUI() {
            // Tab-Interface
            selectedTab = (Tab)GUILayout.Toolbar((int)selectedTab, new[] {
                "Folder Manager", "Package Installer", "Script Importer"
            });

            // Inhalte basierend auf dem aktiven Tab
            switch (selectedTab) {
                case Tab.FolderManager:
                    folderManager.Draw();
                    break;
                case Tab.PackageInstaller:
                    packageInstaller.Draw();
                    break;
                case Tab.ScriptImporter:
                    scriptImporter.Draw();
                    break;
            }
        }

        // ------------------------ Folder Manager ------------------------

        // private class FolderManager
        // {
        //     public List<string> folderNames = new List<string> { "_Project", "Animation", "Scripts", "Prefabs" };
        //     private Vector2 folderScroll;
        //     private bool isEditing = false;        // Bearbeitungsmodus-Flag
        //     private bool essentialsLoaded = false; // Zustand speichern
        //
        //
        //     public void Draw() {
        //         GUILayout.Label("Folder Manager", EditorStyles.boldLabel);
        //
        //         GUILayout.Space(10);
        //
        //         // Oben: Edit Folder List Button
        //         if (GUILayout.Button(isEditing ? "Save Changes" : "Edit Folder List", GUILayout.Width(150))) {
        //             if (isEditing)
        //                 SaveFolderNames();  // Änderungen speichern
        //             isEditing = !isEditing; // Bearbeitungsmodus wechseln
        //         }
        //
        //         GUILayout.Space(20);
        //
        //         // Ordneranzeige oder Bearbeitungsmodus
        //         folderScroll = EditorGUILayout.BeginScrollView(folderScroll, GUILayout.ExpandHeight(true));
        //
        //         if (isEditing) {
        //             // Bearbeitungsliste anzeigen
        //             for (int i = 0; i < folderNames.Count; i++) {
        //                 EditorGUILayout.BeginHorizontal();
        //                 folderNames[i] = EditorGUILayout.TextField(folderNames[i]);
        //                 if (GUILayout.Button("Remove", GUILayout.Width(70))) {
        //                     folderNames.RemoveAt(i);
        //                     i--;
        //                 }
        //
        //                 EditorGUILayout.EndHorizontal();
        //             }
        //
        //             EditorGUILayout.EndScrollView();
        //
        //             GUILayout.Space(10);
        //
        //             // Button zum Hinzufügen neuer Ordner
        //             if (GUILayout.Button("Add New Folder")) {
        //                 folderNames.Add("");
        //             }
        //         }
        //         else {
        //             // Standard-Anzeige der Ordner
        //             foreach (var folderName in folderNames) {
        //                 EditorGUILayout.BeginHorizontal();
        //                 GUILayout.Label(folderName);
        //                 if (GUILayout.Button("Create", GUILayout.Width(70))) {
        //                     CreateFolder(folderName);
        //                 }
        //
        //                 EditorGUILayout.EndHorizontal();
        //             }
        //
        //             GUILayout.FlexibleSpace();
        //             EditorGUILayout.EndScrollView();
        //         }
        //
        //
        //         GUILayout.Space(20); // Platz schaffen
        //
        //         // Unten: Create All Folders Button
        //         if (GUILayout.Button("Create All Folders", GUILayout.Height(30))) {
        //             foreach (string folder in folderNames) {
        //                 CreateFolder(folder);
        //             }
        //         }
        //
        //         GUILayout.Space(20); // Platz schaffen
        //     }
        //
        //     // Ordner erstellen
        //     private void CreateFolder(string folderName) {
        //         string fullPath = Path.Combine(Application.dataPath, folderName);
        //         if (!Directory.Exists(fullPath)) {
        //             Directory.CreateDirectory(fullPath);
        //             Debug.Log($"Folder created: {folderName}");
        //         }
        //
        //         AssetDatabase.Refresh();
        //     }
        //
        //     public void SaveFolderNames() {
        //         string essentialFolders = string.Join(",", folderNames);
        //         EditorPrefs.SetString("EssentialFolders", essentialFolders);
        //         Debug.Log("Saved Essential Folders: " + essentialFolders);
        //
        //         // string json = JsonUtility.ToJson(new FolderList { Folders = folderNames }, true);
        //         // string folderNamesFilePath = Path.Combine(Application.dataPath, "folderNames.json");
        //         // File.WriteAllText(folderNamesFilePath, json);
        //         // Debug.Log("Folder names saved to JSON file: " + folderNamesFilePath);
        //         // AssetDatabase.Refresh();
        //     }
        //
        //     public void LoadFolderNames() {
        //         if (essentialsLoaded) return; // Doppeltes Laden verhindern
        //
        //         if (EditorPrefs.HasKey("EssentialFolders")) {
        //             string essentialFoldersString = EditorPrefs.GetString("EssentialFolders");
        //             folderNames = new List<string>(essentialFoldersString.Split(','));
        //             essentialsLoaded = true; // Markiere Liste als geladen
        //             Debug.Log("Loaded Essential Packages: " + essentialFoldersString);
        //         }
        //         else {
        //             folderNames = new List<string>();
        //             Debug.Log("No essential packages found, initialized empty list.");
        //         }
        //
        //         // string folderNamesFilePath = Path.Combine(Application.dataPath, "folderNames.json");
        //         // if (File.Exists(folderNamesFilePath)) {
        //         //     string json = File.ReadAllText(folderNamesFilePath);
        //         //     FolderList folderList = JsonUtility.FromJson<FolderList>(json);
        //         //     folderNames = folderList.Folders;
        //         //     Debug.Log("Folder names loaded from JSON file.");
        //         // }
        //         // else {
        //         //     Debug.LogWarning($"No folder names JSON file found. Using default list.");
        //         // }
        //     }
        //
        //     [System.Serializable]
        //     private class FolderList
        //     {
        //         public List<string> Folders;
        //     }
        // }

        // ------------------------ Package Importer ------------------------

        // private class PackageImporter
        // {
        //     public string packageFolder;
        //
        //     private string[] packagePaths;
        //     private List<string> packageNames = new List<string>();
        //     private List<string> essentialPackages = new List<string>();
        //     private List<bool> packageSelection = new List<bool>();
        //     private bool selectAll = false; // Toggle für "Alle auswählen"
        //
        //     private bool essentialsLoaded = false;    // Zustand speichern
        //     private bool packageFolderLoaded = false; // Zustand speichern
        //     private bool isEditing = false;           // Bearbeitungsmodus-Flag
        //
        //     private Vector2 packageScroll;
        //
        //     void OnEnable() {
        //         LoadPackages(packageFolder);
        //     }
        //
        //     public void Draw() {
        //         GUILayout.Label("Package Importer", EditorStyles.boldLabel);
        //
        //         GUILayout.Space(20);
        //
        //         GUILayout.BeginHorizontal();
        //
        //         GUILayout.Label(string.IsNullOrEmpty(packageFolder) ? "No folder selected" : packageFolder,
        //             EditorStyles.textField, GUILayout.Width(260));
        //         GUILayout.FlexibleSpace();
        //         if (GUILayout.Button("Select Package Folder", GUILayout.Width(175))) {
        //             string selectedPath = EditorUtility.OpenFolderPanel("Select Package Folder", packageFolder, "");
        //             if (!string.IsNullOrEmpty(selectedPath)) {
        //                 packageFolder = selectedPath;
        //                 SavePackageFolder();
        //                 LoadPackages(selectedPath);
        //             }
        //         }
        //
        //         // GUILayout.FlexibleSpace();
        //         GUILayout.EndHorizontal();
        //
        //         GUILayout.Space(20);
        //
        //         GUILayout.BeginHorizontal();
        //
        //         if (GUILayout.Button("Install Essentials", GUILayout.Height(30))) {
        //             foreach (var package in packageNames) {
        //                 // Check, ob das Package in der Liste "essentialPackages" ist
        //                 bool isEssential = essentialPackages.Contains(package);
        //                 if (isEssential) {
        //                     if (packagePaths != null)
        //                         AssetDatabase.ImportPackage(packagePaths[packageNames.IndexOf(package)], false);
        //                 }
        //             }
        //         }
        //
        //         // GUILayout.FlexibleSpace();
        //         GUILayout.EndHorizontal();
        //
        //         GUILayout.Space(20);
        //
        //         GUILayout.BeginHorizontal();
        //         // Toggle für "Alle auswählen"
        //         bool previousSelectAll = selectAll; // Speichert den vorherigen Status von Select All
        //         selectAll = EditorGUILayout.Toggle(selectAll, GUILayout.Width(15));
        //         GUILayout.Label("Select All");
        //
        //         GUILayout.EndHorizontal();
        //
        //         GUILayout.Space(10);
        //
        //         if (selectAll != previousSelectAll) // Nur wenn sich "Select All" ändert, aktualisiere die Liste
        //         {
        //             for (int i = 0; i < packageSelection.Count; i++) {
        //                 packageSelection[i] = selectAll;
        //             }
        //         }
        //
        //         // Scrollbereich für die Checkbox-Liste
        //         if (packagePaths != null && packagePaths.Length > 0) {
        //             GUILayout.Label("Packages Found:");
        //
        //             GUILayout.Space(10);
        //
        //             GUILayout.BeginVertical(); // Beginne eine vertikale Gruppierung
        //             GUILayout.FlexibleSpace(); // Fügt flexiblen vertikalen Leerraum über dem Button hinzu
        //
        //             packageScroll = EditorGUILayout.BeginScrollView(packageScroll);
        //
        //             for (int i = 0; i < packageNames.Count; i++) {
        //                 EditorGUILayout.BeginHorizontal();
        //
        //                 // Check, ob das Package in der Liste "essentialPackages" ist
        //                 bool isEssential = essentialPackages.Contains(packageNames[i]);
        //
        //                 // GUIStyle für Label definieren
        //                 GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        //                 labelStyle.normal.textColor = isEssential ? Color.green : Color.white;
        //
        //                 // Checkbox für Auswahl
        //                 packageSelection[i] = EditorGUILayout.Toggle(packageSelection[i], GUILayout.Width(20));
        //
        //                 // Package Name mit Farbe (grün, falls essential)
        //                 if (GUILayout.Button(packageNames[i], labelStyle, GUILayout.ExpandWidth(true))) {
        //                     if (!isEssential) {
        //                         essentialPackages.Add(packageNames[i]);
        //                         SaveEssentialPackages(); // Speichere geänderte Essentials
        //                     }
        //                     else {
        //                         if (essentialPackages.Contains(packageNames[i])) {
        //                             essentialPackages.Remove(packageNames[i]);
        //                             SaveEssentialPackages(); // Speichere geänderte Essentials
        //                         }
        //                     }
        //                 }
        //
        //                 GUILayout.FlexibleSpace();
        //
        //                 // // Add to Essentials Button
        //                 // if (GUILayout.Button("+", GUILayout.Width(20)))
        //                 // {
        //                 //     if (!essentialPackages.Contains(packageNames[i]))
        //                 //     {
        //                 //         essentialPackages.Add(packageNames[i]);
        //                 //         SaveEssentialPackages(); // Speichere geänderte Essentials
        //                 //     }
        //                 // }
        //                 //
        //                 // // Remove from Essentials Button
        //                 // if (GUILayout.Button("-", GUILayout.Width(20)))
        //                 // {
        //                 //     if (essentialPackages.Contains(packageNames[i]))
        //                 //     {
        //                 //         essentialPackages.Remove(packageNames[i]);
        //                 //         SaveEssentialPackages(); // Speichere geänderte Essentials
        //                 //     }
        //                 // }
        //
        //                 // Button um Package zu installieren
        //                 if (GUILayout.Button("Install", GUILayout.Width(100))) {
        //                     AssetDatabase.ImportPackage(packagePaths[i], false);
        //                 }
        //
        //                 EditorGUILayout.EndHorizontal();
        //             }
        //
        //             EditorGUILayout.EndScrollView();
        //             GUILayout.FlexibleSpace(); // Fügt flexiblen vertikalen Leerraum unter dem Button hinzu
        //             GUILayout.EndVertical();   // Beende die vertikale Gruppierung
        //         }
        //
        //         GUILayout.Space(10);
        //
        //         if (GUILayout.Button("Install Selected Packages", GUILayout.Height(30))) {
        //             foreach (var package in packageNames) {
        //                 if (packageSelection[packageNames.IndexOf(package)]) {
        //                     if (packagePaths != null)
        //                         AssetDatabase.ImportPackage(packagePaths[packageNames.IndexOf(package)], false);
        //                 }
        //             }
        //         }
        //
        //         GUILayout.Space(20);
        //     }
        //
        //     private void LoadPackages() {
        //         packageNames = new List<string> { "com.unity.textmeshpro", "com.unity.cinemachine" };
        //     }
        //
        //     public void LoadPackages(string path) {
        //         packagePaths = Directory.GetFiles(path, "*.unitypackage", SearchOption.AllDirectories);
        //         Debug.Log("Template Paths:");
        //         packageNames.Clear();
        //         packageSelection.Clear();
        //
        //         foreach (string templatePath in packagePaths) {
        //             string scriptName = Path.GetFileNameWithoutExtension(templatePath);
        //             packageNames.Add(scriptName);
        //             packageSelection.Add(false); // Initially unselected
        //         }
        //     }
        //
        //     public void SaveEssentialPackages() {
        //         string essentialPackagesString = string.Join(",", essentialPackages);
        //         EditorPrefs.SetString("EssentialPackages", essentialPackagesString);
        //         Debug.Log("Saved Essential Packages: " + essentialPackagesString);
        //     }
        //
        //     public void LoadEssentialPackages() {
        //         if (essentialsLoaded) return; // Doppeltes Laden verhindern
        //
        //         if (EditorPrefs.HasKey("EssentialPackages")) {
        //             string essentialPackagesString = EditorPrefs.GetString("EssentialPackages");
        //             essentialPackages = new List<string>(essentialPackagesString.Split(','));
        //             essentialsLoaded = true; // Markiere Liste als geladen
        //             Debug.Log("Loaded Essential Packages: " + essentialPackagesString);
        //         }
        //         else {
        //             essentialPackages = new List<string>();
        //             Debug.Log("No essential packages found, initialized empty list.");
        //         }
        //     }
        //
        //     public void SavePackageFolder() {
        //         EditorPrefs.SetString("PackageFolderPath", packageFolder);
        //         Debug.Log("Saved Package Folder Path: " + packageFolder);
        //     }
        //
        //     public void LoadPackageFolder() {
        //         if (EditorPrefs.HasKey("PackageFolderPath")) {
        //             packageFolder = EditorPrefs.GetString("PackageFolderPath");
        //             packageFolderLoaded = true;
        //             Debug.Log("Loaded Package Folder Path: " + packageFolder);
        //         }
        //         else {
        //             Debug.Log("No saved package folder path found. Using default.");
        //         }
        //     }
        // }

        // ------------------------ Script Importer ------------------------

        // private class ScriptImporter
        // {
        //     public string templateFolder = "C:/Unity/Assets/Scripts/"; // Variable für den Template-Ordner
        //     private string templateFolderName;
        //     private string targetFolder = "Assets/_Project/Scripts";  // Standardzielordner
        //     private List<string> scriptNames = new List<string>();    // Namen der Templates
        //     private List<bool> templateSelections = new List<bool>(); // Auswahlstatus für Templates
        //     private Vector2 scriptScroll;                             // Scrollansicht für die Liste der Templates
        //     private bool selectAll = false;                           // "Select All"-Status für Templates
        //
        //     // Zeichne die GUI
        //     public void Draw() {
        //         GUILayout.Label("Script Importer", EditorStyles.boldLabel);
        //
        //         // Button zum Auswählen des Template-Ordners
        //         EditorGUILayout.LabelField("Template Folder Path:");
        //         EditorGUILayout.BeginHorizontal();
        //         GUILayout.Label(string.IsNullOrEmpty(templateFolder) ? "No folder selected" : templateFolder,
        //             EditorStyles.textField, GUILayout.ExpandWidth(true));
        //         if (GUILayout.Button("Select Template Folder", GUILayout.Width(180))) {
        //             string initialPath = "C:/Unity/Assets/Scripts/";
        //             string selectedPath = EditorUtility.OpenFolderPanel("Select Template Folder", initialPath, "");
        //             if (!string.IsNullOrEmpty(selectedPath)) {
        //                 templateFolder = selectedPath;
        //                 // Nur den letzten Teil des Pfades (Ordnername) extrahieren
        //                 templateFolderName = Path.GetFileName(templateFolder);
        //                 LoadTemplates(templateFolder); // Lade Templates aus dem Ordner
        //             }
        //         }
        //
        //         EditorGUILayout.EndHorizontal();
        //
        //         // Zielordner auswählen
        //         GUILayout.Label("Target Folder Path:");
        //         EditorGUILayout.BeginHorizontal();
        //         targetFolder = EditorGUILayout.TextField(targetFolder);
        //
        //         if (GUILayout.Button("Select Target Folder", GUILayout.Width(150))) {
        //             string defaultTargetPath = Path.Combine(Application.dataPath, "_Project/Scripts");
        //             string selectedTargetPath =
        //                 EditorUtility.OpenFolderPanel("Select Target Folder", defaultTargetPath, "");
        //
        //             if (!string.IsNullOrEmpty(selectedTargetPath)) {
        //                 if (selectedTargetPath.StartsWith(Application.dataPath)) {
        //                     targetFolder = "Assets" + selectedTargetPath.Substring(Application.dataPath.Length);
        //                 }
        //                 else {
        //                     targetFolder = selectedTargetPath;
        //                 }
        //             }
        //         }
        //
        //         EditorGUILayout.EndHorizontal();
        //
        //         GUILayout.Space(20);
        //
        //         EditorGUILayout.BeginHorizontal();
        //         GUILayout.Label($"{templateFolderName}");
        //         EditorGUILayout.EndHorizontal();
        //
        //         GUILayout.Space(10);
        //
        //         if (scriptNames.Count != 0) {
        //             GUILayout.BeginHorizontal();
        //             // Toggle für "Alle auswählen"
        //             bool previousSelectAll = selectAll; // Speichert den vorherigen Status von Select All
        //             selectAll = EditorGUILayout.Toggle(selectAll, GUILayout.Width(15));
        //             GUILayout.Label("Select All");
        //
        //             if (selectAll != previousSelectAll) {
        //                 for (int i = 0; i < templateSelections.Count; i++) {
        //                     templateSelections[i] = selectAll;
        //                 }
        //             }
        //
        //             EditorGUILayout.EndHorizontal();
        //         }
        //
        //         GUILayout.Space(10);
        //
        //         // Liste der geladenen Templates anzeigen
        //         scriptScroll = EditorGUILayout.BeginScrollView(scriptScroll, GUILayout.ExpandHeight(true));
        //         for (int i = 0; i < scriptNames.Count; i++) {
        //             EditorGUILayout.BeginHorizontal();
        //             templateSelections[i] = EditorGUILayout.Toggle(templateSelections[i], GUILayout.Width(20));
        //             GUILayout.Label(scriptNames[i]);
        //
        //             if (GUILayout.Button("Import", GUILayout.Width(80))) {
        //                 string templatePath = Path.Combine(templateFolder, scriptNames[i] + ".txt");
        //                 string targetPath = Path.Combine(targetFolder, scriptNames[i]);
        //                 string templateContent = File.ReadAllText(templatePath);
        //                 Directory.CreateDirectory(targetFolder);
        //                 File.WriteAllText(targetPath, templateContent);
        //                 Debug.Log("Script generated: " + targetPath);
        //                 AssetDatabase.Refresh();
        //             }
        //
        //             EditorGUILayout.EndHorizontal();
        //         }
        //
        //         EditorGUILayout.EndScrollView();
        //
        //         // Button zum Generieren der Skripte
        //         if (GUILayout.Button("Generate Selected Scripts", GUILayout.Height(30))) {
        //             GenerateSelectedScripts();
        //         }
        //
        //         GUILayout.Space(20);
        //     }
        //
        //     // Lade Templates aus einem Ordner
        //     public void LoadTemplates(string folderPath) {
        //         var templatePaths = Directory.GetFiles(folderPath, "*.txt");
        //         scriptNames.Clear();
        //         templateSelections.Clear();
        //
        //         foreach (var templatePath in templatePaths) {
        //             string scriptName = Path.GetFileNameWithoutExtension(templatePath);
        //             scriptNames.Add(scriptName);
        //             templateSelections.Add(false); // Alle Templates initial auf "nicht ausgewählt"
        //         }
        //
        //         Debug.Log($"Loaded {scriptNames.Count} templates from: {folderPath}");
        //     }
        //
        //     // Generiere die ausgewählten Skripte
        //     private void GenerateSelectedScripts() {
        //         if (string.IsNullOrEmpty(templateFolder)) {
        //             Debug.LogError("No template folder selected. Please select a folder containing script templates.");
        //             return;
        //         }
        //
        //         for (int i = 0; i < scriptNames.Count; i++) {
        //             if (templateSelections[i]) {
        //                 string templatePath = Path.Combine(templateFolder, scriptNames[i] + ".txt");
        //                 string targetPath = Path.Combine(targetFolder, scriptNames[i]);
        //
        //                 if (!File.Exists(templatePath)) {
        //                     Debug.LogError("Template file not found: " + templatePath);
        //                     continue;
        //                 }
        //
        //                 string templateContent = File.ReadAllText(templatePath);
        //                 Directory.CreateDirectory(targetFolder);
        //                 File.WriteAllText(targetPath, templateContent);
        //
        //                 Debug.Log("Script generated: " + targetPath);
        //             }
        //         }
        //
        //         AssetDatabase.Refresh();
        //         Debug.Log($"Generated selected scripts in: {targetFolder}");
        //     }
        // }
    }
}