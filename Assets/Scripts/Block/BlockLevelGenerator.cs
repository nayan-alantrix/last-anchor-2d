using System.Collections.Generic;
using UnityEngine;

public class BlockLevelGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject d1DiamondPrefab;
    [SerializeField] private GameObject d3DiamondPrefab;
    [SerializeField] private GameObject normalPlatformPrefab;
    [SerializeField] private GameObject tPlatformPrefab;

    [Header("Spawn Count")]
    [SerializeField] private int normalPlatformCount = 3;
    [SerializeField] private int minDiamonds = 2;
    [SerializeField] private int maxDiamonds = 4;

    [Header("Block Bounds (local offsets from block center)")]
    [SerializeField] private float blockHalfWidth = 2.2f;
    [SerializeField] private float blockHalfHeight = 4.5f;

    [Header("Wall Padding")]
    [SerializeField] private float horizontalPadding = 0.6f;
    [SerializeField] private float verticalPadding = 1.0f;

    [Header("Player Jump Constraints")]
    [SerializeField] private float maxJumpHeight = 4f;       // max vertical distance player can jump
    [SerializeField] private float maxJumpWidth = 3.5f;      // max horizontal distance player can cover
    [SerializeField] private float minVerticalStep = 0.8f;   // min vertical gap between platforms
    [SerializeField] private float minHorizontalDist = 0.5f; // min horizontal gap between platforms

    [Header("Lock local Y position (top of block)")]
    [SerializeField] private float lockLocalY = 4.0f;        // Y offset of the lock from block center

    [Header("Max placement attempts per object")]
    [SerializeField] private int maxPlacementAttempts = 40;

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
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<BlockSpawner>();
        }
    }

    public void GenerateLevel()
    {
        ClearLevel();
        levelComplete = false;
        collectedDiamonds = 0;
        totalDiamonds = 0;

        SpawnPlatformChain();
        SpawnDiamonds();

        if (totalDiamonds == 0)
            OpenNextBlockGate();
    }

    // ---------------------------------------------------------------
    // Platform chain: each platform reachable from the one below it,
    // and the top platform within jump reach of the lock
    // ---------------------------------------------------------------
    private void SpawnPlatformChain()
    {
        int totalPlatforms = 1 + normalPlatformCount; // 1 T-platform + 3 normal

        float xMin = -blockHalfWidth  + horizontalPadding;
        float xMax =  blockHalfWidth  - horizontalPadding;

        // Bottom of usable area (player spawn is near bottom)
        float yMin = -blockHalfHeight + verticalPadding;

        // Lock world Y — top platform must be within jump reach of this
        float lockWorldY = transform.position.y + lockLocalY;

        // Max Y the top platform can sit at so player can reach the lock
        float topPlatformMaxY = lockWorldY - 0.5f;              // just below lock
        float topPlatformMinY = lockWorldY - maxJumpHeight;     // must be reachable

        // We'll build the chain bottom-up
        // Divide the vertical space evenly as a starting guide
        float usableHeight = (topPlatformMaxY - 0.5f) - yMin;
        float stepHint = usableHeight / (totalPlatforms - 1);
        // Clamp step hint so it's always jumpable
        stepHint = Mathf.Clamp(stepHint, minVerticalStep, maxJumpHeight - 0.3f);

        List<Vector3> platformPositions = new List<Vector3>();

        // --- Place platform 0 (T-platform, bottommost) ---
        float firstY = transform.position.y + yMin + Random.Range(0f, stepHint * 0.5f);
        float firstX = Random.Range(transform.position.x + xMin, transform.position.x + xMax);
        Vector3 firstPos = new Vector3(firstX, firstY, 0f);
        platformPositions.Add(firstPos);
        SpawnPlatformAt(tPlatformPrefab, firstPos);

        // --- Place remaining normal platforms chained upward ---
        for (int i = 1; i < totalPlatforms; i++)
        {
            Vector3 prevPos = platformPositions[i - 1];
            bool isLast = (i == totalPlatforms - 1);

            bool placed = false;
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                float y, x;

                if (isLast)
                {
                    // Top platform: must be within jump reach of lock AND reachable from previous
                    float yLow  = Mathf.Max(prevPos.y + minVerticalStep, topPlatformMinY);
                    float yHigh = Mathf.Min(prevPos.y + maxJumpHeight - 0.2f, topPlatformMaxY);
                    if (yLow > yHigh) yLow = yHigh - 0.1f; // fallback
                    y = Random.Range(yLow, yHigh);
                }
                else
                {
                    y = Random.Range(prevPos.y + minVerticalStep, prevPos.y + maxJumpHeight - 0.2f);
                }

                x = Random.Range(transform.position.x + xMin, transform.position.x + xMax);

                Vector3 candidate = new Vector3(x, y, 0f);

                // Check horizontal distance is jumpable from previous
                float horizDist = Mathf.Abs(candidate.x - prevPos.x);
                if (horizDist > maxJumpWidth) continue;
                if (horizDist < minHorizontalDist) continue;

                // Check not overlapping any existing platform
                if (!IsFarEnoughFromPlatforms(candidate, platformPositions)) continue;

                platformPositions.Add(candidate);
                SpawnPlatformAt(normalPlatformPrefab, candidate);
                placed = true;
                break;
            }

            if (!placed)
            {
                // Fallback: place directly above previous within safe range
                float safeY = prevPos.y + Mathf.Clamp(stepHint, minVerticalStep, maxJumpHeight - 0.3f);
                float safeX = Mathf.Clamp(prevPos.x + Random.Range(-1f, 1f), transform.position.x + xMin,transform.position.x + xMax);
                Vector3 fallback = new Vector3(safeX, safeY, 0f);
                platformPositions.Add(fallback);
                SpawnPlatformAt(normalPlatformPrefab, fallback);
                Debug.LogWarning($"[BlockLevelGenerator] Used fallback position for platform {i}");
            }
        }
    }

    private void SpawnPlatformAt(GameObject prefab, Vector3 worldPos)
    {
        if (prefab == null) return;
        GameObject spawned = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        spawnedItems.Add(spawned);
    }

    private bool IsFarEnoughFromPlatforms(Vector3 candidate, List<Vector3> existing)
    {
        foreach (Vector3 pos in existing)
        {
            if (Vector3.Distance(candidate, pos) < 1.2f)
                return false;
        }
        return true;
    }

    // ---------------------------------------------------------------
    // Diamonds: scatter randomly across the block
    // ---------------------------------------------------------------
    private void SpawnDiamonds()
    {
        int diamondCount = Random.Range(minDiamonds, maxDiamonds + 1);
        List<Vector3> occupiedPositions = new List<Vector3>();

        float xMin = transform.position.x - blockHalfWidth  + horizontalPadding;
        float xMax = transform.position.x + blockHalfWidth  - horizontalPadding;
        float yMin = transform.position.y - blockHalfHeight + verticalPadding;
        float yMax = transform.position.y + blockHalfHeight - verticalPadding;

        for (int i = 0; i < diamondCount; i++)
        {
            bool found = false;
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                Vector3 candidate = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0f);

                if (IsFarEnoughFromAll(candidate, occupiedPositions, 0.8f))
                {
                    GameObject prefab = Random.value > 0.5f ? d1DiamondPrefab : d3DiamondPrefab;
                    GameObject spawned = Instantiate(prefab, candidate, Quaternion.identity, transform);
                    spawnedItems.Add(spawned);
                    occupiedPositions.Add(candidate);

                    DiamondCollectible diamond = spawned.GetComponent<DiamondCollectible>();
                    if (diamond != null)
                    {
                        totalDiamonds++;
                        diamond.onCollected += OnDiamondCollected;
                    }

                    found = true;
                    break;
                }
            }

            if (!found)
                Debug.LogWarning("[BlockLevelGenerator] Could not place diamond.");
        }
    }

    private bool IsFarEnoughFromAll(Vector3 candidate, List<Vector3> existing, float minDist)
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