using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
        Hide();
    }

    // Call this every frame while dragging
    public void UpdateArrow(Vector2 fromWorld, Vector2 dragVector, float maxDragDistance)
    {
        if (dragVector.magnitude < 0.05f)
        {
            Hide();
            return;
        }

        // Clamp to max drag
        Vector2 clampedDrag = Vector2.ClampMagnitude(dragVector, maxDragDistance);

        // Position arrow at player
        transform.position = new Vector3(fromWorld.x, fromWorld.y, transform.position.z);

        // Rotate to face drag direction
        float angle = Mathf.Atan2(-clampedDrag.y, -clampedDrag.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Stretch X based on drag magnitude ratio
        float stretchRatio = clampedDrag.magnitude / maxDragDistance;
        transform.localScale = new Vector3(
            originalScale.x * stretchRatio,
            originalScale.y,
            originalScale.z
        );

        Show();
    }

    public void Show() => spriteRenderer.enabled = true;
    public void Hide() => spriteRenderer.enabled = false;
}