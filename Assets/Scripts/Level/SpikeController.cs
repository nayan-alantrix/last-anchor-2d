using UnityEngine;
using DG.Tweening;

public class SpikeController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 0.5f;  // time to move to next block
    [SerializeField] private float intervalTime = 3f; // time before spike moves to next block

    private BlockSpawner spawner;
    private Tweener moveTween;
    private Tween timerTween;
    private bool isActive = false;

    public void Initialize(BlockSpawner blockSpawner)
    {
        spawner = blockSpawner;
    }

    public void OnGameStart(Transform spawnPoint)
    {
        isActive = true;
        transform.position = spawnPoint.position;
        gameObject.SetActive(true);
        ScheduleNextMove();
    }

    public void OnGamePause()
    {
        isActive = false;
        moveTween?.Pause();
        timerTween?.Pause();
        DOTween.Kill(gameObject);
    }

    public void OnGameResume()
    {
        isActive = true;
        moveTween?.Play();
        timerTween?.Play();
        ScheduleNextMove();
    }

    public void OnGameOver()
    {
        isActive = false;
        DOTween.Kill(gameObject);
        gameObject.SetActive(false);
    }

    public void OnMainMenu()
    {
        isActive = false;
        DOTween.Kill(gameObject);
        gameObject.SetActive(false);
    }

    private void ScheduleNextMove()
    {
        DOTween.Kill(gameObject);

        float remainingTime = intervalTime;

        // Update UI timer
        timerTween = DOTween.To(
            () => remainingTime,
            x =>
            {
                remainingTime = x;
                spawner.levelController.SpikeTimer(remainingTime);
            },
            0f,
            intervalTime
        )
        .SetEase(Ease.Linear)
        .SetId(gameObject);

        DOVirtual.DelayedCall(intervalTime, () =>
        {
            if (!isActive) return;

            spawner.levelController.SpikeTimer(0f);
            MoveToNextBlock();
        })
        .SetId(gameObject);
    }

    private void MoveToNextBlock()
    {
        Transform nextSpawn = spawner.GetNextSpikeSpawnPoint();

        if (nextSpawn == null)
        {
            // No next block yet, retry after a short delay
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (isActive) MoveToNextBlock();
            }).SetId(gameObject);
            return;
        }

        moveTween = transform.DOMove(nextSpawn.position, moveSpeed)
            .SetEase(Ease.InOutSine)
            .SetId(gameObject)
            .OnComplete(() =>
            {
                if (isActive) ScheduleNextMove();
            });
    }
}