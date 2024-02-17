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
    [Header("UI Elements")] public RectTransform GameBoardTransform;
    [Header("Prefabs")] public GameObject boardBlock;
    public int width = 5;
    public int height = 5;
    public GameBoard[,] gameBoard;

    private Random random;

    public Block gBlock;
    
    public List<Sprite> replaceSprites;

    private void Start()
    {
        string seed = GetRandomSeed();
        random = new Random(seed.GetHashCode());
        InitializeBoard();
        VerifyBoard();
        InstantiateBoard();
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

    public void InstantiateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                int value = gameBoard[x, y].value;
                if (value <= 0) continue;
                GameObject p = Instantiate(boardBlock, GameBoardTransform);
                Cube cube = p.GetComponent<Cube>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(-120 + (64 * x), -170 - (64 * y));
                cube.Initialize(value,  new Point(x, y), blocks[value - 1]);
            }
        }
    }

    int newValue(ref List<int> remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < blocks.Length; i++)
            available.Add(i + 1);
        foreach (var i in remove)
            available.Remove(i);
    
        if (available.Count <= 0) return 0;
        return available[random.Next(0, available.Count)];
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

    private void SetValueAtPoint(Point point, int value)
    {
        gameBoard[point.x, point.y].value = value;
    }

    private string GetRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDIFJabcdifj123456789";
        for (int i = 0; i < 20; i++)
            seed += acceptableChars[UnityEngine.Random.Range(0, acceptableChars.Length)];
        return seed;
    }

    public Vector2 GetPositionFromPoint(Point point)
    {
        return new Vector2(-120 + (64 * point.x), -170 - (64 * point.y));
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

    private int GetValue(Point point)
    {
        if (point.x < 0 || point.x >= width || point.y < 0 || point.y >= height) return -1;
        return gameBoard[point.x, point.y].value;
    }

    private int FillBlock()
    {
        int value = 0;
        value = (random.Next(0, 100) / (100 / blocks.Length)) + 1;
        return value;
    }
}
