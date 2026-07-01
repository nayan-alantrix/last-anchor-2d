using UnityEngine;
using DG.Tweening;

public class SpikeController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float catchUpSpeed = 0.1f;

    [Header("Timer Difficulty")]
    [SerializeField] private float startInterval = 8f;    // timer at block 1
    [SerializeField] private float minInterval = 2f;      // hardest timer ever
    [SerializeField] private float difficultyRate = 0.3f; // how fast it gets harder per block

    private BlockSpawner spawner;
    private Tweener moveTween;
    private Tween timerTween;
    private Tween delayTween;
    private bool isActive = false;
    private bool isMoving = false;
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

    public void OnPlayerMovedToNextBlock()
    {
        if (!isActive) return;

        int myTransition = ++transitionId;
        KillAll();
        isMoving = false;

        Transform playerBlockSpawn = spawner.GetCurrentPlayerSpikeSpawnPoint();
        MoveToTargetPoint(playerBlockSpawn, catchUpSpeed, myTransition);
    }

    // Timer shrinks every block — more competitive as game goes on
    private float GetCurrentInterval()
    {
        int blockIndex = spawner.CurrentBlockIndex;
        float interval = startInterval - (difficultyRate * blockIndex);
        return Mathf.Max(interval, minInterval);
    }

    private void StartTimer()
    {
        int myTransition = ++transitionId;
        KillAll();
        isMoving = false;

        float intervalTime = GetCurrentInterval();
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

            Transform nextSpawn = spawner.GetNextSpikeSpawnPoint();
            MoveToTargetPoint(nextSpawn, moveSpeed, myTransition);
        });
    }

    private void MoveToTargetPoint(Transform target, float speed, int myTransition)
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