using System.Collections.Generic;
using UnityEngine;

public class BlockLevelGenerator : MonoBehaviour
{
    [Header("Prefabs - Diamonds")]
    [SerializeField] private GameObject d1DiamondPrefab;
    [SerializeField] private GameObject d3DiamondPrefab;

    [Header("Prefabs - Platforms")]
    [SerializeField] private GameObject normalPlatformPrefab;
    [SerializeField] private GameObject tPlatformPrefab;

    [Header("Prefabs - Obstacles")]
    [SerializeField] private GameObject singleSpikePrefab;

    [Header("Spawn Count")]
    [SerializeField] private int normalPlatformCount = 3;
    [SerializeField] private int minDiamonds = 2;
    [SerializeField] private int maxDiamonds = 4;
    [SerializeField] private int minSpikes = 0;
    [SerializeField] private int maxSpikes = 2;
    [SerializeField] [Range(0f, 1f)] private float spikeChancePerPlatform = 0.3f;

    [Header("Block Bounds (local offsets from block center)")]
    [SerializeField] private float blockHalfWidth = 2.2f;
    [SerializeField] private float blockHalfHeight = 4.5f;

    [Header("Wall Padding")]
    [SerializeField] private float horizontalPadding = 0.6f;
    [SerializeField] private float verticalPadding = 1.0f;

    [Header("Player Jump Constraints")]
    [SerializeField] private float maxJumpHeight = 4f;
    [SerializeField] private float maxJumpWidth = 3.5f;
    [SerializeField] private float minVerticalStep = 0.8f;
    [SerializeField] private float minHorizontalDist = 0.5f;

    [Header("Lock local Y position (top of block)")]
    [SerializeField] private float lockLocalY = 4.0f;

    [Header("Spike Placement")]
    [SerializeField] private float spikeHeightAbovePlatform = 0.4f; // sits just above platform surface

    [Header("Max placement attempts per object")]
    [SerializeField] private int maxPlacementAttempts = 40;

