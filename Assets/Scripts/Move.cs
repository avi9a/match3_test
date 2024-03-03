using UnityEngine;

public class Move : MonoBehaviour
{
    public static Move Instance;
    private GameManager gameManager;

    private Cube movingCube;
    private Point newIndex;
    private Vector2 mouseStart;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    private void Update()
    {
        if (movingCube != null)
        {
            Vector2 dir = (Vector2)Input.mousePosition - mouseStart;
            Vector2 normDir = dir.normalized;
            Vector2 absDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
    
            newIndex = Point.Clone(movingCube.index);
            Point add = Point.Zero;
            if (dir.magnitude > 32)
            {
                if (absDir.x > absDir.y)
                    add = new Point((normDir.x > 0) ? 1 : -1, 0);
                else if (absDir.y > absDir.x)
                    add = new Point(0, (normDir.y > 0) ? -1 : 1);
            }
            newIndex.Add(add);
    
            Vector2 position = gameManager.GetPositionFromPoint(movingCube.index);
            if (!newIndex.Equals(movingCube.index))
                position += Point.Mult(new Point(add.x, -add.y), 16).ToVector();
            movingCube.MovePositionTo(position);
        }
    }

    public void MoveBlock(Cube piece)
    {
        if (movingCube != null) return;
        movingCube = piece;
        mouseStart = Input.mousePosition;
    }

    public void DropBlock()
    {
        if (movingCube == null) return;
        if (!newIndex.Equals(movingCube.index))
        {
            gameManager.FlipBlocks(movingCube.index, newIndex);
        }
        else 
            gameManager.ResetBlock(movingCube);
        movingCube = null;
        SaveData();
    }

    private void SaveData()
    {
        var cubes = gameManager.gameBoardTransform.GetComponentsInChildren<Cube>();
        for (int i = 0; i < cubes.Length; i++)
        {
            Point cubeIndex = cubes[i].index;
            PlayerPrefs.SetInt("cubeIndexX"+i, cubeIndex.x);
            PlayerPrefs.SetInt("cubeIndexY"+i, cubeIndex.y);
            PlayerPrefs.Save();
        }
    }
}
