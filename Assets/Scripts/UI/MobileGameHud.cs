using JannahGarden.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace JannahGarden.UI
{
    public sealed class MobileGameHud : MonoBehaviour
    {
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private ShapeIconGraphic pausePlayIcon;
        [SerializeField] private Joystick movementJoystick;

        public void Configure(Button pause, Button quit, ShapeIconGraphic icon, Joystick joystick, Button restart = null)
        {
            pauseButton = pause;
            restartButton = restart;
            quitButton = quit;
            pausePlayIcon = icon;
            movementJoystick = joystick;
        }

        private void OnEnable()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(TogglePause);
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);

            if (InputManager.Instance != null)
                InputManager.Instance.BindMovementJoystick(movementJoystick);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PauseChanged += OnPauseChanged;
                OnPauseChanged(GameStateManager.Instance.IsPaused);
            }
        }

        private void OnDisable()
        {
            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(TogglePause);
            if (restartButton != null)
                restartButton.onClick.RemoveListener(RestartGame);
            if (quitButton != null)
                quitButton.onClick.RemoveListener(QuitGame);

            if (InputManager.Instance != null)
                InputManager.Instance.UnbindMovementJoystick(movementJoystick);

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.PauseChanged -= OnPauseChanged;
        }

        private void TogglePause()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.TogglePause();
        }

        private void QuitGame()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.QuitGame();
            else
                Application.Quit();
        }

        private void RestartGame()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.RestartGame();
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (pausePlayIcon != null)
                pausePlayIcon.Shape = isPaused ? ShapeIconGraphic.IconShape.Play : ShapeIconGraphic.IconShape.Pause;
        }
    }
}
