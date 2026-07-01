using System.Collections;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private BlockSpawner _blockSpawner;
    [SerializeField] private PlayerController _playerController;
    [ SerializeField] private SpikeController _spikeController;
    private GameFlowController _gameFlowController;

    public void Initialize(GameFlowController gameFlowController)
    {
        _gameFlowController = gameFlowController;
        _blockSpawner.Initialize(this, _spikeController);
        _playerController.Initialize(this);
    }

    public void OnGameStart()
    {
        BlockData blockData = _blockSpawner.OnGameStart();
        _playerController.OnGameStart(blockData.playerSpawnPoint);
        _spikeController.OnGameStart(blockData.spikeSpawnPoint);
    }
    public void OnGamePause()
    {
        _blockSpawner.OnGamePause();
        _playerController.OnGamePaused();
        _spikeController.OnGamePause();
    }
    public void OnGameResume()
    {
        StartCoroutine(ActivatePlayerAfterDelay(0.3f));
        _blockSpawner.OnGameResume();
        _spikeController.OnGameResume();
    }

    private IEnumerator ActivatePlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _playerController.OnGameResumed();
    }
    public void OnGameOver()
    {
        _blockSpawner.OnGameOver();
        _playerController.OnGameOver();
        _spikeController.OnGameOver();
    }
    
    public void OnMainMenuClicked()
    {
        _blockSpawner.OnMainMenuClicked();
        _playerController.OnMainMenu();
        _spikeController.OnMainMenu();
    }
    public void SetGameOver(int score)=> _gameFlowController.OnGameOver(score);

    public void PlayerForcedGrounded(Transform spawnPoint, float moveTime) => _playerController.ForceGrounded(spawnPoint, moveTime);

    public void SpikeTimer(float time) => _gameFlowController.UpdateTime(time);
    
    public void UpdateCurrentScore(int score) => _gameFlowController.UpdateCurrentScore(score);

    public void PlayAudio(AudioType audioType) => _gameFlowController.GetAudioController().PlayAudio(audioType);
    
}