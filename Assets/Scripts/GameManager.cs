using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using SystemRandom = System.Random;

[Serializable]
public class GameManager : MonoBehaviour
{
    public Board board;
    public Sprite[] blocks;
    [Header("UI Elements")] public RectTransform gameBoardTransform;
    [Header("Prefabs")] public GameObject boardBlock;
    public int width = 5;
    public int height = 6;
    public GameBoardCube[,] gameBoard;

    private SystemRandom random;
    public int[] blockValues;

    private List<Cube> update;
    private List<FlippedBlock> flipped;
    private List<Cube> dead;

    private Animator animator;
    public RuntimeAnimatorController animatorWater;
    public RuntimeAnimatorController animatorFire;

    private void Start()
    {
        string seed = GetRandomSeed();
        random = new SystemRandom(seed.GetHashCode());
        update = new List<Cube>();
        flipped = new List<FlippedBlock>();
        dead = new List<Cube>();
        InitializeBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    private void Update()
    {
        List<Cube> finishedUpdating = new List<Cube>();
        for (int i = 0; i < update.Count; i++)
        {
            Cube cube = update[i];
            if (!cube.UpdateBlock()) finishedUpdating.Add(cube);
        }
    
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            Cube cube = finishedUpdating[i];
            FlippedBlock flip = GetFlipped(cube);
            List<Point> connected = IsConnnected(cube.index, true);
            if (flip != null)
            {
                Cube flippedCube = flip.GetBlock(cube);
                AddPionts(ref connected, IsConnnected(flippedCube.index, true));
            }
            
            if (connected.Count != 0)
            {
                foreach (Point point in connected)
                {
                    GameBoardCube block = GetBlockAtPoint(point);
                    Cube cubePiece = block.GetCube();
                    if (cubePiece != null)
                        StartCoroutine(DestroyAnimation(cubePiece));
                    dead.Add(cube);
                    block.SetCube(null);
                }

                GravityOnBoard();
            }

            flipped.Remove(flip);
            update.Remove(cube);
        }
    }
    
    private IEnumerator DestroyAnimation(Cube cubePiece)
    {
        var cubeAnimator = cubePiece.GetComponent<Animator>();
        cubeAnimator.SetBool("Destroy", true);
        yield return new WaitForSeconds(1f);
        cubePiece.gameObject.SetActive(false);
    }

