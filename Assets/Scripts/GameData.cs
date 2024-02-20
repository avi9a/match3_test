using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    // public int level;
    public float[] position;

    public GameData(/*GameManager game,*/ Cube block)
    {
        // level = game.levelNumber;

        position = new float[2];
        position[0] = block.position.x;
        position[1] = block.position.y;
    }
}
