using UnityEngine;
using UnityEngine.Serialization;

public class Cell : MonoBehaviour
{
    public bool Collapsed;
    public TileWeightBundle[] TileOptions;
    public bool IsPath;

    public void CreateCell(bool collapseState, TileWeightBundle[] tiles)
    {
        Collapsed = collapseState;
        TileOptions = tiles;
        IsPath = false;
    }

    public void RecreateCell(TileWeightBundle[] tiles)
    {
        TileOptions = tiles;
    }
    
    public void MarkAsPath()
    {
        IsPath = true;
        Collapsed = true;
    }
}