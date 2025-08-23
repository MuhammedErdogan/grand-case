using _GrandGames.GameModules.Level;
using _GrandGames.GameModules.Level.Source;
using _GrandGames.GameModules.Overlay;
using UnityEngine;

namespace _GrandGames.Modules.Flow
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

        private void OnEnable()
        {
            _lobbyUI.OnPlayButtonClickedEvent += OnPlayButtonClicked;
        }

        private void OnDisable()
        {
            _lobbyUI.OnPlayButtonClickedEvent -= OnPlayButtonClicked;
        }

        private async void OnPlayButtonClicked()
        {
            var currentLevel = await _levelService.GetLevelData(1, default);

            var board = await _boardBuilder.BuildAsync(currentLevel);

            _overlayUI.ShowLoadingPanel();

            _lobbyUI.Hide();
            _gameUI.Show(board);

            _overlayUI.HideLoadingPanel();

            Debug.Log($"Board Size: {board.GetLength(0)}x{board.GetLength(1)}");
        }

        private void OnLobbyLoaded()
        {
            //var difficulty = _levelService.GetDifficulty();
            _lobbyUI.SetDifficulty(2);
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
            _levelScheduler.OnLevelFinished(TestLevel, remoteSource);
        }
    }
}