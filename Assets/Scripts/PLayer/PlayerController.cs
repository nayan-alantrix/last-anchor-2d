using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ArrowIndicator arrowIndicator; 
    [SerializeField] private ParticleSystem bounceEffect;

    [Header("Mobile Settings")]
    [SerializeField] private float launchForce = 8f;
    [SerializeField] private float maxDragDistance = 2.5f;
    [SerializeField] private float minDragThreshold = 0.2f;

    [Header("References")]
    [SerializeField] private GameObject playerView;

    private Camera mainCam;
    private Vector2 touchStartWorldPos;
    private bool isDragging = false;
    private bool isGrounded = true;
    private LevelController levelController;
    private bool isActive = false;

    public void Initialize(LevelController controller)
    {
        levelController = controller;
    }

    public void OnGameStart()
    {
        isGrounded = false;
        rb.simulated = true;
        isActive = true;
        playerView.SetActive(true);
        var main = bounceEffect.main;
        main.startColor = Color.blue;
    }

    public void OnGamePaused()
    {
        isActive = false;
    }

    public void OnGameResumed()
    {
        isActive = true;
    }

    public void OnGameOver()
    {
        isActive = false;
        rb.simulated = false;
        playerView.SetActive(false);
        var main = bounceEffect.main;
        main.startColor = Color.red;
        bounceEffect.Play();
    }

    public void OnMainMenu()
    {
        isActive = false;
        rb.simulated = false;
        playerView.SetActive(false);
    }

    void Awake()
    {
        mainCam = Camera.main;
        if (arrowIndicator != null) arrowIndicator.Hide();
        rb.simulated = false;
        isActive = false;
    }

    void Update()
    {
        if (!isActive) return;
        if (!isGrounded)
        {
            if (isDragging) CancelDrag();
            return;
        }
        HandleMobileInput();
    }

    private void HandleMobileInput()
    {
        Pointer currentPointer = Pointer.current;
        if (currentPointer == null) return;

        if (currentPointer.press.wasPressedThisFrame)
        {
            Vector2 screenPos = currentPointer.position.ReadValue();
            touchStartWorldPos = mainCam.ScreenToWorldPoint(screenPos);
            isDragging = true;
        }

        if (isDragging && currentPointer.press.isPressed)
        {
            Vector2 currentScreenPos = currentPointer.position.ReadValue();
            Vector2 currentWorldPos = mainCam.ScreenToWorldPoint(currentScreenPos);
            Vector2 dragVector = touchStartWorldPos - currentWorldPos;

            // Update arrow stretch and rotation
            arrowIndicator.UpdateArrow(transform.position, dragVector, maxDragDistance);
        }

        if (isDragging && currentPointer.press.wasReleasedThisFrame)
        {
            Vector2 releaseScreenPos = currentPointer.position.ReadValue();
            Vector2 releaseWorldPos = mainCam.ScreenToWorldPoint(releaseScreenPos);
            Vector2 finalDragVector = touchStartWorldPos - releaseWorldPos;

            if (finalDragVector.magnitude > maxDragDistance)
                finalDragVector = finalDragVector.normalized * maxDragDistance;

            arrowIndicator.Hide();

            if (finalDragVector.magnitude >= minDragThreshold)
                FireCube(finalDragVector);
            else
                CancelDrag();
        }
    }

    private void FireCube(Vector2 launchDir)
    {
        isDragging = false;
        isGrounded = false;
        arrowIndicator.Hide();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(launchDir * launchForce, ForceMode2D.Impulse);
    }

    private void CancelDrag()
    {
        isDragging = false;
        arrowIndicator.Hide();
    }

    // Called by GateContactDetector when player enters next block
    public void ForceGrounded(Transform spawnPoint,float moveTime)
    {
        isGrounded = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
        //move form current loction to spawn point using DOTween
        transform.DOMove(spawnPoint.position, 0.5f).onComplete = () => rb.simulated = true;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            bounceEffect.Play();
        }

        if (collision.gameObject.CompareTag("Spike"))
        {
            Debug.Log("Player hit spike");
            levelController.SetGameOver();
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;
        }
    }
}