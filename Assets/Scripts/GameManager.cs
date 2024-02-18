using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

[Serializable]
public class GameManager : MonoBehaviour
{
    public Board board;
    public Sprite[] blocks;
    [Header("UI Elements")] public RectTransform gameBoardTransform;
    [Header("Prefabs")] public GameObject boardBlock;
    public int width = 5;
    public int height = 5;
    public GameBoard[,] gameBoard;

    private Random random;
    public int[] blockValues;

    private List<Cube> update;

    private Animator animator;
    public RuntimeAnimatorController anim1;
    public RuntimeAnimatorController anim2;

    private void Start()
    {
        string seed = GetRandomSeed();
        random = new Random(seed.GetHashCode());
        update = new List<Cube>();
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
            update.Remove(cube);
        }
    }

    public void InitializeBoard()
    {
        gameBoard = new GameBoard[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gameBoard[x, y] = new GameBoard(board.board[y].elements[x] ? - 1 : FillBlock(), new Point(x, y));
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
                int value = GetValue(point);
                if (value <= 0) continue;

                remove = new List<int>();
                while (isConnnected(point, true).Count > 0)
                {
                    value = GetValue(point);
                    if (!remove.Contains(value))
                        remove.Add(value);
                    SetValueAtPoint(point, newValue(ref remove));
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
                GameBoard board = GetBlockAtPoint(new Point(x, y));
                int value = board.value;
                if (value <= 0) continue;
                GameObject p = Instantiate(boardBlock, gameBoardTransform);
                Cube cube = p.GetComponent<Cube>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(-120 + (64 * x), -170 - (64 * y));
                value = blockValues[index];
                cube.Initialize(value,  new Point(x, y), blocks[value - 1]);
                board.SetCube(cube);
                index++;
                
                animator = cube.GetComponent<Animator>();
                if (cube.value == 1)
                {
                    animator.runtimeAnimatorController = anim1;
                }
                else
                {
                    animator.runtimeAnimatorController = anim2;
                }
            }
        }
    }

    public void ResetBlock(Cube cube)
    {
        cube.ResetPosition();
        cube.flipped = null;
        update.Add(cube);
        Debug.Log("Reset");
    }

    public void FlipBlocks(Point one, Point two)
    {
        if (GetValue(one) < 0) return;
        GameBoard pointOne = GetBlockAtPoint(one);
        Cube cubeOne = pointOne.GetCube();
        if (GetValue(two) > 0)
        {
            GameBoard pointTwo = GetBlockAtPoint(two);
            Cube cubeTwo = pointTwo.GetCube();
            pointOne.SetCube(cubeTwo);
            pointTwo.SetCube(cubeOne);
            cubeOne.flipped = cubeTwo;
            cubeTwo.flipped = cubeOne;
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
    
    private List<Point> isConnnected(Point point, bool main)
    {
        List<Point> connected = new List<Point>();
        int value = GetValue(point);
        Point[] directions =
        {
            Point.Up,
            Point.Down,
            Point.Left,
            Point.Right
        };
    
        foreach (var direction in directions)
        {
            List<Point> line = new List<Point>();
            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(point, Point.Mult(direction, i));
                if (GetValue(check) == value)
                {
                    line.Add(check);
                    same++;
                }
            }

            if (same > 1)
                AddPionts(ref connected, line);
        }

        for (int i = 0; i < 2; i++)
        {
            List<Point> line = new List<Point>();
            int same = 0;
            Point next = Point.Add(point, directions[i]);
            Point nextTwo = Point.Add(point, directions[i + 2]);
            Point[] check = { next, nextTwo };
            foreach (Point nextCheck in check)
            {
                if (GetValue(nextCheck) == value)
                {
                    line.Add(nextCheck);
                    same++;
                }
            }
            
            if (same > 1)
                AddPionts(ref connected, line);
        }

        for (int i = 0; i < 4; i++)
        {
            List<Point> square = new List<Point>();
            int same = 0;
            int next = i + 1;
            if (next >= 4)
                next -= 4;

            Point[] check = { Point.Add(point, directions[i]), Point.Add(point, directions[next]), Point.Add(point, Point.Add(directions[i], directions[next]))};
            foreach (Point nextCheck in check)
            {
                if (GetValue(nextCheck) == value)
                {
                    square.Add(nextCheck);
                    same++;
                }
            }

            if (same > 2)
                AddPionts(ref connected, square);
        }

        if (main)
        {
            for (int i = 0; i < connected.Count; i++)
                AddPionts(ref connected, isConnnected(connected[i], false));
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
        int value = 0;
        value = (random.Next(0, 100) / (100 / blocks.Length)) + 1;
        return value;
    }
    
    private int GetValue(Point point)
    {
        if (point.x < 0 || point.x >= width || point.y < 0 || point.y >= height) return -1;
        return gameBoard[point.x, point.y].value;
    }

    private void SetValueAtPoint(Point point, int value)
    {
        gameBoard[point.x, point.y].value = value;
    }

    private GameBoard GetBlockAtPoint(Point point)
    {
        return gameBoard[point.x, point.y];
    }
    
    private int newValue(ref List<int> remove)
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
        return seed;
    }
    
    public Vector2 GetPositionFromPoint(Point point)
    {
        return new Vector2(-point.x, point.y);
    }
}
