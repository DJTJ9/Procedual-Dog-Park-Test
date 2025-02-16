using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    public int Dimensions;
    public TileWeightBundle[] TileObjects;
    public List<Cell> GridComponents;
    public Cell CellObj;

    public DrunkardsWalk DrunkardsWalk;

    private int iterations;

    private readonly object lockObject = new object();

    private void Awake() {
        DrunkardsWalk = GetComponent<DrunkardsWalk>();
        GridComponents = new List<Cell>();
        InitializeGrid();
        DrunkardsWalk.GenerateDrunkardsWalk(Dimensions, GridComponents);
        StartCoroutine(RunWaveFunction());
    }

    private void InitializeGrid() {
        for (int z = 0; z < Dimensions; z++) {
            for (int x = 0; x < Dimensions; x++) {
                Cell newCell = Instantiate(CellObj, new Vector3(x, 0, z), Quaternion.identity);
                newCell.CreateCell(false, TileObjects);
                GridComponents.Add(newCell);
            }
        }
    }

    IEnumerator RunWaveFunction() {
        while (iterations < Dimensions * Dimensions) {
            yield return StartCoroutine(CheckEntropy());
            iterations++;
        }

        Debug.Log("Generierung abgeschlossen!");
    }

    IEnumerator CheckEntropy() {
        List<Cell> tempGrid = null;

        Task.Run(() => {
            tempGrid = GridComponents
                .Where(c => !c.Collapsed && !c.IsPath) // Wege ignorieren
                .OrderBy(c => c.TileOptions.Length)
                .ToList();
        }).Wait();

        if (tempGrid == null || tempGrid.Count == 0)
        {
            yield break;
        }

        int minOptions = tempGrid[0].TileOptions.Length;

        Task.Run(() => {
            tempGrid.RemoveAll(cell => cell.TileOptions.Length > minOptions);
        }).Wait();

        Cell cellToCollapse = tempGrid[UnityEngine.Random.Range(0, tempGrid.Count)];
        yield return StartCoroutine(CollapseCell(cellToCollapse));
    }

    IEnumerator CollapseCell(Cell cellToCollapse) {
        if (cellToCollapse.Collapsed) {
            Debug.LogWarning($"Cell {cellToCollapse.transform.position} ist bereits kollabiert. Abbruch!");
            yield break;
        }

        // Auswahl der Tile-Bundles
        TileWeightBundle tile = SelectTileBasedOnWeight(cellToCollapse.TileOptions);

        cellToCollapse.TileOptions = new[] { tile };
        cellToCollapse.Collapsed = true; // Korrigiere den Status direkt nach der Auswahl

        Debug.Log($"Cell {cellToCollapse.transform.position} kollabiert mit Tile {tile.Tile.name}");

        // Unity-spezifisches Instanziieren
        Instantiate(tile.Tile, cellToCollapse.transform.position, tile.Tile.transform.rotation);

        yield return UpdateGenerationAsync();
    }

    TileWeightBundle SelectTileBasedOnWeight(TileWeightBundle[] options) {
        int totalWeight = options.Sum(option => option.Weight);
        int randomWeight = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var option in options) {
            currentWeight += option.Weight;
            if (randomWeight <= currentWeight) {
                return option;
            }
        }

        throw new Exception("Konnte kein Tile mit Gewicht auswählen.");
    }

    IEnumerator UpdateGenerationAsync() {
        Task<List<Cell>> generationUpdateTask = Task.Run(() => {
            List<Cell> updatedGeneration = new List<Cell>(GridComponents);

            Parallel.For(0, Dimensions, z => {
                for (int x = 0; x < Dimensions; x++) {
                    int index = x + z * Dimensions;
                    Cell currentCell = GridComponents[index];

                    if (currentCell.Collapsed) {
                        updatedGeneration[index] = currentCell;
                        continue;
                    }

                    List<TileWeightBundle> options = new List<TileWeightBundle>(TileObjects);
                    lock (lockObject) {
                        ValidateNeighbours(x, z, options);
                        updatedGeneration[index].RecreateCell(options.ToArray());
                    }
                }
            });

            return updatedGeneration;
        });

        // Warten, bis die Generation-Logik abgeschlossen ist
        while (!generationUpdateTask.IsCompleted) {
            yield return null; // Async-Logik Block nicht den Hauptthread
        }

        lock (lockObject) {
            // Ergebnisse aktualisieren
            GridComponents = generationUpdateTask.Result;
        }
    }

    void ValidateNeighbours(int x, int z, List<TileWeightBundle> options) {
        Parallel.Invoke(
            () => {
                if (z > 0) {
                    Cell up = GridComponents[x + (z - 1) * Dimensions];

                    lock (lockObject) {
                        if (up.Collapsed) {
                            Debug.Log(
                                $"[ValidateNeighbours] 'up' Nachbar bei {up.transform.position} bereits kollabiert.");
                            return;
                        }

                        List<TileWeightBundle> validOptions =
                            GetValidOptions(up, neighbour => neighbour.Tile.UpNeighbours);
                        if (up.IsPath) {
                            validOptions.AddRange(up.TileOptions.Where(tile => tile.Tile.CompareTag("PathNeighbour")));
                        }

                        CheckValidity(options, validOptions);
                    }
                }
            },
            // Ähnliche Überprüfung für die anderen Nachbarrichtungen
            () => {
                if (z < Dimensions - 1) {
                    Cell down = GridComponents[x + (z + 1) * Dimensions];

                    lock (lockObject) {
                        if (down.Collapsed) {
                            Debug.Log(
                                $"[ValidateNeighbours] 'down' Nachbar bei {down.transform.position} bereits kollabiert.");
                            return;
                        }

                        List<TileWeightBundle> validOptions =
                            GetValidOptions(down, neighbour => neighbour.Tile.DownNeighbours);
                        if (down.IsPath) {
                            validOptions.AddRange(down.TileOptions.Where(tile =>
                                tile.Tile.CompareTag("PathNeighbour")));
                        }

                        CheckValidity(options, validOptions);
                    }
                }
            },
            () => {
                if (x > 0) {
                    Cell left = GridComponents[x - 1 + z * Dimensions];

                    lock (lockObject) {
                        if (left.Collapsed) {
                            Debug.Log(
                                $"[ValidateNeighbours] 'left' Nachbar bei {left.transform.position} bereits kollabiert.");
                            return;
                        }

                        List<TileWeightBundle> validOptions =
                            GetValidOptions(left, neighbour => neighbour.Tile.LeftNeighbours);
                        if (left.IsPath) {
                            validOptions.AddRange(left.TileOptions.Where(tile =>
                                tile.Tile.CompareTag("PathNeighbour")));
                        }

                        CheckValidity(options, validOptions);
                    }
                }
            },
            () => {
                if (x < Dimensions - 1) {
                    Cell right = GridComponents[x + 1 + z * Dimensions];

                    lock (lockObject) {
                        if (right.Collapsed) {
                            Debug.Log(
                                $"[ValidateNeighbours] 'right' Nachbar bei {right.transform.position} bereits kollabiert.");
                            return;
                        }

                        List<TileWeightBundle> validOptions =
                            GetValidOptions(right, neighbour => neighbour.Tile.RightNeighbours);
                        if (right.IsPath) {
                            validOptions.AddRange(
                                right.TileOptions.Where(tile => tile.Tile.CompareTag("PathNeighbour")));
                        }

                        CheckValidity(options, validOptions);
                    }
                }
            }
        );
    }

    List<TileWeightBundle> GetValidOptions(Cell               cell,
        Func<TileWeightBundle, IEnumerable<TileWeightBundle>> getNeighbours) {
        List<TileWeightBundle> validOptions = new List<TileWeightBundle>();
        foreach (var option in cell.TileOptions) {
            validOptions.AddRange(getNeighbours(option));
        }

        return validOptions;
    }

    void CheckValidity(List<TileWeightBundle> optionList, List<TileWeightBundle> validOptions) {
        Dictionary<Tile, TileWeightBundle> validOptionMap = validOptions
            .GroupBy(option => option.Tile)
            .ToDictionary(g => g.Key, g => g.First());

        optionList.RemoveAll(option => !validOptionMap.ContainsKey(option.Tile));
    }
}