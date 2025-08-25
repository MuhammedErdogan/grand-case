using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GrandGames.GameModules.Flow
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Image _difficultyImage;
        [SerializeField] private TextMeshProUGUI _levelText;

        public Action OnPlayButtonClickedEvent;

        // can be moved to a config file or ScriptableObject for easier tweaking
        private static readonly Dictionary<int, Color> Colors = new()
        {
            { 0, new Color(0.4f, 0.8f, 0.4f) }, // Easy - Green
            { 1, new Color(1f, 0.65f, 0f) }, // Medium - Orange
            { 2, new Color(1f, 0.2f, 0.2f) } // Hard - Red
        };

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

            _difficultyImage.color = Colors[difficulty];
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void SetLevel(int levelServiceCurrentLevel)
        {
            _levelText.SetText($"Level {levelServiceCurrentLevel}");
        }
    }
}