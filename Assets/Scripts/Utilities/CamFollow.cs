using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Zone Settings")]
    // Define the Y threshold for each zone boundary (world space Y where the lock is)
    // Camera only moves up past this Y when that zone is unlocked
    public float[] zoneBoundaryYPositions; // e.g., { 10f, 25f, 40f } bottom to top

    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public float verticalOffset = 2f; // how far above player center to keep camera

    [Header("Horizontal")]
    public bool lockHorizontal = true; // keep cam centered horizontally
    public float fixedX = 0f;

    private int currentZoneIndex = 0;      // which zone the player is currently allowed in
    private float maxAllowedCamY;          // camera cannot go above this Y
    private float cameraHalfHeight;

    void Start()
    {
        cameraHalfHeight = Camera.main.orthographicSize;

        // Camera starts locked to bottom zone
        // maxAllowedCamY = the Y of first boundary minus a bit so cam can't peek above
        if (zoneBoundaryYPositions != null && zoneBoundaryYPositions.Length > 0)
            maxAllowedCamY = zoneBoundaryYPositions[0] - cameraHalfHeight;
        else
            maxAllowedCamY = float.MaxValue; // no restriction
    }

    /// <summary>
    /// Call this from your lock/zone system when zone index is unlocked.
    /// zoneIndex = 0 means first lock opened (camera can now move to zone 1), etc.
    /// </summary>
    public void UnlockZone(int zoneIndex)
    {
        if (zoneIndex > currentZoneIndex)
        {
            currentZoneIndex = zoneIndex;

            // Allow camera to move up to the next boundary (or unlimited if last zone)
            if (currentZoneIndex < zoneBoundaryYPositions.Length)
                maxAllowedCamY = zoneBoundaryYPositions[currentZoneIndex] - cameraHalfHeight;
            else
                maxAllowedCamY = float.MaxValue;

            Debug.Log($"[CameraFollow] Zone {zoneIndex} unlocked. New maxAllowedCamY: {maxAllowedCamY}");
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        float targetY = player.position.y + verticalOffset;
        float clampedY = Mathf.Min(targetY, maxAllowedCamY);

        float targetX = lockHorizontal ? fixedX : player.position.x;

        Vector3 targetPos = new Vector3(targetX, clampedY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }
}

