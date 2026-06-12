using System.Collections.Generic;
using UnityEngine;

public class BlockLevelGenerator : MonoBehaviour
{
    [Header("Pattern Pool")]
    public List<LevelPattern> patternPool;

    private List<GameObject> spawnedItems = new List<GameObject>();
    private int totalDiamonds = 0;
    private int collectedDiamonds = 0;
    private bool levelComplete = false;

    private BlockSpawner spawner;

    public void Initialize(BlockSpawner blockSpawner)
    {
        spawner = blockSpawner;
    }
    void Start()
    {
        if(spawner == null) spawner = FindFirstObjectByType<BlockSpawner>();
    }

    public void GenerateLevel()
    {
        ClearLevel();
        levelComplete = false;
        collectedDiamonds = 0;
        totalDiamonds = 0;

        if (patternPool == null || patternPool.Count == 0)
        {
            Debug.LogWarning("No patterns assigned: " + gameObject.name);
            OpenNextBlockGate();
            return;
        }

        LevelPattern pattern = patternPool[Random.Range(0, patternPool.Count)];

        foreach (PatternItem item in pattern.items)
        {
            if (item.prefab == null) continue;

            Vector3 worldPos = transform.position + item.localPosition;
            GameObject spawned = Instantiate(item.prefab, worldPos, Quaternion.identity, transform);
            spawnedItems.Add(spawned);

            DiamondCollectible diamond = spawned.GetComponent<DiamondCollectible>();
            if (diamond != null)
            {
                totalDiamonds++;
                diamond.onCollected += OnDiamondCollected;
            }
        }

        if (totalDiamonds == 0)
            OpenNextBlockGate();
    }

    void OnDiamondCollected()
    {
        collectedDiamonds++;

        if (collectedDiamonds >= totalDiamonds && !levelComplete)
        {
            levelComplete = true;
            OpenNextBlockGate();
        }
    }

    // Opens the NEXT block's bottom gate (lockedObject + trigger)
    void OpenNextBlockGate()
    {
        BlockController nextBlock = spawner.GetNextBlockController();
        if (nextBlock != null)
            nextBlock.ReadyGate();
        else
            Debug.LogWarning("No next block found to open gate.");
    }

    public void ClearLevel()
    {
        foreach (GameObject obj in spawnedItems)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedItems.Clear();
    }
}