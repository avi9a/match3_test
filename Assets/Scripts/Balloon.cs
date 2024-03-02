using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Balloon : MonoBehaviour
{
    public List<GameObject> balloons;
    void Start()
    {
        foreach (var balloon in balloons)
        {
            balloon.transform.DOLocalMoveX(Random.Range(0f, 1f), Random.Range(1, 3)).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            balloon.transform.DOLocalMoveY(500f, Random.Range(5, 10)).SetLoops(-1, LoopType.Restart);
        }
    }
}
