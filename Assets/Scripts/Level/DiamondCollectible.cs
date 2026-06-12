using UnityEngine;

public class DiamondCollectible : MonoBehaviour
{
    public System.Action onCollected;
    private bool collected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Diamond collected!");
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        onCollected?.Invoke();
        gameObject.SetActive(false);
    }
}