using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class LoadingScreenController : MonoBehaviour
    {
        //TODO: это только заготовка
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _progressText;
    
        public void SetProgress(float progress)
        {
            _progressBar.value = progress;
            _progressText.text = $"{Mathf.Round(progress * 100)}%";
        }
    }
}