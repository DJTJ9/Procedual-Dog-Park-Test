using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrunkardsWalk : MonoBehaviour
{
    public int Steps;
    public float TurnProbability;
    public Vector3Int StartPoint;
    public GameObject PathPrefab;

    public void GenerateDrunkardsWalk(int dimensions, List<Cell> grid) {
        Vector3Int currentPosition = StartPoint;
        int startIndex = currentPosition.x + currentPosition.z * dimensions;
        Cell startCell = grid[startIndex];

        if (startCell != null && !startCell.IsPath) {
            startCell.MarkAsPath(); // Markiere die Zelle als Pfad
            
            // // Falls ein Tile gesetzt wird, wähle eine Standard-Pfad-Option
            // if (startCell.TileOptions != null && startCell.TileOptions.Length > 0) {
            //     // Finde das passende PathTile (z. B. mit Tag "PathTile")
            //     var pathTile = startCell.TileOptions.FirstOrDefault(bundle => 
            //         bundle.Tile.CompareTag("PathTile"));
            //
            //     if (pathTile.Tile != null) {
            //         // Instanziere das PathPrefab am Startpunkt
            Vector3 worldPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
            Instantiate(PathPrefab, worldPosition, Quaternion.identity);
            //
            //         // Setze die Zelle auf die Auswahl des PathTiles und markiere sie als "kollabiert"
            //         startCell.TileOptions = new TileWeightBundle[] { pathTile };
            //         startCell.Collapsed = true;
            //     } else {
            //         Debug.LogWarning($"No valid PathTile found for StartPoint at {currentPosition}");
            //     }
            // }
        }

        Debug.Log($"Start Generating Drunkard's Walk from {currentPosition}");

    if (!IsPositionValid(currentPosition, dimensions)) {
        Debug.LogError($"Invalid start position: {currentPosition}");
        return;
    }

    // Die erste Richtung wird zufällig gewählt
    int lastDirection = Random.Range(0, 4);

    for (int i = 0; i < Steps; i++) {
        int currentDirection;

        // Wähle eine neue Richtung, die nicht die entgegengesetzte zur letzten ist
        do {
            currentDirection = Random.value > TurnProbability ? lastDirection : Random.Range(0, 4);
        } while (IsOppositeDirection(lastDirection, currentDirection));

        // Bewege dich basierend auf der aktuellen Richtung
        switch (currentDirection) {
            case 0: // Nach links
                currentPosition.x = Mathf.Max(0, currentPosition.x - 1);
                break;
            case 1: // Nach rechts
                currentPosition.x = Mathf.Min(dimensions - 1, currentPosition.x + 1);
                break;
            case 2: // Vorwärts
                currentPosition.z = Mathf.Min(dimensions - 1, currentPosition.z + 1);
                break;
            case 3: // Rückwärts
                currentPosition.z = Mathf.Max(0, currentPosition.z - 1);
                break;
        }

        if (!IsPositionValid(currentPosition, dimensions)) {
            Debug.LogError($"Invalid position at step {i}: {currentPosition}");
            continue;
        }

        // Indiziere die Position in der Grid-Liste
        int gridIndex = currentPosition.x + currentPosition.z * dimensions;
        var cell = grid[gridIndex];

        if (cell == null) {
            Debug.LogError($"Cell is null at position: {currentPosition}");
            continue;
        }

        if (cell.Collapsed) {
            Debug.Log($"Cell already collapsed at: {currentPosition}");
            continue;
        }

        // Markiere die Zelle als Weg
        cell.Collapsed = true;
        
        if(!cell.IsPath) cell.MarkAsPath();

        // Instanziere die grafische Darstellung des Wegs
        Vector3 worldPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
        Instantiate(PathPrefab, worldPosition, PathPrefab.transform.rotation);

        // Aktualisiere die letzte Richtung
        lastDirection = currentDirection;
    }
    }

// Helferfunktion, um zu überprüfen, ob zwei Richtungen entgegengesetzt sind
    private bool IsOppositeDirection(int dir1, int dir2) {
        return (dir1 == 0 && dir2 == 1) || (dir1 == 1 && dir2 == 0) ||
               (dir1 == 2 && dir2 == 3) || (dir1 == 3 && dir2 == 2);
    }

    private bool IsPositionValid(Vector3Int position, int dimensions) {
        return position.x >= 0 && position.x < dimensions &&
               position.y >= 0 && position.y < 1 && // Höhe auf 1 beschränkt
               position.z >= 0 && position.z < dimensions;
    }
}