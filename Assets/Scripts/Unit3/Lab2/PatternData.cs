using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternData
{
    public int[] Pattern;
    public int ID;
    public float Weight;
    public List<int>[] Adjacency = new List<int>[4];

    public PatternData(int id, int[] pattern, float weight)
    {
        ID = id;
        Pattern = pattern;
        Weight = weight;
        for (int i = 0; i < 4; i++)
        {
            Adjacency[i] = new List<int>();
        }
    }
}

