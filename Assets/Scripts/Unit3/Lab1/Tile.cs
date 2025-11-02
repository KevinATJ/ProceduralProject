using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public int ID;
    public string Name;
    public GameObject Prefab;
    public List<int> UpCompatibility;
    public List<int> DownCompatibility;
    public List<int> LeftCompatibility;
    public List<int> RightCompatibility;

    public Tile()
    {
        UpCompatibility = new List<int>();
        DownCompatibility = new List<int>();
        LeftCompatibility = new List<int>();
        RightCompatibility = new List<int>();
    }

    public Tile(int id, string name)
    {
        ID = id;
        Name = name;
        UpCompatibility = new List<int>();
        DownCompatibility = new List<int>();
        LeftCompatibility = new List<int>();
        RightCompatibility = new List<int>();
    }
}
