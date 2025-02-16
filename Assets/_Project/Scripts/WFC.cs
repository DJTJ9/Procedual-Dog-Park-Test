using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class WFC : MonoBehaviour
{
    // //TODO: 
    // /*Kleine Seeflecken nach dem Algorhitmus miteinanderverbinden (maybe)
    //  * 
    //  */
    //
    // public int Dimensions;
    // public TileWeightBundle[] TileObjects;
    // public List<Cell> GridComponents;
    // public Cell CellObj;
    //
    // public DrunkardsWalk DrunkardsWalk;
    //
    // private int iterations;
    //
    // void Awake() {
    //     DrunkardsWalk = GetComponent<DrunkardsWalk>();
    //     GridComponents = new List<Cell>();
    //     InitializeGrid();
    //     DrunkardsWalk.GenerateDrunkardsWalk(Dimensions, GridComponents);
    //     _ = CheckEntropyAsync(); // Unity wartet nicht direkt, aber der Code läuft asynchron
    // }
    //
    // void InitializeGrid()
    // {
    //     int totalCells = Dimensions * Dimensions;
    //     NativeArray<Vector3> cellPositions = new NativeArray<Vector3>(totalCells, Allocator.TempJob);
    //     NativeArray<int> cellData = new NativeArray<int>(totalCells, Allocator.TempJob);
    //
    //     // Job erstellen
    //     GridInitializationJob job = new GridInitializationJob
    //     {
    //         Dimensions = Dimensions,
    //         CellPositions = cellPositions,
    //         CellData = cellData
    //     };
    //
    //     // Job parallel ausführen
    //     JobHandle jobHandle = job.Schedule(totalCells, 64); // Chunks von 64
    //     jobHandle.Complete();
    //
    //     // Unity-spezifisches Instantiate im Hauptthread
    //     for (int i = 0; i < totalCells; i++)
    //     {
    //         Cell newCell = Instantiate(CellObj, cellPositions[i], Quaternion.identity);
    //         newCell.CreateCell(false, TileObjects);
    //         GridComponents.Add(newCell);
    //     }
    //
    //     // Speicher freigeben
    //     cellPositions.Dispose();
    //     cellData.Dispose();
    // }
    //
    //
    // private async Task<List<Cell>> GetSortedEntropyCellsAsync(List<Cell> gridComponents)
    // {
    //     return await Task.Run(() =>
    //     {
    //         // Temporäre Liste aller nicht-kollabierten Zellen erstellen
    //         List<Cell> tempGrid = gridComponents.Where(c => !c.Collapsed).ToList();
    //
    //         // Nach der Anzahl der TileOptions sortieren
    //         tempGrid.Sort((a, b) => a.TileOptions.Length - b.TileOptions.Length);
    //
    //         return tempGrid;
    //     });
    // }
    //
    // private async Task CheckEntropyAsync()
    // {
    //     List<Cell> tempGrid = await GetSortedEntropyCellsAsync(GridComponents);
    //
    //     if (tempGrid.Count == 0) // Wenn keine gültigen (unkollabierten) Zellen übrig sind
    //     {
    //         Debug.Log("Alle Zellen sind kollabiert.");
    //         return;
    //     }
    //
    //     Debug.Log($"Erste unkollabierte Zelle hat {tempGrid[0].TileOptions.Length} Optionen.");
    //
    //     int arrLength = tempGrid[0].TileOptions.Length;
    //
    //     int stopIndex = tempGrid.FindIndex(cell => cell.TileOptions.Length > arrLength);
    //     if (stopIndex > 0)
    //     {
    //         tempGrid = tempGrid.GetRange(0, stopIndex);
    //     }
    //
    //     Cell cellToCollapse = tempGrid[UnityEngine.Random.Range(0, tempGrid.Count)];
    //     await CollapseCell(cellToCollapse);
    //
    //     // Hier explizit sicherstellen, dass die Methode fortgeführt wird
    //     await CheckEntropyAsync();
    // }
    //
    // async Task CollapseCell(Cell cellToCollapse)
    // {
    //     if (cellToCollapse.Collapsed)
    //         return;
    //
    //     // Wähle eine zufällige Kachel basierend auf der Gewichtung
    //     TileWeightBundle selectedTile = SelectTileBasedOnWeight(cellToCollapse.TileOptions);
    //
    //     // Markiere die Zelle als kollabiert und setze das Tile
    //     cellToCollapse.TileOptions = new[] { selectedTile };
    //     cellToCollapse.Collapsed = true;
    //
    //     // Erzeuge das Tile im Spiel (Sichtbar)
    //     Instantiate(selectedTile.Tile, cellToCollapse.transform.position, selectedTile.Tile.transform.rotation);
    //
    //     // Aktualisiere den Zustand der Nachbarn und die Entropie
    //     await UpdateGenerationAsync();
    // }
    //
    // TileWeightBundle SelectTileBasedOnWeight(TileWeightBundle[] options)
    // {
    //     int totalWeight = 0;
    //     Debug.LogWarning($"{options.Length} tile options selected");
    //     // Berechne das Gesamtgewicht
    //     foreach (var option in options) {
    //         totalWeight += option.Weight;
    //     }
    //
    //     // Zufällige Gewichtsauswahl
    //     int randomWeight = UnityEngine.Random.Range(0, totalWeight);
    //     int currentWeight = 0;
    //
    //     foreach (var option in options) {
    //         currentWeight += option.Weight;
    //         if (randomWeight <= currentWeight) {
    //             return option; // Passendes Tile gefunden
    //         }
    //     }
    //
    //     if (totalWeight == 0)
    //         throw new Exception("Total weight is 0. Check weight distribution.");
    //
    //     throw new Exception("Failed to select a tile. Check weight distribution.");
    // }
    //
    // async Task UpdateGenerationAsync() {
    //     List<Cell> newGenerationCell = new List<Cell>(GridComponents);
    //
    //     for (int z = 0; z < Dimensions; z++) {
    //         for (int x = 0; x < Dimensions; x++) {
    //             var index = x + z * Dimensions;
    //             if (GridComponents[index].Collapsed) {
    //                 newGenerationCell[index] = GridComponents[index];
    //             }
    //             else {
    //                 List<TileWeightBundle> options = new List<TileWeightBundle>();
    //                 foreach (TileWeightBundle t in TileObjects) {
    //                     options.Add(t);
    //                 }
    //
    //                 //update above
    //                 if (z > 0) {
    //                     Cell up = GridComponents[x + (z - 1) * Dimensions];
    //                     List<TileWeightBundle> validOptions = new List<TileWeightBundle>();
    //
    //                     if (up.IsPath) {
    //                         validOptions.AddRange(up.TileOptions.Where(tile => tile.Tile.CompareTag("PathNeighbour")));
    //                         Debug.Log($"Up-Nachbar ist ein Pfad, Optionen: {validOptions.Count}");
    //                     }
    //                     else {
    //                         foreach (TileWeightBundle possibleOptions in up.TileOptions) {
    //                             var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
    //                             if (valOption < 0)
    //                             {
    //                                 Debug.LogWarning("Kein valider Nachbar gefunden. Optionen überspringen.");
    //                                 continue;
    //                             }
    //                             var valid = TileObjects[valOption].Tile.UpNeighbours;
    //
    //                             validOptions = validOptions.Concat(valid).ToList();
    //                         }
    //                     }
    //
    //                     await CheckValidityAsync(options, validOptions);
    //                 }
    //
    //                 //look down
    //                 if (z < Dimensions - 1) {
    //                     Cell down = GridComponents[x + (z + 1) * Dimensions];
    //                     List<TileWeightBundle> validOptions = new List<TileWeightBundle>();
    //                     
    //                     if (down.IsPath) {
    //                         validOptions.AddRange(down.TileOptions.Where(tile => tile.Tile.CompareTag("PathNeighbour")));
    //                         Debug.Log($"Up-Nachbar ist ein Pfad, Optionen: {validOptions.Count}");
    //                     }
    //                     else {
    //                         foreach (TileWeightBundle possibleOptions in down.TileOptions) {
    //                             var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
    //                             if (valOption < 0)
    //                             {
    //                                 Debug.LogWarning("Kein valider Nachbar gefunden. Optionen überspringen.");
    //                                 continue;
    //                             }
    //                             var valid = TileObjects[valOption].Tile.DownNeighbours;
    //
    //                             validOptions = validOptions.Concat(valid).ToList();
    //                         }
    //                     }
    //
    //                     await CheckValidityAsync(options, validOptions);
    //                 }
    //
    //                 //look left
    //                 if (x > 0) {
    //                     Cell left = GridComponents[x - 1 + z * Dimensions];
    //                     List<TileWeightBundle> validOptions = new List<TileWeightBundle>();
    //
    //                     if (left.IsPath) {
    //                         validOptions.AddRange(left.TileOptions.Where(tile => tile.Tile.CompareTag("PathNeighbour")));
    //                         Debug.Log($"Up-Nachbar ist ein Pfad, Optionen: {validOptions.Count}");
    //                     }
    //                     else {
    //                         foreach (TileWeightBundle possibleOptions in left.TileOptions) {
    //                             var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
    //                             if (valOption < 0)
    //                             {
    //                                 Debug.LogWarning("Kein valider Nachbar gefunden. Optionen überspringen.");
    //                                 continue;
    //                             }
    //                             var valid = TileObjects[valOption].Tile.RightNeighbours;
    //
    //                             validOptions = validOptions.Concat(valid).ToList();
    //                         }
    //                     }
    //
    //                     await CheckValidityAsync(options, validOptions);
    //                 }
    //                 
    //                 //update right
    //                 if (x < Dimensions - 1) {
    //                     Cell right = GridComponents[x + 1 + z * Dimensions];
    //                     List<TileWeightBundle> validOptions = new List<TileWeightBundle>();
    //                     
    //                     if (right.IsPath) {
    //                         validOptions.AddRange(right.TileOptions.Where(tile => tile.Tile.CompareTag("PathNeighbour")));
    //                         Debug.Log($"Up-Nachbar ist ein Pfad, Optionen: {validOptions.Count}");
    //                     }
    //                     else {
    //                         foreach (TileWeightBundle possibleOptions in right.TileOptions) {
    //                             var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
    //                             if (valOption < 0)
    //                             {
    //                                 Debug.LogWarning("Kein valider Nachbar gefunden. Optionen überspringen.");
    //                                 continue;
    //                             }
    //                             var valid = TileObjects[valOption].Tile.LeftNeighbours;
    //
    //                             validOptions = validOptions.Concat(valid).ToList();
    //                         }
    //                     }
    //
    //                     await CheckValidityAsync(options, validOptions);
    //                 }
    //
    //                 TileWeightBundle[] newTileList = new TileWeightBundle[options.Count];
    //
    //                 for (int i = 0; i < options.Count; i++) {
    //                     newTileList[i] = options[i];
    //                 }
    //
    //                 newGenerationCell[index].RecreateCell(newTileList);
    //             }
    //         }
    //     }
    //
    //     GridComponents = newGenerationCell;
    //     iterations++;
    //
    //     if (iterations < Dimensions * Dimensions)
    //     {
    //         Debug.Log($"Iteration {iterations} abgeschlossen. Starte nächste Entropie-Prüfung.");
    //         await CheckEntropyAsync();
    //     }
    //     else
    //     {
    //         Debug.Log("Generierung abgeschlossen!");
    //     }
    // }
    //
    // async Task<List<TileWeightBundle>> ValidateOptionsAsync(List<TileWeightBundle> options, List<TileWeightBundle> validOptions)
    // {
    //     return await Task.Run(() =>
    //     {
    //         // Erstelle ein Dictionary mit gültigen Optionen
    //         var validOptionMap = validOptions
    //             .Where(bundle => bundle.Tile != null)
    //             .GroupBy(bundle => bundle.Tile)
    //             .ToDictionary(group => group.Key, group => group.First());
    //
    //         // Optionen basierend auf Gültigkeit filtern und Gewichte aktualisieren
    //         options = options.Where(option =>
    //             validOptionMap.ContainsKey(option.Tile)).ToList();
    //
    //         for (int i = 0; i < options.Count; i++)
    //         {
    //             if (validOptionMap.TryGetValue(options[i].Tile, out var match))
    //             {
    //                 options[i] = new TileWeightBundle
    //                 {
    //                     Tile = options[i].Tile,
    //                     Weight = match.Weight
    //                 };
    //             }
    //         }
    //
    //         return options;
    //     });
    // }
    //
    // private async Task CheckValidityAsync(List<TileWeightBundle> optionList, List<TileWeightBundle> validOption)
    // {
    //     optionList = await ValidateOptionsAsync(optionList, validOption);
    //     
    //     if (optionList.Count == 0)
    //     {
    //         Debug.LogError("Keine gültigen Optionen mehr! Dies kann den Algorithmus stoppen.");
    //     }
    // }
}

public struct GridInitializationJob : IJobParallelFor
{
    public int Dimensions;
    public NativeArray<Vector3> CellPositions; // Positionen für die Zellen
    public NativeArray<int> CellData;          // Daten für Zellen-Berechnung

    public void Execute(int index)
    {
        int x = index % Dimensions;
        int z = index / Dimensions;

        // Berechnen der Positionen (Zellen im Grid)
        CellPositions[index] = new Vector3(x, 0, z);

        // Initialisiere beliebige Zell-Daten (z. B. spezifische Initialisierungslogik)
        CellData[index] = x + z; // Beispiel: einfache Initialisierung
    }
}