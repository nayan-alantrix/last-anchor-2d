using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockSpawner : MonoBehaviour
{
    [Header("Spike")]
    private SpikeController spike;
    [Header("References")]
    public BlockController blockPrefab;
    public Transform cameraTransform;

    [Header("Settings")]
    public float blockHeight = 10f;
    public float despawnDistance = 11f;
    public int initialBlockCount = 4;

    [Header("Camera Move Settings")]
    public float cameraMoveSpeed = 1f;
    public Ease cameraEase = Ease.InOutSine;

    private List<GameObject> blocks = new List<GameObject>();
    private int currentBlockIndex = 0;
    private bool isMoving = false;

    public LevelController levelController { get; private set; }

    public void Initialize(LevelController controller, SpikeController spike)
    {
        levelController = controller;
        this.spike = spike;
        spike.Initialize(this);
    }

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    public BlockData OnGameStart()
    {
        cameraTransform.position = new Vector3(transform.position.x, 0f, cameraTransform.position.z);
        ClearAllBlocks();
        currentBlockIndex = 0;
        isMoving = false;
        SpawnBLocks();

        BlockController firstBlock = blocks[0].GetComponent<BlockController>();
        return new BlockData
        {
            playerSpawnPoint = firstBlock.playerSpawnPointTransform,
            spikeSpawnPoint = firstBlock.spikeSpawnPointTransform
        };
    }

    public void OnGamePause()
    {   
    }
    public void OnGameResume()
    {
    }

    public void OnGameOver()
    {
        isMoving = false;
    }

    public void OnMainMenuClicked()
    {
        currentBlockIndex = 0;
        isMoving = false;
        ClearAllBlocks();
    }

    private void SpawnBLocks()
    {
        for (int i = 0; i < initialBlockCount; i++)
        {
            SpawnBlockAt(i * blockHeight, i);
        }
    }

    private void ClearAllBlocks()
    {
        foreach (GameObject block in blocks)
        {
            if (block != null)
                Destroy(block);
        }
        blocks.Clear();
    }

    void Update()
    {
        CheckAndRecycleBlocks();
    }

    public void MoveToNextBlock()
    {
        if (isMoving) return;

        int nextIndex = currentBlockIndex + 1;

        if (nextIndex >= blocks.Count)
        {
            Debug.LogWarning("No next block available.");
            return;
        }

        currentBlockIndex = nextIndex;

        // Unlock next block
        BlockController nextController = blocks[currentBlockIndex].GetComponent<BlockController>();
        if (nextController != null)
            nextController.Unlock();

        // Generate level in next block
        BlockLevelGenerator nextGenerator = blocks[currentBlockIndex].GetComponent<BlockLevelGenerator>();
        if (nextGenerator != null)
            nextGenerator.GenerateLevel();

        float targetY = blocks[currentBlockIndex].transform.position.y;
        Vector3 targetPos = new Vector3(cameraTransform.position.x, targetY, cameraTransform.position.z);

        isMoving = true;

        cameraTransform.DOMove(targetPos, cameraMoveSpeed)
            .SetEase(cameraEase)
            .OnComplete(() =>
            {
                isMoving = false;
                CheckAndRecycleBlocks();
            });
    }
    public Transform GetNextSpikeSpawnPoint()
    {
        int nextIndex = currentBlockIndex + 1;
        if (nextIndex >= blocks.Count) return null;

        BlockController nextBlock = blocks[nextIndex].GetComponent<BlockController>();
        return nextBlock?.spikeSpawnPointTransform;
    }

    void CheckAndRecycleBlocks()
    {
        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            GameObject block = blocks[i];
            if (block == null) { blocks.RemoveAt(i); continue; }

            float distBelowCamera = cameraTransform.position.y - block.transform.position.y;

            if (distBelowCamera > despawnDistance)
            {
                Destroy(block);
                blocks.RemoveAt(i);

                if (i <= currentBlockIndex)
                    currentBlockIndex = Mathf.Max(0, currentBlockIndex - 1);

                float newY = GetHighestBlockY() + blockHeight;
                SpawnBlockAt(newY, spawnIndex: 99);
            }
        }
    }

    float GetHighestBlockY()
    {
        float highest = float.MinValue;
        foreach (GameObject b in blocks)
        {
            if (b == null) continue;
            if (b.transform.position.y > highest)
                highest = b.transform.position.y;
        }
        return highest;
    }

    void SpawnBlockAt(float yPosition, int spawnIndex)
    {
        Vector3 pos = new Vector3(transform.position.x, yPosition, transform.position.z);
        BlockController newBlock = Instantiate<BlockController>(blockPrefab, pos, Quaternion.identity, transform);
        blocks.Add(newBlock.gameObject);
        newBlock.Initialize(this);
        if (newBlock != null)
        {
            if (spawnIndex == 0)
                newBlock.SetActive();
            else
                newBlock.SetLocked();
        }

        // Generate level for 1st block immediately, rest generated on MoveToNextBlock
        if (spawnIndex == 0)
        {
            BlockLevelGenerator generator = newBlock.GetComponent<BlockLevelGenerator>();
            if (generator != null)
                generator.GenerateLevel();
        }
    }
    public BlockController GetNextBlockController()
    {
        int nextIndex = currentBlockIndex + 1;
        if (nextIndex >= blocks.Count) return null;
        return blocks[nextIndex].GetComponent<BlockController>();
    }
}

public struct BlockData
{
    public Transform spikeSpawnPoint;
    public Transform playerSpawnPoint;
}