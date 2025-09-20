using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public bool isObstacle;
    public int gCost;
    public int hCost;
    public Node parent;
    public int movementPenalty;

    public Node(Vector3 _worldPos, int _gridX, int _gridY, bool _isObstacle)
    {
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        isObstacle = _isObstacle;
        movementPenalty = 0;
    }

    public int fCost
    {
        get { return gCost + hCost + movementPenalty; }
    }
}

