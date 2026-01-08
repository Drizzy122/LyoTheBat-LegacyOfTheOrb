using UnityEngine;
using DG.Tweening;
public class RotateObject : MonoBehaviour
{
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float cycleDuration = 1.5f;

    void Start()
    {
        float randomSpeed = cycleDuration * Random.Range(0.8f, 1.2f);
        float randomDelay = Random.Range(0f, 1f);
        
        transform.DORotate(new Vector3(0, 360, 0), randomSpeed * 2, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear)
            .SetDelay(randomDelay)
            .SetLink(gameObject);
        
        transform.DOLocalMoveY(floatHeight, randomSpeed).SetRelative(true)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetDelay(randomDelay)
            .SetLink(gameObject);
    }
}