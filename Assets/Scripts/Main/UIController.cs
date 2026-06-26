using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIController : MonoBehaviour
{
    [Header("Configs ")]
    [SerializeField] private float _gameOverDelay = 0.7f;
    [Header("UI panels")]
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _gamePlayUI;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _gameOverMenu;

    [Header("Main menu ")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;

    [Header("Gameplay UI")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private TextMeshProUGUI _spikeTimerText;

    [Header("Pause menu")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("Game over menu")]
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButtonGameOver;

    //safafv

    private GameFlowController _gameFlowController;

    public void Initialize(GameFlowController gameFlowController)
    {
        _gameFlowController = gameFlowController;
        RegisterButtonCallbacks();
    }

    private void Awake()
    {
        ActivateUI(GameState.MainMenu);
    }

    private void RegisterButtonCallbacks()
    {
        _playButton.onClick.AddListener(() => _gameFlowController.OnGameStart());
        _quitButton.onClick.AddListener(() => Application.Quit());
        _pauseButton.onClick.AddListener(() => _gameFlowController.OnGamePause());
        _resumeButton.onClick.AddListener(() => _gameFlowController.OnGameResume());
        _restartButton.onClick.AddListener(() => _gameFlowController.OnGameStart());
        _mainMenuButton.onClick.AddListener(() => _gameFlowController.OnMainMenuClicked());
        _retryButton.onClick.AddListener(() => _gameFlowController.OnGameStart());
        _mainMenuButtonGameOver.onClick.AddListener(() => _gameFlowController.OnMainMenuClicked());
    }
    public void OnGameStart()
    {
        ActivateUI(GameState.Playing);
    }
    public void OnGamePause()
    {
        ActivateUI(GameState.Paused);   
    }
    public void OnGameResume()
    {
        ActivateUI(GameState.Playing);
    }

    public void OnGameOver()
    {
        StartCoroutine(DelayAction(_gameOverDelay));
    }

    private IEnumerator DelayAction(float delay)
    {
        yield return new WaitForSeconds(delay);
        ActivateUI(GameState.GameOver);
    }

    public void OnMainMenuClicked()
    {
        ActivateUI(GameState.MainMenu);
    }

    public void UpdateSpikeTimer(float time)
    {
        _spikeTimerText.text = time.ToString("F2");
    }

    private void ActivateUI(GameState state)
    {
        _mainMenu.SetActive(false);
        _gamePlayUI.SetActive(false);
        _pauseMenu.SetActive(false);
        _gameOverMenu.SetActive(false);
        switch (state)
        {
            case GameState.MainMenu:
                _mainMenu.SetActive(true);
                break;
            case GameState.Playing:
                _gamePlayUI.SetActive(true);
                break;
            case GameState.Paused:
                _pauseMenu.SetActive(true);
                break;
            case GameState.GameOver:
                _gameOverMenu.SetActive(true);
                break;
        }
    }
    enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }    
}

