using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;


public class MapGeneration : MonoBehaviour
{
    //TODO: 
    /*Kleine Seeflecken nach dem Algorhitmus miteinanderverbinden (maybe)
     *
     */

    public int Dimensions;
    public TileWeightBundle[] TileObjects;
    public List<Tile> LandTiles;
    public List<Tile> WaterTiles;
    public List<GameObject> DogParkPrefabs;
    public int NumberOfPrefabsToPlace;
    public List<Cell> GridComponents;
    public Cell CellObj;

    [SerializeField] private GameObject fencePrefab;       
    [SerializeField] private float fenceSegmentLength = 1f;

    private int _iterations = 0;
    private Stopwatch stopwatch = new();
    private List<Cell> floorCells = new();
    private List<CombineInstance> landTileMeshes = new List<CombineInstance>();
    private GameObject combinedLandTileObject;

    void Awake() {
        stopwatch.Start();
        LandTiles = new();
        WaterTiles = new();
        GridComponents = new List<Cell>();
        InitializeGrid();
        StartCoroutine(CheckEntropy());
    }

    void InitializeGrid() {
        for (int z = 0; z < Dimensions; z++) {
            for (int x = 0; x < Dimensions; x++) {
                Cell newCell = Instantiate(CellObj, new Vector3(x, 0, z), Quaternion.identity);
                newCell.CreateCell(false, TileObjects);
                GridComponents.Add(newCell);
            }
        }
    }


