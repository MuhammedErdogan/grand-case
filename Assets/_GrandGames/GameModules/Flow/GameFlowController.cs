using System;
using _GrandGames.GameModules.Level;
using _GrandGames.GameModules.Level.Source;
using _GrandGames.GameModules.Overlay;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.GameModules.Flow
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Services")] [SerializeField] private LevelService _levelService;
        [SerializeField] private LevelScheduler _levelScheduler;

        [SerializeField] private BoardBuilder _boardBuilder;

        [Header("References")] [SerializeField] private LobbyUI _lobbyUI;
        [SerializeField] private GameUI _gameUI;
        [SerializeField] private OverlayUI _overlayUI;

        [SerializeField] private int TestLevel = 1;

        private void Awake()
        {
            _levelService = new LevelService();
            _levelScheduler = new LevelScheduler();

            _boardBuilder = new BoardBuilder();

            _lobbyUI ??= FindFirstObjectByType<LobbyUI>();
        }

        private void Start()
        {
            InitialGameProcess();
        }

        private void OnEnable()
        {
            _lobbyUI.OnPlayButtonClickedEvent += OnPlayButtonClicked;

            _gameUI.OnWon += OnGameWon;
            _gameUI.OnLost += OnGameLost;
        }

        private void OnDisable()
        {
            _lobbyUI.OnPlayButtonClickedEvent -= OnPlayButtonClicked;

            _gameUI.OnWon -= OnGameWon;
            _gameUI.OnLost -= OnGameLost;
        }

        private async void InitialGameProcess()
        {
            _overlayUI.ShowLoadingPanel();

            await _levelScheduler.CheckLevelSchedule(_levelService.CurrentLevel - 1, _levelService.RemoteSource);

            await PrepareLevelAndLobbyUI();

            _lobbyUI.Show();
            _gameUI.Hide();

            _overlayUI.HideLoadingPanel();
        }

        private void OnPlayButtonClicked()
        {
            LoadLevelProcess();
        }

        private async void LoadLevelProcess()
        {
            _overlayUI.ShowLoadingPanel();

            await Resources.UnloadUnusedAssets();
            GC.Collect();

            var currentLevel = await _levelService.GetCurrentLevelData(this.GetCancellationTokenOnDestroy());

            Debug.Log(currentLevel);

            var board = await _boardBuilder.BuildAsync(currentLevel);

            _lobbyUI.Hide();
            _gameUI.Show(board);

            await UniTask.Delay(500); //for simulate lobby loading time

            _overlayUI.HideLoadingPanel();

            Debug.Log($"Board Size: {board.GetLength(0)}x{board.GetLength(1)}");
        }

        private void OnGameWon()
        {
            LoadLobbyFromLevelFinished(true);
        }

        private void OnGameLost()
        {
            LoadLobbyFromLevelFinished(false);
        }

        private async void LoadLobbyFromLevelFinished(bool success)
        {
            if (success)
            {
                _ = _levelScheduler.CheckLevelSchedule(_levelService.CurrentLevel, _levelService.RemoteSource);

                _levelService.IncrementLevel();
            }

            _overlayUI.ShowLoadingPanel();

            await PrepareLevelAndLobbyUI();

            _lobbyUI.Show();
            _gameUI.Hide();

            await Resources.UnloadUnusedAssets();

            await UniTask.Delay(500); //for simulate lobby loading time

            _overlayUI.HideLoadingPanel();
        }

        private async UniTask PrepareLevelAndLobbyUI()
        {
            var difficulty = await _levelService.PrepareCurrentLevel(this.GetCancellationTokenOnDestroy());
            _lobbyUI.SetDifficulty((int)difficulty);
            _lobbyUI.SetLevel(_levelService.CurrentLevel);
        }

        [ContextMenu("Test Get From Remote")]
        private void GetFromRemote()
        {
            _levelService.GetFromRemote();
        }

        [ContextMenu("Test Get From Cache")]
        private void GetFromCache()
        {
            _levelService.GetFromCache();
        }

        [ContextMenu("Test Get From Resources")]
        private void GetFromResources()
        {
            _levelService.GetFromResources();
        }

        [ContextMenu("Test Level Finished")]
        private void TestLevelFinished()
        {
            var remoteSource = new RemoteSource();
            _ = _levelScheduler.CheckLevelSchedule(TestLevel, remoteSource);
        }
    }
}