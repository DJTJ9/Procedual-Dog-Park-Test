using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class WFC : MonoBehaviour
{
    public int Dimensions;
    public TileWeightBundle[] TileObjects;
    public List<Cell> GridComponents;
    public Cell CellObj;

    public DrunkardsWalk DrunkardsWalk;

    private int _iterations = 0;
    

    void Awake() {
        DrunkardsWalk = GetComponent<DrunkardsWalk>();
        GridComponents = new List<Cell>();
        InitializeGrid();
        DrunkardsWalk.GenerateDrunkardsWalk(Dimensions, GridComponents);
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


    IEnumerator CheckEntropy()
    {
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

    void CollapseCell(Cell cellToCollapse)
    {
        if (cellToCollapse.Collapsed)
            return;

        // Wähle eine zufällige Kachel für die Zelle
        TileWeightBundle tile = SelectTileBasedOnWeight(cellToCollapse.TileOptions);
        cellToCollapse.TileOptions = new TileWeightBundle[] { tile };
        cellToCollapse.Collapsed = true;

        // Instanziere das Tile in der Welt
        Instantiate(tile.Tile, cellToCollapse.transform.position, tile.Tile.transform.rotation);
        
        UpdateGeneration();
    }

    TileWeightBundle SelectTileBasedOnWeight(TileWeightBundle[] options)
    {
        int totalWeight = 0;
        Debug.LogWarning($"{options.Length} tile options selected");
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

                        foreach (TileWeightBundle possibleOptions in up.TileOptions) {
                            var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                            var valid = TileObjects[valOption].Tile.UpNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //update right
                    if (x < Dimensions - 1) {
                        Cell right = GridComponents[x + 1 + z * Dimensions];
                        List<TileWeightBundle> validOptions = new List<TileWeightBundle>();

                        foreach (TileWeightBundle possibleOptions in right.TileOptions) {
                            var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                            var valid = TileObjects[valOption].Tile.LeftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look down
                    if (z < Dimensions - 1) {
                        Cell down = GridComponents[x + (z + 1) * Dimensions];
                        List<TileWeightBundle> validOptions = new List<TileWeightBundle>();

                        foreach (TileWeightBundle possibleOptions in down.TileOptions) {
                            var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                            var valid = TileObjects[valOption].Tile.DownNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look left
                    if (x > 0) {
                        Cell left = GridComponents[x - 1 + z * Dimensions];
                        List<TileWeightBundle> validOptions = new List<TileWeightBundle>();

                        foreach (TileWeightBundle possibleOptions in left.TileOptions) {
                            var valOption = Array.FindIndex(TileObjects, obj => obj == possibleOptions);
                            var valid = TileObjects[valOption].Tile.RightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
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
    }

    void CheckValidity(List<TileWeightBundle> optionList, List<TileWeightBundle> validOption) {
        // for (int x = optionList.Count - 1; x >= 0; x--)
        // {
        //     var element = optionList[x];
        //     if (!validOption.Contains(element))
        //     {
        //         optionList.RemoveAt(x);
        //     }
        // }

        for (int x = optionList.Count - 1; x >= 0; x--) {
            var optionTile = optionList[x].Tile; // Hol dir das Tile aus der aktuellen Option
        
            // Suche in validOption nach einem Bundle mit dem gleichen Tile
            var match = validOption.Find(bundle => bundle.Tile.Equals(optionTile));
        
            if (match.Tile != null) {
                // Aktualisiere das Gewicht des Tiles in der Option
                optionList[x] = new TileWeightBundle {
                    Tile = optionTile,
                    Weight = match.Weight // Update das Gewicht
                };
            }
            else {
                // Entferne das Tile aus den Optionen, da es nicht in validOption enthalten ist
                optionList.RemoveAt(x);
            }
        }
    }
}