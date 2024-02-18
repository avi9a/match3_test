using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameBoard
{
    public int value;
    public Point index;
    public Cube cube;

    public GameBoard(int pointValue, Point pointIndex)
    {
        value = pointValue;
        index = pointIndex;
    }

    public void SetCube(Cube piece)
    {
        cube = piece;
        value = (cube == null) ? 0 : cube.value;
        if (cube == null) return;
        cube.SetIndex(index);
    }

    public Cube GetCube()
    {
        return cube;
    }
}
