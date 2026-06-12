using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [SerializeField] private UIController _uiController;
    [SerializeField] private LevelController _levelController;

    private void Awake()
    {
        _uiController.Initialize(this);
        _levelController.Initialize(this);
    }
    public void OnGameStart()
    {
        _uiController.OnGameStart();
        _levelController.OnGameStart();
        Time.timeScale = 1f;
    }
    public void OnGamePause()
    {
        _uiController.OnGamePause();
        _levelController.OnGamePause();
        Time.timeScale = 0f;
    }
    public void OnGameResume()
    {
        _uiController.OnGameResume();
        _levelController.OnGameResume();
        Time.timeScale = 1f;
    }
    public void OnGameOver()
    {
        _uiController.OnGameOver();
        _levelController.OnGameOver();
    }
    public void OnMainMenuClicked()
    {
        _uiController.OnMainMenuClicked();
        _levelController.OnMainMenuClicked();
    }
}
