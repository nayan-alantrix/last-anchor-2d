using UnityEngine;
using DG.Tweening;

public class SpikeController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float catchUpSpeed = 0.1f;
    [SerializeField] private float intervalTime = 3f;

    private BlockSpawner spawner;
    private Tweener moveTween;
    private Tween timerTween;
    private Tween delayTween;
    private bool isActive = false;
    private bool isMoving = false;

    public void Initialize(BlockSpawner blockSpawner)
    {
        spawner = blockSpawner;
    }

    public void OnGameStart(Transform spawnPoint)
    {
        isActive = true;
        isMoving = false;
        transform.position = spawnPoint.position;
        StartTimer();
    }

    public void OnGamePause()
    {
        isActive = false;
        timerTween?.Pause();
        delayTween?.Pause();
        moveTween?.Pause();
    }

    public void OnGameResume()
    {
        isActive = true;
        timerTween?.Play();
        delayTween?.Play();
        moveTween?.Play();
    }

    public void OnGameOver()
    {
        isActive = false;
        KillAll();
    }

    public void OnMainMenu()
    {
        isActive = false;
        KillAll();
    }

    // Called when player moves to next block
    public void OnPlayerMovedToNextBlock()
    {
        if (!isActive) return;
        KillAll();
        isMoving = false;
        MoveToTargetPoint(spawner.GetCurrentSpikeSpawnPoint(), catchUpSpeed);
    }

    private void StartTimer()
    {
        KillAll();
        isMoving = false;
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
        ).SetEase(Ease.Linear);

        delayTween = DOVirtual.DelayedCall(intervalTime, () =>
        {
            if (!isActive) return;
            spawner.levelController.SpikeTimer(0f);
            MoveToNextSpikePoint();
        });
    }

    private void MoveToNextSpikePoint()
    {
        if (spawner == null) return;
        Transform nextSpawn = spawner.GetNextSpikeSpawnPoint();
        MoveToTargetPoint(nextSpawn, moveSpeed);
    }

    private void MoveToTargetPoint(Transform target, float speed)
    {
        // If target not ready — start timer anyway, no stuck state
        if (target == null)
        {
            StartTimer();
            return;
        }

        isMoving = true;

        moveTween = transform.DOMove(target.position, speed)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                if (!isActive) return;
                isMoving = false;
                StartTimer();
            });
    }

    private void KillAll()
    {
        timerTween?.Kill();
        timerTween = null;
        delayTween?.Kill();
        delayTween = null;
        moveTween?.Kill();
        moveTween = null;
    }
}