using UnityEngine;
using UnityEngine.UI;

namespace JannahGarden.UI
{
    public sealed class MobileFpsCounter : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField, Min(0.1f)] private float refreshInterval = 0.5f;

        private float elapsed;
        private int frames;

        public void Configure(Text fpsLabel)
        {
            label = fpsLabel;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            frames++;
            if (elapsed < refreshInterval || label == null)
                return;

            float fps = frames / Mathf.Max(elapsed, 0.0001f);
            label.text = "FPS " + Mathf.RoundToInt(fps);
            elapsed = 0f;
            frames = 0;
        }
    }
}
