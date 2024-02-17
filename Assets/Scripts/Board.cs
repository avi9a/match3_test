using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Board
{
    public static int row;
    public static int collumn;
    
    [Serializable]
    public struct boardData
    {
        public bool[] elements;
    }
    public boardData[] board = new boardData[row + collumn];
}
