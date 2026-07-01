using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private UIController _uiController;
    [SerializeField] private LevelController _levelController;
    [SerializeField] private AudioController _audioController;
    public AudioController GetAudioController() => _audioController;
    
    [Header("Game Config")]
    public static string highScoreKey { get; private set; } = "HighScore";  

    private void Awake()
    {
        _uiController.Initialize(this);
        _levelController.Initialize(this);
    }
    public void OnGameStart()
    {
        _audioController.PlayMusic(AudioType.BGM_1);
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
    public void OnGameOver(int score)
    {
        _uiController.OnGameOver(score);
        _levelController.OnGameOver();
    }
    public void OnMainMenuClicked()
    {
        _uiController.OnMainMenuClicked();
        _levelController.OnMainMenuClicked();
    }

    public void UpdateCurrentScore(int score)
    {
        _uiController.UpdateCurrentScore(score);
    }
    public void UpdateTime(float time)
    {
        _uiController.UpdateSpikeTimer(time);
    }
}
