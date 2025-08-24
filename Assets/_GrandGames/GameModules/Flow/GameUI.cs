using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GrandGames.GameModules.Flow
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Button _winButton;
        [SerializeField] private Button _loseButton;
        [SerializeField] private TextMeshProUGUI _boardText; //for testing

        public Action OnWon;
        public Action OnLost;

        private void OnEnable()
        {
            _winButton.onClick.AddListener(OnWinButtonClicked);
            _loseButton.onClick.AddListener(OnLoseButtonClicked);
        }

        private void OnDisable()
        {
            _winButton.onClick.RemoveListener(OnWinButtonClicked);
            _loseButton.onClick.RemoveListener(OnLoseButtonClicked);
        }

        private void OnWinButtonClicked()
        {
            OnWon?.Invoke();
        }

        private void OnLoseButtonClicked()
        {
            OnLost?.Invoke();
        }

        public void Show(char[,] board)
        {
            Debug.Log($"GameUI Show with board size: {board.GetLength(0)}x{board.GetLength(1)}");
            gameObject.SetActive(true);

            _boardText.text = BoardFormatter.ToText(board);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}