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
        if (Collapsed)
        {
            Debug.LogWarning($"[MarkAsPath] Versuch, eine bereits kollabierte Zelle erneut als Weg zu markieren: {this.name}");
            return;
        }

        Debug.Log($"[MarkAsPath] Markiere Zelle {this.name} als Weg.");
        IsPath = true;
        Collapsed = true;
    }
}