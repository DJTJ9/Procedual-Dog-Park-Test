using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridSize;  // Größe des Grids (z.B. 10x10x1 oder ein anderes Format)
    public Cell[,,] grid; // 3D-Grid des Spiels

    public void InitializeGrid(int size)
    {
        gridSize = size;
        grid = new Cell[size, 1, size]; // Höhe auf 1 beschränkt für 2D
        
        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector3Int position = new Vector3Int(x, 0, z);
                grid[x, 0, z] = InstantiateCell(position); // Fülle jede Position mit einer Instanz
            }
        }
    }
    
    private Cell InstantiateCell(Vector3Int position)
    {
        Cell newCell = new GameObject($"Cell_{position.x}_{position.z}").AddComponent<Cell>();
        newCell.CreateCell(false, null); // Initialize ohne TileOptions
        return newCell;
    }

    public void SetCell(Vector3Int position, Cell cell)
    {
        grid[position.x, position.y, position.z] = cell;
    }

    public Cell GetCell(Vector3Int position)
    {
        return grid[position.x, position.y, position.z];
    }
}