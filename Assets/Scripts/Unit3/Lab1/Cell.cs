using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Cell
{
    public List<int> PossibleTileIDs;
    public int Row;
    public int Col;
    public bool Collapsed = false;
    public int ChosenTileID = -1;
    public int Entropy => PossibleTileIDs.Count;
    public bool IsContradiction => Entropy == 0 && !Collapsed;

    public Cell(int row, int col, List<int> allTileIDs)
    {
        Row = row;
        Col = col;
        if (allTileIDs != null)
        {
            PossibleTileIDs = new List<int>(allTileIDs);
        }
        else
        {
            PossibleTileIDs = new List<int>();
        }
    }
    public bool RemoveOption(int tileID)
    {
        return PossibleTileIDs.Remove(tileID);
    }
    public Cell DeepCopy()
    {
        Cell copy = new Cell(this.Row, this.Col, null);
        copy.PossibleTileIDs = new List<int>(this.PossibleTileIDs);
        copy.Collapsed = this.Collapsed;
        copy.ChosenTileID = this.ChosenTileID;
        return copy;
    }
}
