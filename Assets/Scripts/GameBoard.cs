using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameBoard // : MonoBehaviour
{
    public int value;
    public Point index;

    public GameBoard(int pointValue, Point pointIndex)
    {
        value = pointValue;
        index = pointIndex;
    }
}
