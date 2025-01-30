using UnityEngine;
using UnityEngine.Serialization;

public class Cell : MonoBehaviour
{
    public bool Collapsed;
    public TileWeightBundle[] TileOptions;

    public void CreateCell(bool collapseState, TileWeightBundle[] tiles)
    {
        Collapsed = collapseState;
        TileOptions = tiles;
    }

    public void RecreateCell(TileWeightBundle[] tiles)
    {
        TileOptions = tiles;
    }
}