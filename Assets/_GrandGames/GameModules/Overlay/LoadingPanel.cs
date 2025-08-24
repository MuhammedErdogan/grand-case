using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GrandGames.GameModules.Overlay
{
    public class LoadingPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _loadingImage;

        [SerializeField] private TextMeshProUGUI _loadingInfoText;

        private Coroutine _rotateCoroutine;

        private void OnDestroy()
        {
            if (_rotateCoroutine == null)
            {
                return;
            }

            StopCoroutine(_rotateCoroutine);
            _rotateCoroutine = null;
        }

        public void Show(string info)
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            
            _loadingInfoText.text = info;

            _rotateCoroutine = StartCoroutine(RotateLoadingImage());
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            
            if (_rotateCoroutine == null)
            {
                return;
            }

            StopCoroutine(_rotateCoroutine);
            _rotateCoroutine = null;
        }

        public void SetInfo(string info)
        {
            _loadingInfoText.text = info;
        }

        private IEnumerator RotateLoadingImage()
        {
            while (_canvasGroup.alpha > 0)
            {
                _loadingImage.transform.Rotate(new Vector3(0, 0, -90) * Time.deltaTime);
                yield return null;
            }
        }
    }
}