using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditableMatrix
{
    public int width = 5;
    public int height = 5;
    public List<Row> rows = new List<Row>();

    [Serializable]
    public class Row
    {
        public List<int> values = new List<int>();
    }

    public int[,] ToArray()
    {
        int[,] array = new int[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                array[y, x] = rows[y].values[x];
            }
        }
        return array;
    }

    public void Resize(int newWidth, int newHeight)
    {
        width = newWidth;
        height = newHeight;

        while (rows.Count < height)
            rows.Add(new Row());
        while (rows.Count > height)
            rows.RemoveAt(rows.Count - 1);

        foreach (var row in rows)
        {
            while (row.values.Count < width)
                row.values.Add(0);
            while (row.values.Count > width)
                row.values.RemoveAt(row.values.Count - 1);
        }
    }
}