    IEnumerator CheckEntropy() {
        List<Cell> tempGrid = new List<Cell>(GridComponents);

        tempGrid.RemoveAll(c => c.Collapsed);

        tempGrid.Sort((a, b) => { return a.TileOptions.Length - b.TileOptions.Length; });

        if (tempGrid.Count == 0) {
            yield break;
        }

        int arrLength = tempGrid[0].TileOptions.Length;

        int stopIndex = default;

        for (int i = 1; i < tempGrid.Count; i++) {
            if (tempGrid[i].TileOptions.Length > arrLength) {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0) {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        yield return null;

        CollapseCell(tempGrid[UnityEngine.Random.Range(0, tempGrid.Count)]);
    }

    void CollapseCell(Cell cellToCollapse) {
        if (cellToCollapse.Collapsed)
            return;

        // Wähle eine zufällige Kachel für die Zelle
        TileWeightBundle tile = SelectTileBasedOnWeight(cellToCollapse.TileOptions);
        cellToCollapse.TileOptions = new TileWeightBundle[] { tile };
        cellToCollapse.Collapsed = true;

        // Instanziere das Tile in der Welt
        Tile instantiatedTile = Instantiate(tile.Tile, cellToCollapse.transform.position, tile.Tile.transform.rotation);

        if (tile.Tile == TileObjects[0].Tile) floorCells.Add(cellToCollapse);


        if (LandTiles.Contains(tile.Tile)) {
            MeshFilter meshFilter = instantiatedTile.GetComponent<MeshFilter>();
            if (meshFilter != null) {
                CombineInstance combineInstance = new CombineInstance {
                    mesh = meshFilter.sharedMesh,
                    transform = instantiatedTile.transform.localToWorldMatrix
                };
                landTileMeshes.Add(combineInstance);
            }
        }

        UpdateGeneration();
    }

    TileWeightBundle SelectTileBasedOnWeight(TileWeightBundle[] options) {
        int totalWeight = 0;
        // UnityEngine.Debug.LogWarning($"{options.Length} tile options selected");
        // Berechne das Gesamtgewicht
        foreach (var option in options) {
            totalWeight += option.Weight;
        }

        // Zufällige Gewichtsauswahl
        int randomWeight = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var option in options) {
            currentWeight += option.Weight;
            if (randomWeight <= currentWeight) {
                return option; // Passendes Tile gefunden
            }
        }

        if (totalWeight == 0)
            throw new Exception("Total weight is 0. Check weight distribution.");

        throw new Exception("Failed to select a tile. Check weight distribution.");
    }

    void UpdateGeneration() {
        List<Cell> newGenerationCell = new List<Cell>(GridComponents);

        for (int z = 0; z < Dimensions; z++) {
            for (int x = 0; x < Dimensions; x++) {
                var index = x + z * Dimensions;
                if (GridComponents[index].Collapsed) {
                    newGenerationCell[index] = GridComponents[index];
                }
                else {
                    List<TileWeightBundle> options = new List<TileWeightBundle>();
                    foreach (TileWeightBundle t in TileObjects) {
                        options.Add(t);
                    }

                    //update above
                    if (z > 0) {
                        Cell up = GridComponents[x + (z - 1) * Dimensions];
                        List<TileWeightBundle> validOptions = new List<TileWeightBundle>();

                        if (up.IsPath) {
                            validOptions.AddRange(up.TileOptions.Where(tile => tile.Tile.CompareTag("PathNeighbour")));
                        }
                        else {
                            foreach (TileWeightBundle possibleOptions in up.TileOptions) {
                                var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                                var valid = TileObjects[valOption].Tile.UpNeighbours;

                                validOptions = validOptions.Concat(valid).ToList();
                            }
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look down
                    if (z < Dimensions - 1) {
                        Cell down = GridComponents[x + (z + 1) * Dimensions];
                        List<TileWeightBundle> validOptions = new List<TileWeightBundle>();

                        if (down.IsPath) {
                            validOptions.AddRange(down.TileOptions.Where(tile =>
                                tile.Tile.CompareTag("PathNeighbour")));
                        }
                        else {
                            foreach (TileWeightBundle possibleOptions in down.TileOptions) {
                                var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                                var valid = TileObjects[valOption].Tile.DownNeighbours;

                                validOptions = validOptions.Concat(valid).ToList();
                            }
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look left
                    if (x > 0) {
                        Cell left = GridComponents[x - 1 + z * Dimensions];
                        List<TileWeightBundle> validOptions = new List<TileWeightBundle>();

                        if (left.IsPath) {
                            validOptions.AddRange(left.TileOptions.Where(tile =>
                                tile.Tile.CompareTag("PathNeighbour")));
                        }
                        else {
                            foreach (TileWeightBundle possibleOptions in left.TileOptions) {
                                var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                                var valid = TileObjects[valOption].Tile.RightNeighbours;

                                validOptions = validOptions.Concat(valid).ToList();
                            }
                        }

                        CheckValidity(options, validOptions);
                    }

                    //update right
                    if (x < Dimensions - 1) {
                        Cell right = GridComponents[x + 1 + z * Dimensions];
                        List<TileWeightBundle> validOptions = new List<TileWeightBundle>();

                        if (right.IsPath) {
                            validOptions.AddRange(
                                right.TileOptions.Where(tile => tile.Tile.CompareTag("PathNeighbour")));
                        }
                        else {
                            foreach (TileWeightBundle possibleOptions in right.TileOptions) {
                                var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                                var valid = TileObjects[valOption].Tile.LeftNeighbours;

                                validOptions = validOptions.Concat(valid).ToList();
                            }
                        }

                        CheckValidity(options, validOptions);
                    }

                    TileWeightBundle[] newTileList = new TileWeightBundle[options.Count];

                    for (int i = 0; i < options.Count; i++) {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        GridComponents = newGenerationCell;
        _iterations++;

        if (_iterations < Dimensions * Dimensions) {
            StartCoroutine(CheckEntropy());
        }
        else {
            PlaceAdditionalPrefabs();
            PlaceFenceAroundArea();
            stopwatch.Stop();
            Debug.Log($"Time: {stopwatch.ElapsedMilliseconds} ms\"");
        }
    }

    void CheckValidity(List<TileWeightBundle> optionList, List<TileWeightBundle> validOption) {
        // Entfernen Sie Duplikate basierend auf dem Schlüssel "Tile"
        Dictionary<Tile, TileWeightBundle> validOptionMap = validOption
            .Where(bundle => bundle.Tile != null) // Entfernt alle Einträge, bei denen Tile null ist
            .GroupBy(bundle => bundle.Tile)
            .ToDictionary(group => group.Key, group => group.First());

        foreach (var bundle in validOption) {
            if (bundle.Tile == null) {
                continue;
            }

            if (!validOptionMap.ContainsKey(bundle.Tile)) {
                validOptionMap.Add(bundle.Tile, bundle);
            }
        }

        for (int x = optionList.Count - 1; x >= 0; x--) {
            var optionTile = optionList[x].Tile; 
            // Suche in validOption nach einem Bundle mit dem gleichen Tile
            if (validOptionMap.TryGetValue(optionTile, out var match)) {
                optionList[x] = new TileWeightBundle {
                    Tile = optionTile,
                    Weight = match.Weight
                };
            }
            else {
                // Entfernt das Tile aus den Optionen, da es nicht in validOption enthalten ist
                optionList.RemoveAt(x);
            }
        }
    }

    void FinalizeLandTileMesh() {
        if (landTileMeshes.Count == 0) {
            Debug.LogWarning("Keine Land-Tiles gefunden, um sie zu kombinieren.");
            return;
        }

        // Neues GameObject für das kombinierte Land-Tile-Mesh erstellen
        combinedLandTileObject = new GameObject("Combined Land Tiles");
        combinedLandTileObject.transform.position = Vector3.zero;
        combinedLandTileObject.transform.rotation = Quaternion.identity;

        MeshFilter landTileMeshFilter = combinedLandTileObject.AddComponent<MeshFilter>();
        MeshRenderer landTileRenderer = combinedLandTileObject.AddComponent<MeshRenderer>();

        // Kombiniere die Meshes der Land-Tiles
        Mesh combinedLandMesh = new Mesh();
        combinedLandMesh.CombineMeshes(landTileMeshes.ToArray(), true, true);
        landTileMeshFilter.mesh = combinedLandMesh;

        // Das Material für das kombinierte Mesh festlegen (verwendet das Material des ersten Tiles)
        landTileRenderer.material = TileObjects[0].Tile.GetComponent<MeshRenderer>().sharedMaterial;

        // Falls benötigt, füge einen Collider hinzu
        MeshCollider collider = combinedLandTileObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedLandMesh;

        Debug.Log("Land-Tiles wurden erfolgreich zu einem einzigen großen Mesh kombiniert.");
    }

    void PlaceAdditionalPrefabs() {
        // Beschränke die Liste der verfügbaren Zellen (nur inneren Bereich)
        List<Cell> innerCells = floorCells.Where(cell =>
        {
            Vector3 cellPosition = cell.transform.position;

            // Exkludiere Zellen, die sich an den Rändern befinden
            return cellPosition.x > 0 && cellPosition.x < Dimensions - 1 &&
                   cellPosition.z > 0 && cellPosition.z < Dimensions - 1;
        }).ToList();

        for (int i = 0; i < NumberOfPrefabsToPlace; i++)
        {
            if (innerCells.Count == 0 || DogParkPrefabs.Count == 0) break;

            // Wähle eine zufällige Zelle aus dem inneren Bereich und ein zufälliges Prefab
            Cell randomInnerCell = innerCells[UnityEngine.Random.Range(0, innerCells.Count)];
            GameObject randomPrefab = DogParkPrefabs[UnityEngine.Random.Range(0, DogParkPrefabs.Count)];

            // Zufällige Rotation (0, 90, 180, 270 Grad)
            float randomYRotation = UnityEngine.Random.Range(0, 4) * 90f; // Werte: 0, 90, 180, 270

            // Instanziiere das Prefab mit der zufälligen Rotation
            Instantiate(randomPrefab, randomInnerCell.transform.position, Quaternion.Euler(0f, randomYRotation, 0f));

            // Entferne die genutzte Zelle aus der Liste, damit dort keine weiteren Prefabs spawnen
            innerCells.Remove(randomInnerCell);
        }
    }

    void PlaceFenceAroundArea() {
        if (!fencePrefab) {
            Debug.LogError("Zaun Prefab nicht zugewiesen!");
            return;
        }

        float gridStart = 0;            // Unterste Ecke
        float gridEnd = Dimensions - 1; // Oberste Ecke
        float offset = 0f;              // Abstand zur Spielfeldmitte 


        // Zaun entlang der oberen und unteren Seiten
        for (float x = gridStart + offset; x <= gridEnd - offset; x += fenceSegmentLength) {
            // Oberer Zaunabschnitt (innen)
            Instantiate(fencePrefab, new Vector3(x, 0, gridStart + offset), Quaternion.identity);

            // Unterer Zaunabschnitt (innen)
            Instantiate(fencePrefab, new Vector3(x, 0, gridEnd - offset), Quaternion.identity);
        }

        // Zaun entlang der linken und rechten Seiten
        for (float z = gridStart + offset; z <= gridEnd - offset; z += fenceSegmentLength) {
            // Linker Zaunabschnitt (innen)
            Instantiate(fencePrefab, new Vector3(gridStart + offset, 0, z), Quaternion.Euler(0, 90, 0));

            // Rechter Zaunabschnitt (innen)
            Instantiate(fencePrefab, new Vector3(gridEnd - offset, 0, z), Quaternion.Euler(0, 90, 0));
        }
    }
}