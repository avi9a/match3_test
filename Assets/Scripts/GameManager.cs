using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using SystemRandom = System.Random;

[Serializable]
public class GameManager : MonoBehaviour
{
    public Level level;
    public List<Level> levels;
    public int levelNumber;
    [Header("UI Elements")] public RectTransform gameBoardTransform;
    [Header("Prefabs")] public GameObject boardBlock;
    private int width = 5;
    private int height = 6;

    private List<Cube> update;
    private List<Cube> dead;

    private Animator animator;
    public List<GameObject> balloons;

    private int indexX;
    private int indexY;

    public bool start;

    private void Start()
    {
        if (PlayerPrefs.HasKey("StartGame"))
        {
            start = true;
            LoadData();
        }
        else
        {
            start = false;
            PlayerPrefs.SetInt("StartGame", 1);
            PlayerPrefs.Save();
        }

        foreach (var balloon in balloons)
        {
            balloon.transform.DOLocalMoveX(Random.Range(0f, 1f), Random.Range(1, 3)).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            balloon.transform.DOLocalMoveY(500f, Random.Range(5, 10)).SetLoops(-1, LoopType.Restart);
        }

        StartGame();
    }
    private void StartGame()
    {
        update = new List<Cube>();
        dead = new List<Cube>();
        InitializeBoard();
        InstantiateBoard();

        // Load
        if (start)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    Point point = new Point(x, y);
                    GameBoardCube block = GetBlockAtPoint(point);
                    block.value = PlayerPrefs.GetInt("Value"+block.index.x+block.index.y);

                    if (PlayerPrefs.HasKey("Value" + block.index.x + block.index.y))
                    {
                        level.gameBoard[x, y] = new GameBoardCube(block.value, new Point(x, y));
                    }
                    else
                    {
                        return;
                    }
                }
            }

            var cubes = gameBoardTransform.GetComponentsInChildren<Cube>();
            for (int i = 0; i < cubes.Length; i++)
            {
                if (PlayerPrefs.HasKey("cubeIndexX" + i))
                {
                    cubes[i].index.x = PlayerPrefs.GetInt("cubeIndexX" + i);
                    cubes[i].index.y = PlayerPrefs.GetInt("cubeIndexY" + i);
                    GameBoardCube block = GetBlockAtPoint(cubes[i].index);
                    cubes[i].rect.anchoredPosition = new Vector2(50 + (64 * cubes[i].index.x), -50 - (64 * cubes[i].index.y));
                    cubes[i].SetIndex(cubes[i].index);
                    block.SetCube(cubes[i]);
                    update.Add(cubes[i]);
                }
                else
                {
                    return;
                }
            }
        }
    }

    private void Update()
    {
        List<Cube> finishedUpdating = new List<Cube>();
        for (int i = 0; i < update.Count; i++)
        {
            Cube cube = update[i];
            if (cube != null && !cube.UpdateBlock()) finishedUpdating.Add(cube);
        }
    
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            Cube cube = finishedUpdating[i];
            indexX = cube.index.x;
            indexY = cube.index.y;
            
            List<Point> connected = IsConnnected(cube.index, true);
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
            }

            GravityOnBoard();
            update.Remove(cube);
        }
    }
    
    private IEnumerator DestroyAnimation(Cube cubePiece)
    {
        var cubeAnimator = cubePiece.GetComponent<Animator>();
        cubeAnimator.SetBool("Destroy", true);
        yield return new WaitForSeconds(1f);
        cubePiece.gameObject.SetActive(false);

        CompleteLevel();
    }

    private void GravityOnBoard()
    {
        StartCoroutine(WaitAndFall());
    }
    
    private IEnumerator WaitAndFall()
    {
        yield return new WaitForSeconds(0.8f);
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                Point point = new Point(x, y);
                GameBoardCube block = GetBlockAtPoint(point);
                int value = GetValueAtPoint(point);
                
                PlayerPrefs.SetInt("Value"+block.index.x+block.index.y, block.value);
                PlayerPrefs.Save();
                
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

    public void InitializeBoard()
    {
        level.gameBoard = new GameBoardCube[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                level.gameBoard[x, y] = new GameBoardCube(level.board.board[y].elements[x] ? 0 : 1, new Point(x, y));
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
                GameObject piece = Instantiate(boardBlock, gameBoardTransform);
                Cube cube = piece.GetComponent<Cube>();
                RectTransform rect = piece.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(50 + (64 * x), -50 - (64 * y));
                value = level.blockValues[index];
                cube.Initialize(value,  new Point(x, y), level.blocks[value - 1]);
                board.SetCube(cube);
                index++;
                animator = cube.GetComponent<Animator>();
                animator.runtimeAnimatorController = cube.value == 1 ? level.animatorWater : level.animatorFire;
            }
        }
    }

    public void ResetBlock(Cube cube)
    {
        cube.ResetPosition();
        update.Add(cube);
    }

    public void FlipBlocks(Point one, Point two)
    {
        GameBoardCube pointOne = GetBlockAtPoint(one);
        Cube cubeOne = pointOne.GetCube();
        GameBoardCube pointTwo = GetBlockAtPoint(two);
        Cube cubeTwo = pointTwo.GetCube();
        pointOne.SetCube(cubeTwo);
        pointTwo.SetCube(cubeOne);
        var cubeOneIndex = 0;
        if (cubeOneIndex != null && cubeOne != null)
            cubeOneIndex = cubeOne.transform.GetSiblingIndex();
        var cubeTwoIndex = 0;
        if (cubeTwoIndex != 0 && cubeTwo != null)
            cubeTwoIndex = cubeTwo.transform.GetSiblingIndex();
        update.Add(cubeOne);
        update.Add(cubeTwo);
        if (cubeOneIndex != null && cubeTwo != null) 
            cubeTwo.transform.SetSiblingIndex(cubeOneIndex);
        if (cubeTwoIndex != 0 && cubeOne != null) 
            cubeOne.transform.SetSiblingIndex(cubeTwoIndex);
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
            {
                AddPoints(ref connected, line);
            }
        }
        
        for (int i = 0; i < 4; i++) //check for a 2x2
        {
            List<Point> square = new List<Point>();
            int same = 0;
            int next = i + 1;
            if (next >= 4)
                next -= 4;
        
            Point[] check = { Point.Add(point, directions[i]), Point.Add(point, directions[next]), Point.Add(point, Point.Add(directions[i], directions[next]))};
            foreach (Point nextCheck in check)
            {
                if (GetValueAtPoint(nextCheck) == value)
                {
                    square.Add(nextCheck);
                    same++;
                }
            }
        
            if (same > 2)
                AddPoints(ref connected, square);
        }

        if (main) //check for other matches along the current match
        {
            for (int i = 0; i < connected.Count; i++)
                AddPoints(ref connected, IsConnnected(connected[i], false));
        }

        if (connected.Count > 0)
            connected.Add(point);
        return connected;
    }
    
    private void AddPoints(ref List<Point> points, List<Point> add)
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
    
    private int GetValueAtPoint(Point point)
    {
        if (point.x < 0 || point.x >= width || point.y < 0 || point.y >= height) return -1; // return a hole
        return level.gameBoard[point.x, point.y].value;
    }

    private void SetValueAtPoint(Point point, int value)
    {
        level.gameBoard[point.x, point.y].value = value;
    }

    private GameBoardCube GetBlockAtPoint(Point point)
    {
        return level.gameBoard[point.x, point.y];
    }
    
    private string GetRandomSeed()
    {
        string seed = "";
        return seed;
    }
    
    public Vector2 GetPositionFromPoint(Point point)
    {
        return new Vector2(50 + (64 * point.x), -50 - (64 * point.y));
    }
    
    public void SaveData()
    {
        PlayerPrefs.SetInt("Level", levelNumber);
        PlayerPrefs.SetString("LevelName", level.name);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        levelNumber =  PlayerPrefs.GetInt("Level");
        if (levelNumber == 0)
        {
            levelNumber = 1;
        }
        if (PlayerPrefs.HasKey("LevelName"))
        {
            var levelName = PlayerPrefs.GetString("LevelName");
            level = Resources.Load<Level>(levelName);
        }
    }
    
    public void RestartLevel()
    {
        start = false;
        var blocks = gameBoardTransform.GetComponentsInChildren<Cube>();
        foreach (var block in blocks)
        {
            block.transform.gameObject.SetActive(false);
            Destroy(block);
        }
        level = levels[levelNumber - 1];
        SaveData();
        // SceneManager.LoadScene("Main");
        StartGame();
    }
    
    public void NextLevel()
    {
        start = false;
        var blocks = gameBoardTransform.GetComponentsInChildren<Cube>();
        foreach (var block in blocks)
        {
            block.transform.gameObject.SetActive(false);
            Destroy(block);
        }
        levelNumber += 1;
        if (levelNumber > levels.Count)
        {
            levelNumber = 1;
        }
        level = levels[levelNumber - 1];
        SaveData();
        // SceneManager.LoadScene("Main");
        StartGame();
    }

    public void CompleteLevel()
    {
        start = false;
        var blocks = gameBoardTransform.GetComponentsInChildren<Cube>();
        if (blocks.Length <= 0)
        {
            levelNumber += 1;
            if (levelNumber > levels.Count)
            {
                levelNumber = 1;
            }
            level = levels[levelNumber - 1];
            SaveData();
            // SceneManager.LoadScene("Main");
            StartGame();
        }
    }
}
