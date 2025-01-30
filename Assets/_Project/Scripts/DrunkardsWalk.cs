using System.Collections.Generic;
using UnityEngine;

public class DrunkardsWalk : MonoBehaviour
{
    public int steps = 20;
    public float TurnProbability;
    public Vector3Int startPoint;
    public GameObject pathPrefab;
    public Transform gridParent;

    public void GenerateDrunkardsWalk(Vector3Int startPoint, int steps, int dimensions, List<Cell> grid, GameObject pathPrefab) {
        Vector3Int currentPosition = startPoint;

        Debug.Log($"Start Generating Drunkard's Walk from {currentPosition}");

        if (!IsPositionValid(currentPosition, dimensions)) {
            Debug.LogError($"Invalid start position: {currentPosition}");
            return;
        }

        // Die erste Richtung wird zufällig gewählt
        int lastDirection = Random.Range(0, 4);

        for (int i = 0; i < steps; i++) {
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

            // Holt die Zelle an der neuen Position
            var cell = grid[i];

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

            // Instanziere die grafische Darstellung des Wegs
            Vector3 worldPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
            Instantiate(pathPrefab, worldPosition, Quaternion.identity);

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