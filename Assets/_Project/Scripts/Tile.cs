using UnityEngine;
using UnityEngine.Serialization;

public class Tile : MonoBehaviour
{
    public TileWeightBundle[] UpNeighbours;
    public TileWeightBundle[] DownNeighbours;
    public TileWeightBundle[] LeftNeighbours;
    public TileWeightBundle[] RightNeighbours;


    // Überschreibe Equals für allgemeine Vergleiche
    public override bool Equals(object obj) {
        if (obj is Tile other) {
            return this == other; // Reuse von == Implementierung
        }

        return false;
    }
}

[System.Serializable]
public struct TileWeightBundle
{
    public Tile Tile;
    public int Weight;
    
    // Überschreibe == Operator
    public static bool operator == (TileWeightBundle a, TileWeightBundle b)
    {
        return a.Tile == b.Tile;
        // return a.Tile == b.Tile && a.Weight == b.Weight;
        
        // OPerator für TIle Vergleich überschreiben!!!!
    }
    // Überschreibe != Operator (immer zusammen mit == benötigt)
    public static bool operator !=(TileWeightBundle a, TileWeightBundle b)
    {
        return !(a == b);
    }


}