    private List<GameObject> spawnedItems = new List<GameObject>();
    private List<Vector3> platformPositions = new List<Vector3>();
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
        if (spawner == null)
            spawner = FindFirstObjectByType<BlockSpawner>();
    }

    public void GenerateLevel()
    {
        ClearLevel();
        levelComplete = false;
        collectedDiamonds = 0;
        totalDiamonds = 0;
        platformPositions.Clear();

        SpawnPlatformChain();
        SpawnSpikesOnPlatforms();
        SpawnDiamonds();

        if (totalDiamonds == 0)
            OpenNextBlockGate();
    }

    // =================================================================
    // PLATFORMS — chained vertically, each reachable from the previous
    // =================================================================
    private void SpawnPlatformChain()
    {
        int totalPlatforms = 1 + normalPlatformCount; // 1 T-platform (bottom) + N normal

        float xMin = -blockHalfWidth + horizontalPadding;
        float xMax =  blockHalfWidth - horizontalPadding;
        float yMin = -blockHalfHeight + verticalPadding;

        float lockWorldY = transform.position.y + lockLocalY;
        float topPlatformMaxY = lockWorldY - 0.5f;
        float topPlatformMinY = lockWorldY - maxJumpHeight;

        float usableHeight = (topPlatformMaxY - 0.5f) - yMin;
        float stepHint = Mathf.Clamp(usableHeight / (totalPlatforms - 1), minVerticalStep, maxJumpHeight - 0.3f);

        PlaceFirstPlatform(xMin, xMax, yMin, stepHint);
        PlaceChainedPlatforms(totalPlatforms, xMin, xMax, topPlatformMinY, topPlatformMaxY, stepHint);
    }

    private void PlaceFirstPlatform(float xMin, float xMax, float yMin, float stepHint)
    {
        float firstY = transform.position.y + yMin + Random.Range(0f, stepHint * 0.5f);
        float firstX = Random.Range(transform.position.x + xMin, transform.position.x + xMax);
        Vector3 firstPos = new Vector3(firstX, firstY, 0f);

        platformPositions.Add(firstPos);
        SpawnPrefabAt(tPlatformPrefab, firstPos);
    }

    private void PlaceChainedPlatforms(int totalPlatforms, float xMin, float xMax, float topMinY, float topMaxY, float stepHint)
    {
        for (int i = 1; i < totalPlatforms; i++)
        {
            Vector3 prevPos = platformPositions[i - 1];
            bool isLast = (i == totalPlatforms - 1);

            Vector3 placedPos;
            if (TryFindPlatformSpot(prevPos, isLast, xMin, xMax, topMinY, topMaxY, out placedPos))
            {
                platformPositions.Add(placedPos);
                SpawnPrefabAt(normalPlatformPrefab, placedPos);
            }
            else
            {
                Vector3 fallback = GetFallbackPlatformPos(prevPos, xMin, xMax, stepHint);
                platformPositions.Add(fallback);
                SpawnPrefabAt(normalPlatformPrefab, fallback);
                Debug.LogWarning($"[BlockLevelGenerator] Used fallback position for platform {i}");
            }
        }
    }

    private bool TryFindPlatformSpot(Vector3 prevPos, bool isLast, float xMin, float xMax, float topMinY, float topMaxY, out Vector3 result)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            float y;
            if (isLast)
            {
                float yLow = Mathf.Max(prevPos.y + minVerticalStep, topMinY);
                float yHigh = Mathf.Min(prevPos.y + maxJumpHeight - 0.2f, topMaxY);
                if (yLow > yHigh) yLow = yHigh - 0.1f;
                y = Random.Range(yLow, yHigh);
            }
            else
            {
                y = Random.Range(prevPos.y + minVerticalStep, prevPos.y + maxJumpHeight - 0.2f);
            }

            float x = Random.Range(transform.position.x + xMin, transform.position.x + xMax);
            Vector3 candidate = new Vector3(x, y, 0f);

            float horizDist = Mathf.Abs(candidate.x - prevPos.x);
            if (horizDist > maxJumpWidth || horizDist < minHorizontalDist) continue;
            if (!IsFarEnoughFrom(candidate, platformPositions, 1.2f)) continue;

            result = candidate;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private Vector3 GetFallbackPlatformPos(Vector3 prevPos, float xMin, float xMax, float stepHint)
    {
        float safeY = prevPos.y + Mathf.Clamp(stepHint, minVerticalStep, maxJumpHeight - 0.3f);
        float safeX = Mathf.Clamp(prevPos.x + Random.Range(-1f, 1f), transform.position.x + xMin, transform.position.x + xMax);
        return new Vector3(safeX, safeY, 0f);
    }

    // =================================================================
    // SPIKES — placed on top of a subset of platforms as obstacles
    // =================================================================
    private void SpawnSpikesOnPlatforms()
    {
        if (singleSpikePrefab == null) return;

        int spikeCount = Random.Range(minSpikes, maxSpikes + 1);
        if (spikeCount == 0) return;

        // Never place a spike on the very first (spawn) platform
        List<int> eligibleIndices = new List<int>();
        for (int i = 1; i < platformPositions.Count; i++)
            eligibleIndices.Add(i);

        Shuffle(eligibleIndices);

        int placed = 0;
        foreach (int index in eligibleIndices)
        {
            if (placed >= spikeCount) break;
            if (Random.value > spikeChancePerPlatform) continue;

            Vector3 platformPos = platformPositions[index];
            Vector3 spikePos = platformPos + new Vector3(0f, spikeHeightAbovePlatform, 0f);

            SpawnPrefabAt(singleSpikePrefab, spikePos);
            placed++;
        }
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // =================================================================
    // DIAMONDS — scattered across the block, avoiding platforms/spikes
    // =================================================================
    private void SpawnDiamonds()
    {
        int diamondCount = Random.Range(minDiamonds, maxDiamonds + 1);
        List<Vector3> occupiedPositions = new List<Vector3>(platformPositions);

        float xMin = transform.position.x - blockHalfWidth + horizontalPadding;
        float xMax = transform.position.x + blockHalfWidth - horizontalPadding;
        float yMin = transform.position.y - blockHalfHeight + verticalPadding;
        float yMax = transform.position.y + blockHalfHeight - verticalPadding;

        for (int i = 0; i < diamondCount; i++)
        {
            if (TryFindDiamondSpot(xMin, xMax, yMin, yMax, occupiedPositions, out Vector3 spot))
            {
                GameObject prefab = Random.value > 0.5f ? d1DiamondPrefab : d3DiamondPrefab;
                GameObject spawned = Instantiate(prefab, spot, Quaternion.identity, transform);
                spawnedItems.Add(spawned);
                occupiedPositions.Add(spot);

                DiamondCollectible diamond = spawned.GetComponent<DiamondCollectible>();
                if (diamond != null)
                {
                    totalDiamonds++;
                    diamond.onCollected += OnDiamondCollected;
                }
            }
            else
            {
                Debug.LogWarning("[BlockLevelGenerator] Could not place diamond.");
            }
        }
    }

    private bool TryFindDiamondSpot(float xMin, float xMax, float yMin, float yMax, List<Vector3> occupied, out Vector3 result)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector3 candidate = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0f);
            if (IsFarEnoughFrom(candidate, occupied, 0.8f))
            {
                result = candidate;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    // =================================================================
    // SHARED HELPERS
    // =================================================================
    private void SpawnPrefabAt(GameObject prefab, Vector3 worldPos)
    {
        if (prefab == null) return;
        GameObject spawned = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        spawnedItems.Add(spawned);
    }

    private bool IsFarEnoughFrom(Vector3 candidate, List<Vector3> existing, float minDist)
    {
        foreach (Vector3 pos in existing)
        {
            if (Vector3.Distance(candidate, pos) < minDist)
                return false;
        }
        return true;
    }

    private void OnDiamondCollected()
    {
        collectedDiamonds++;
        if (collectedDiamonds >= totalDiamonds && !levelComplete)
        {
            levelComplete = true;
            OpenNextBlockGate();
        }
    }

    private void OpenNextBlockGate()
    {
        BlockController nextBlock = spawner.GetNextBlockController();
        if (nextBlock != null)
            nextBlock.ReadyGate();
        else
            Debug.LogWarning("[BlockLevelGenerator] No next block found to open gate.");
    }

    private void ClearLevel()
    {
        foreach (GameObject obj in spawnedItems)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedItems.Clear();
    }
}