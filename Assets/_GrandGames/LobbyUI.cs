using System;
using UnityEngine;
using UnityEngine.UI;

namespace _GrandGames
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Button _playButton;

        public Action OnPlayButtonClickedEvent;

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }

        private void OnPlayButtonClicked()
        {
            OnPlayButtonClickedEvent?.Invoke();
        }
    }
}