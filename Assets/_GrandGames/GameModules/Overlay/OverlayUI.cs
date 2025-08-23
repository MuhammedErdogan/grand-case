using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GrandGames.GameModules.Overlay
{
    public class OverlayUI : MonoBehaviour
    {
        [SerializeField] private LoadingPanel _loadingPanel;
        [SerializeField] private CanvasGroup _fadeCanvasGroup;

        public void ShowLoadingPanel()
        {
            _loadingPanel.Show("Loading...");
        }

        public void HideLoadingPanel()
        {
            _loadingPanel.Hide();
        }

        public async UniTask FadeIn(float duration)
        {
        }

        public async UniTask FadeOut(float duration)
        {
        }
    }
}