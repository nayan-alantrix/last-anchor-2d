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
    [SerializeField] private bool isMoving = false;

    private int transitionId = 0;

    public void Initialize(BlockSpawner blockSpawner)
    {
        spawner = blockSpawner;
    }

    public void OnGameStart(Transform spawnPoint)
    {
        isActive = true;
        isMoving = false;
        transitionId = 0;
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

    // Player moved to next block — spike catches up to player's current block
    public void OnPlayerMovedToNextBlock()
    {
        if (!isActive) return;

        int myTransition = ++transitionId;
        KillAll();
        isMoving = false;

        // Move to player's current block (syncs spikeBlockIndex = currentBlockIndex)
        Transform playerBlockSpawn = spawner.GetCurrentPlayerSpikeSpawnPoint();
        MoveToTargetPoint(playerBlockSpawn, catchUpSpeed, myTransition, isCatchUp: true);
    }

    private void StartTimer()
    {
        int myTransition = ++transitionId;
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
        )
        .SetEase(Ease.Linear);

        delayTween = DOVirtual.DelayedCall(intervalTime, () =>
        {
            if (!isActive) return;
            if (myTransition != transitionId) return;

            spawner.levelController.SpikeTimer(0f);

            // Timer expired — spike advances to NEXT block on its own
            Transform nextSpawn = spawner.GetNextSpikeSpawnPoint();
            MoveToTargetPoint(nextSpawn, moveSpeed, myTransition, isCatchUp: false);
        });
    }

    private void MoveToTargetPoint(Transform target, float speed, int myTransition, bool isCatchUp)
    {
        if (myTransition != transitionId) return;

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
                if (myTransition != transitionId) return;

                isMoving = false;
                // After any move — always start fresh timer
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