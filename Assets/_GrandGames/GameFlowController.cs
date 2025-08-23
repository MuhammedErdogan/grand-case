using System;
using _GrandGames.Levels.Logic;
using UnityEngine;

namespace _GrandGames
{
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private LevelService _levelService;
        [SerializeField] private LevelScheduler _levelScheduler;

        [SerializeField] private BoardBuilder _boardBuilder;

        [SerializeField] private LobbyUI _lobbyUI;

        private void Awake()
        {
            _levelService = GetComponent<LevelService>();
            _levelScheduler = GetComponent<LevelScheduler>();

            _boardBuilder = new BoardBuilder();

            _lobbyUI ??= FindFirstObjectByType<LobbyUI>();
        }
    }
}