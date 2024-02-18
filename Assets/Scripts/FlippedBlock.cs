using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlippedBlock
{
    public Cube one;
    public Cube two;

    public FlippedBlock(Cube oneCube, Cube twoCubbe)
    {
        one = oneCube;
        two = twoCubbe;
    }

    public Cube GetBlock(Cube cube)
    {
        if (cube == one)
            return two;
        else if (cube == two)
            return one;
        else
            return null;
    }
}
