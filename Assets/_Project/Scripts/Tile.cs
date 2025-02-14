using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileWeightBundle[] UpNeighbours;
    public TileWeightBundle[] DownNeighbours;
    public TileWeightBundle[] LeftNeighbours;
    public TileWeightBundle[] RightNeighbours;


    public override bool Equals(object obj)
    {
        if (obj is Tile other && !ReferenceEquals(other, null))
        {
            return name == other.name; // Vergleich basierend auf Namen
        }
        return false;
    }

    public override int GetHashCode()
    {
        return name?.GetHashCode() ?? 0;
    }

    public static bool operator ==(Tile tile1, Tile tile2)
    {
        if (ReferenceEquals(tile1, tile2)) return true;
        if (ReferenceEquals(tile1, null) || ReferenceEquals(tile2, null)) return false;
        return tile1.name == tile2.name;
    }

    public static bool operator !=(Tile tile1, Tile tile2)
    {
        return !(tile1 == tile2);
    }
}

[Serializable]
public struct TileWeightBundle : IEquatable<TileWeightBundle>
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


    public bool Equals(TileWeightBundle other) {
        return Equals(Tile, other.Tile);
    }

    public override bool Equals(object obj) {
        return obj is TileWeightBundle other && Equals(other);
    }

    public override int GetHashCode() {
        return (Tile != null ? Tile.GetHashCode() : 0);
    }
}