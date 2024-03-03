using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject
{
    public Board board;
    public Sprite[] blocks;
    private int width = 5;
    private int height = 6;
    public GameBoardCube[,] gameBoard;

    public int[] blockValues;

    public RuntimeAnimatorController animatorWater;
    public RuntimeAnimatorController animatorFire;
}
