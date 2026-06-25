using UnityEngine;
using DG.Tweening;

public class SpikeController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float intervalTime = 3f;

    private BlockSpawner spawner;
    private Tweener moveTween;
    private Tween timerTween;
    private Tween delayTween;
    private bool isActive = false;

    public void Initialize(BlockSpawner blockSpawner)
    {
        spawner = blockSpawner;
    }

    public void OnGameStart(Transform spawnPoint)
    {
        isActive = true;
        transform.position = spawnPoint.position;
        ScheduleNextMove();
    }

    public void OnGamePause()
    {
        isActive = false;
        // Pause all tweens by id — don't kill them
        timerTween?.Pause();
        delayTween?.Pause();
        moveTween?.Pause();
    }

    public void OnGameResume()
    {
        isActive = true;
        // Resume exactly where they left off
        timerTween?.Play();
        delayTween?.Play();
        moveTween?.Play();
    }

    public void OnGameOver()
    {
        isActive = false;
        DOTween.Kill(gameObject);
    }

    public void OnMainMenu()
    {
        isActive = false;
        DOTween.Kill(gameObject);
    }

    // Called when player moves to next block — reset timer
    public void OnPlayerMovedToNextBlock(Transform nextSpawnPoint)
    {
        if (!isActive) return;
        DOTween.Kill(gameObject);
        transform.position = nextSpawnPoint.position;
        ScheduleNextMove();
    }

    private void ScheduleNextMove()
    {
        DOTween.Kill(gameObject);

        float remainingTime = intervalTime;

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

        delayTween = DOVirtual.DelayedCall(intervalTime, () =>
        {
            if (!isActive) return;
            spawner.levelController.SpikeTimer(0f);
            MoveToNextBlock();
        })
        .SetId(gameObject);
    }

    private void MoveToNextBlock()
    {
        if (spawner == null) return;

        Transform nextSpawn = spawner.GetNextSpikeSpawnPoint();

        if (nextSpawn == null)
        {
            delayTween = DOVirtual.DelayedCall(0.5f, () =>
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