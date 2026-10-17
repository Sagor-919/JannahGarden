using System;
using UnityEngine;

namespace JannahGarden.Gameplay
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public bool IsPaused { get; private set; }
        public event Action<bool> PauseChanged;

        private float resumeTimeScale = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Time.timeScale = 1f;
            IsPaused = false;
        }

        public void TogglePause()
        {
            SetPaused(!IsPaused);
        }

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
                return;

            if (paused)
            {
                resumeTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = resumeTimeScale > 0f ? resumeTimeScale : 1f;
            }

            IsPaused = paused;
            PauseChanged?.Invoke(IsPaused);
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                SetPaused(true);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
#if !UNITY_EDITOR
            if (!hasFocus)
                SetPaused(true);
#endif
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            Instance = null;
            Time.timeScale = 1f;
        }
    }
}