    private void GravityOnBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                Point point = new Point(x, y);
                GameBoardCube block = GetBlockAtPoint(point);
                int value = GetValueAtPoint(point);
                if (value != 0) continue; //if it is not a hole => do nothing
                for (int nexty = y-1; nexty >= -1; nexty--)
                {
                    Point next = new Point(x, nexty);
                    int nextValue = GetValueAtPoint(next);
                    if (nextValue == 0) continue;
                    if (nextValue != -1)
                    {
                        GameBoardCube get = GetBlockAtPoint(next);
                        Cube cube = get.GetCube();
                        block.SetCube(cube);
                        update.Add(cube);
                        get.SetCube(null);
                    }
                    break;
                }
            }
        }
    }

    private FlippedBlock GetFlipped(Cube cube)
    {
        FlippedBlock flip = null;
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].GetBlock(cube) != null)
            {
                flip = flipped[i];
                break;
            }
        }

        return flip;
    }

    public void InitializeBoard()
    {
        gameBoard = new GameBoardCube[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gameBoard[x, y] = new GameBoardCube(board.board[y].elements[x] ? - 1 : FillBlock(), new Point(x, y));
            }
        }
    }
    
    public void VerifyBoard()
    {
        List<int> remove;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point point = new Point(x, y);
                int value = GetValueAtPoint(point);
                if (value <= 0) continue;

                remove = new List<int>();
                while (IsConnnected(point, true).Count > 0)
                {
                    value = GetValueAtPoint(point);
                    if (!remove.Contains(value))
                        remove.Add(value);
                    SetValueAtPoint(point, NewValue(ref remove));
                }
            }
        }
    }

    public void InstantiateBoard()
    {
        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                GameBoardCube board = GetBlockAtPoint(new Point(x, y));
                int value = board.value;
                if (value <= 0) continue;
                GameObject p = Instantiate(boardBlock, gameBoardTransform);
                Cube cube = p.GetComponent<Cube>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(50 + (64 * x), -50 - (64 * y));
                value = blockValues[index];
                cube.Initialize(value,  new Point(x, y), blocks[value - 1]);
                board.SetCube(cube);
                index++;
                 
                animator = cube.GetComponent<Animator>();
                animator.runtimeAnimatorController = cube.value == 1 ? animatorWater : animatorFire;
            }
        }
    }

    public void ResetBlock(Cube cube)
    {
        cube.ResetPosition();
        update.Add(cube);
        Debug.Log("Reset");
    }

    public void FlipBlocks(Point one, Point two, bool main)
    {
        if (GetValueAtPoint(one) < 0) return;
        GameBoardCube pointOne = GetBlockAtPoint(one);
        Cube cubeOne = pointOne.GetCube();
        if (GetValueAtPoint(two) > 0)
        {
            GameBoardCube pointTwo = GetBlockAtPoint(two);
            Cube cubeTwo = pointTwo.GetCube();
            pointOne.SetCube(cubeTwo);
            pointTwo.SetCube(cubeOne);
            flipped.Add(new FlippedBlock(cubeOne, cubeTwo));
            var cubeOneIndex = cubeOne.transform.GetSiblingIndex();
            var cubeTwoIndex = cubeTwo.transform.GetSiblingIndex();
            update.Add(cubeOne);
            update.Add(cubeTwo);
            cubeOne.transform.SetSiblingIndex(cubeTwoIndex);
            cubeTwo.transform.SetSiblingIndex(cubeOneIndex);
        }
        else
        {
            ResetBlock(cubeOne);
        }
    }
    
    private List<Point> IsConnnected(Point point, bool main)
    {
        List<Point> connected = new List<Point>();
        int value = GetValueAtPoint(point);
        Point[] directions =
        {
            Point.Up,
            Point.Down,
            Point.Left,
            Point.Right
        };
    
        foreach (var direction in directions) //checking if there is 2 or more same blocks in the directions
        {
            List<Point> line = new List<Point>();
            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(point, Point.Mult(direction, i));
                if (GetValueAtPoint(check) == value)
                {
                    line.Add(check);
                    same++;
                }
            }

            if (same > 1)
                AddPionts(ref connected, line);
        }

        for (int i = 0; i < 2; i++) //checking if we are in the middle of 2 of the same blocks
        {
            List<Point> line = new List<Point>();
            int same = 0;
            Point next = Point.Add(point, directions[i]);
            Point nextTwo = Point.Add(point, directions[i + 2]);
            Point[] check = { next, nextTwo };
            foreach (Point nextCheck in check) //checking both sides of the block
            {
                if (GetValueAtPoint(nextCheck) == value)
                {
                    line.Add(nextCheck);
                    same++;
                }
            }
            
            if (same > 1)
                AddPionts(ref connected, line);
        }

        // for (int i = 0; i < 4; i++) //check for a 2x2
        // {
        //     List<Point> square = new List<Point>();
        //     int same = 0;
        //     int next = i + 1;
        //     if (next >= 4)
        //         next -= 4;
        //
        //     Point[] check = { Point.Add(point, directions[i]), Point.Add(point, directions[next]), Point.Add(point, Point.Add(directions[i], directions[next]))};
        //     foreach (Point nextCheck in check)
        //     {
        //         if (GetValue(nextCheck) == value)
        //         {
        //             square.Add(nextCheck);
        //             same++;
        //         }
        //     }
        //
        //     if (same > 2)
        //         AddPionts(ref connected, square);
        // }

        if (main) //check for other matches along the current match
        {
            for (int i = 0; i < connected.Count; i++)
                AddPionts(ref connected, IsConnnected(connected[i], false));
        }

        if (connected.Count > 0)
            connected.Add(point);
        return connected;
    }
    
    private void AddPionts(ref List<Point> points, List<Point> add)
    {
        foreach (var point in add)
        {
            bool doAdd = true;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Equals(point))
                {
                    doAdd = false;
                    break;
                }
            }
            
            if (doAdd) points.Add(point);
        }
    }
    
    private int FillBlock()
    {
        int value = 1;
        value = (random.Next(0, 100) / (100 / blocks.Length)) + 1;
        return value;
    }
    
    private int GetValueAtPoint(Point point)
    {
        if (point.x < 0 || point.x >= width || point.y < 0 || point.y >= height) return -1; // return a hole
        return gameBoard[point.x, point.y].value;
    }

    private void SetValueAtPoint(Point point, int value)
    {
        gameBoard[point.x, point.y].value = value;
    }

    private GameBoardCube GetBlockAtPoint(Point point)
    {
        return gameBoard[point.x, point.y];
    }
    
    private int NewValue(ref List<int> remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < blocks.Length; i++)
            available.Add(i + 1);
        foreach (var i in remove)
            available.Remove(i);
    
        if (available.Count <= 0) return 0;
        return available[random.Next(0, available.Count)];
    }
    
    private string GetRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdifghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
        for (int i = 0; i < 20; i++)
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        return seed;
    }
    
    public Vector2 GetPositionFromPoint(Point point)
    {
        return new Vector2(-point.x, point.y);
    }
}
