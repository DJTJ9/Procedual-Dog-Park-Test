using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrunkardsWalk : MonoBehaviour
{
    public int Steps;
    public float TurnProbability;
    public Vector3Int StartPoint;
    public GameObject PathPrefab;

    public void GenerateDrunkardsWalk(int dimensions, List<Cell> grid)
{
    Vector3Int currentPosition = StartPoint;
    if (!IsPositionValid(currentPosition, dimensions))
    {
        Debug.LogError($"[DrunkardsWalk] Ungültige Startposition: {currentPosition}");
        return;
    }

    int startIndex = currentPosition.x + currentPosition.z * dimensions;
    Cell startCell = grid[startIndex];

    if (startCell == null || startCell.Collapsed) {
        Debug.LogError($"[DrunkardsWalk] Startzelle ist null oder bereits kollabiert: {currentPosition}");
        return;
    }

    // Markiere die Startzelle sicher ohne Überschreiben
    Debug.Log($"[DrunkardsWalk] Markiere Startzelle als Weg: {currentPosition}");
    MarkCellAsPath(startCell, currentPosition);

    int lastDirection = Random.Range(0, 4); // Zufällige Richtung beim Start

    for (int i = 0; i < Steps; i++)
    {
        int currentDirection;
        do {
            currentDirection = Random.value > TurnProbability ? lastDirection : Random.Range(0, 4);
        } while (IsOppositeDirection(lastDirection, currentDirection));

        // Bewege dich in die gewählte Richtung
        switch (currentDirection)
        {
            case 0: currentPosition.x = Mathf.Max(0, currentPosition.x - 1); break; // Links
            case 1: currentPosition.x = Mathf.Min(dimensions - 1, currentPosition.x + 1); break; // Rechts
            case 2: currentPosition.z = Mathf.Min(dimensions - 1, currentPosition.z + 1); break; // Vorwärts
            case 3: currentPosition.z = Mathf.Max(0, currentPosition.z - 1); break; // Rückwärts
        }

        if (!IsPositionValid(currentPosition, dimensions))
        {
            Debug.LogWarning($"[DrunkardsWalk] Ungültige Position nach Schritt {i}: {currentPosition}");
            continue;
        }

        // Hole die neue Zelle und überprüfe ihren Status
        int gridIndex = currentPosition.x + currentPosition.z * dimensions;
        Cell cell = grid[gridIndex];

        if (cell == null)
        {
            Debug.LogError($"[DrunkardsWalk] Konnte keine Zelle an Position {currentPosition} finden.");
            continue;
        }

        if (cell.Collapsed)
        {
            Debug.Log($"[DrunkardsWalk] Zelle bereits kollabiert: {currentPosition}");
            continue;
        }

        // Markiere die Zelle sicher
        Debug.Log($"[DrunkardsWalk] Markiere Zelle als Weg: {currentPosition}");
        MarkCellAsPath(cell, currentPosition);

        lastDirection = currentDirection; // Aktualisiere die Richtung
    }
}

// Helper-Methode, um Zellen sicher zu markieren
private void MarkCellAsPath(Cell cell, Vector3Int position)
{
    if (cell.Collapsed) {
        Debug.LogWarning($"[MarkCellAsPath] Fehler: Konnte kollabierte Zelle nicht erneut markieren: {position}.");
        return;
    }

    cell.MarkAsPath(); // Markiere Zelle als Weg
    Vector3 worldPosition = new Vector3(position.x, position.y, position.z);
    Instantiate(PathPrefab, worldPosition, PathPrefab.transform.rotation); // Platzierung des visuellen Pfads
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