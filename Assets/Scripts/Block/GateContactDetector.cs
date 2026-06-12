using UnityEngine;

public class GateContactDetector : MonoBehaviour
{
    [SerializeField] private BlockController blockController;

    // Fires when player has FULLY passed through and exited the trigger
    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        blockController.OnGateTouched();
    }
}