using JannahGarden.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JannahGarden.Input
{
    /// <summary>Routes touches on the fixed joystick to movement and other drags to the camera.</summary>
    public sealed class MobileTouchInputSurface : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private Joystick movementJoystick;
        [SerializeField] private RectTransform movementTouchArea;
        [SerializeField] private Transform cameraOrbit;
        [SerializeField, Min(0.01f)] private float lookDegreesPerPixel = 0.18f;
        [SerializeField] private float minimumPitch = -10f;
        [SerializeField] private float maximumPitch = 70f;

        private int movementPointerId = int.MinValue;
        private int cameraPointerId = int.MinValue;
        private float yaw;
        private float pitch = 18f;

        public void Configure(Joystick joystick, Transform orbit)
        {
            movementJoystick = joystick;
            movementTouchArea = joystick != null ? joystick.transform.Find("Background") as RectTransform : null;
            cameraOrbit = orbit;
            if (cameraOrbit != null)
            {
                Vector3 angles = cameraOrbit.localEulerAngles;
                yaw = angles.y;
                pitch = NormalizePitch(angles.x);
            }
        }

        private void OnEnable()
        {
            if (movementTouchArea == null && movementJoystick != null)
                movementTouchArea = movementJoystick.transform.Find("Background") as RectTransform;

            if (cameraOrbit != null)
            {
                Vector3 angles = cameraOrbit.localEulerAngles;
                yaw = angles.y;
                pitch = NormalizePitch(angles.x);
            }

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.PauseChanged += OnPauseChanged;
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.PauseChanged -= OnPauseChanged;
            ResetTouchState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused)
                return;

            if (movementTouchArea != null && RectTransformUtility.RectangleContainsScreenPoint(
                    movementTouchArea, eventData.position, eventData.pressEventCamera))
            {
                if (movementPointerId != int.MinValue || movementJoystick == null)
                    return;

                movementPointerId = eventData.pointerId;
                movementJoystick.OnPointerDown(eventData);
                return;
            }

            if (cameraPointerId == int.MinValue)
                cameraPointerId = eventData.pointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == movementPointerId)
            {
                if (movementJoystick != null)
                    movementJoystick.OnDrag(eventData);
                return;
            }

            if (eventData.pointerId != cameraPointerId || cameraOrbit == null)
                return;

            yaw += eventData.delta.x * lookDegreesPerPixel;
            pitch = Mathf.Clamp(pitch - eventData.delta.y * lookDegreesPerPixel, minimumPitch, maximumPitch);
            cameraOrbit.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == movementPointerId)
            {
                if (movementJoystick != null)
                    movementJoystick.OnPointerUp(eventData);
                movementPointerId = int.MinValue;
            }

            if (eventData.pointerId == cameraPointerId)
                cameraPointerId = int.MinValue;
        }

        private static float NormalizePitch(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
                ResetTouchState();
        }

        private void ResetTouchState()
        {
            if (movementPointerId != int.MinValue && movementJoystick != null)
                movementJoystick.OnPointerUp(null);

            movementPointerId = int.MinValue;
            cameraPointerId = int.MinValue;
        }
    }
}
