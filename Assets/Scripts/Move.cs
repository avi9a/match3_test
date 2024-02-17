using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public static Move instance;
    private GameManager gameManager;

    private Cube cube;
    private Point newIndex;
    private Vector2 mauseStart;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    private void Update()
    {
        if (cube != null)
        {
            Vector2 dir = (Vector2)Input.mousePosition - mauseStart;
            Vector2 normDir = dir.normalized;
            Vector2 adsDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

            newIndex = Point.Clone(cube.index);
            Point add = Point.Zero;
            if (dir.magnitude > 32)
            {
                if (adsDir.x > adsDir.y)
                    add = (new Point((normDir.x > 0) ? 1 : -1, 0));
                else if (adsDir.y > adsDir.x)
                    add = (new Point(0, (normDir.y > 0) ? 1 : -1));
            }
            newIndex.Add(add);

            Vector2 position = gameManager.GetPositionFromPoint(cube.index);
            if (!newIndex.Equals(cube.index))
                position += Point.Mult(new Point(add.x, -add.y), 1).ToVector();
            cube.MovePosition(position);
        }
    }

    public void MoveBlock(Cube piece)
    {
        if (cube != null) return;
        cube = piece;
        mauseStart = Input.mousePosition;
    }

    public void DropBlock()
    {
        if (cube == null) return;
        cube = null;
    }
}
