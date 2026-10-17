using UnityEngine;

namespace JannahGarden.Gameplay
{
    [DefaultExecutionOrder(-90)]
    public sealed class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [SerializeField] private Joystick movementJoystick;

        public Vector2 Movement
        {
            get { return movementJoystick != null ? movementJoystick.Direction : Vector2.zero; }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void BindMovementJoystick(Joystick joystick)
        {
            movementJoystick = joystick;
        }

        public void UnbindMovementJoystick(Joystick joystick)
        {
            if (movementJoystick == joystick)
                movementJoystick = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
