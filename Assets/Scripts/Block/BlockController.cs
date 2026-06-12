using UnityEngine;
using System.Collections;
using DG.Tweening;

public class BlockController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BlockLevelGenerator levelGenerator;
    [SerializeField] public GameObject lockedObject;
    [SerializeField] public GameObject blockedObject;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform spikeSpawnPoint;

    [Header("Settings")]
    public float moveTime = 0.5f;

    public Transform playerSpawnPointTransform => playerSpawnPoint;
    public Transform spikeSpawnPointTransform => spikeSpawnPoint;

    private BlockSpawner spawner;
    private bool gateReady = false;

    // Cache components
    private SpriteRenderer lockedSprite;

    void Awake()
    {
        lockedSprite = lockedObject.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (spawner == null)
            spawner = FindFirstObjectByType<BlockSpawner>();
    }

    public void Initialize(BlockSpawner blockSpawner)
    {
        spawner = blockSpawner;
        if (levelGenerator != null)
            levelGenerator.Initialize(blockSpawner);
    }

    public void SetLocked()
    {
        lockedObject.SetActive(true);
        blockedObject.SetActive(false);
        gateReady = false;
    }

    public void SetActive()
    {
        lockedObject.SetActive(false);
        blockedObject.SetActive(true);
        gateReady = false;
    }

    public void Unlock()
    {
        blockedObject.SetActive(false);
        gateReady = false;
    }

    public void ReadyGate()
    {
        gateReady = true;
        lockedObject.SetActive(true);
        lockedSprite.color = Color.green;
        blockedObject.SetActive(false);
    }

    public void OnGateTouched()
    {
        if (!gateReady) return;

        lockedObject.SetActive(false);
        spawner.levelController.PlayerForcedGrounded(playerSpawnPointTransform, moveTime);
        spawner.MoveToNextBlock();

        // Replace coroutine with DOTween delay — consistent with rest of codebase
        DOVirtual.DelayedCall(moveTime, () =>
        {
            blockedObject.SetActive(true);
            gateReady = false;
            lockedSprite.color = Color.white;
        });
    }
}