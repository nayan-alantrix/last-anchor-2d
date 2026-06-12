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
        _blockSpawner.Initialize(this);
        _playerController.Initialize(this);
    }

    public void OnGameStart()
    {
        BlockData blockData = _blockSpawner.OnGameStart();

        _playerController.transform.position = blockData.playerSpawnPoint.position;
        _playerController.OnGameStart();
        _spikeController.OnGameStart(blockData.spikeSpawnPoint);
    }
    public void OnGamePause()
    {
        _blockSpawner.OnGamePause();
        _playerController.OnGamePaused();
    }
    public void OnGameResume()
    {
        _playerController.OnGameResumed();
    }

    public void OnGameOver()
    {
        _blockSpawner.OnGameOver();
        _playerController.OnGameOver();
    }

    public void OnMainMenuClicked()
    {
        _blockSpawner.OnMainMenuClicked();
        _playerController.OnMainMenu();
    }

    public void PlayerForcedGrounded(Transform spawnPoint, float moveTime)
    {
        _playerController.ForceGrounded(spawnPoint, moveTime);
    }
}