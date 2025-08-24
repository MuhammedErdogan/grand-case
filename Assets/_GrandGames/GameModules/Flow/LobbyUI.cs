using System;
using UnityEngine;
using UnityEngine.UI;

namespace _GrandGames.GameModules.Flow
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Image _difficultyImage;

        public Action OnPlayButtonClickedEvent;

        private void Start()
        {
            SetDifficulty(1);
        }

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

        public void SetDifficulty(int difficulty)
        {
            Debug.Log($"Difficulty set to {difficulty}");

            _difficultyImage.color = difficulty switch
            {
                0 => Color.green,
                1 => Color.yellow,
                2 => Color.red,
                _ => Color.white
            };
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}