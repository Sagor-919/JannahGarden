using JannahGarden.Gameplay;
using UnityEngine;

namespace JannahGarden.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class MobileCharacterMotor : MonoBehaviour
    {
        [SerializeField] private Transform movementBoundary;
        [SerializeField] private Camera movementCamera;
        [SerializeField] private Transform characterVisual;
        [SerializeField, Min(0f)] private float moveSpeed = 3.5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField, Min(0f)] private float boundaryInset = 0.05f;
        [SerializeField, Min(0.1f)] private float fallbackBoundaryRadius = 2.5f;

        private CharacterController characterController;
        private float verticalSpeed;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        public void Configure(Transform boundary, Camera cameraToFollow, Transform visualTransform)
        {
            movementBoundary = boundary;
            movementCamera = cameraToFollow;
            characterVisual = visualTransform;
        }

        private void Update()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused)
                return;

            Vector2 input = InputManager.Instance != null ? InputManager.Instance.Movement : Vector2.zero;
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward = movementCamera != null ? movementCamera.transform.forward : Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            Vector3 direction = right * input.x + forward * input.y;
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            float deltaTime = Time.deltaTime;
            Vector3 horizontal = direction * moveSpeed;
            horizontal = ConstrainToBoundary(horizontal, deltaTime);

            if (horizontal.sqrMagnitude > 0.001f)
            {
                Quaternion facing = Quaternion.LookRotation(horizontal.normalized, Vector3.up);
                Transform facingTarget = characterVisual != null ? characterVisual : transform;
                facingTarget.rotation = Quaternion.RotateTowards(facingTarget.rotation, facing, 720f * deltaTime);
            }

            if (characterController.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;
            else
                verticalSpeed += gravity * deltaTime;

            Vector3 velocity = horizontal + Vector3.up * verticalSpeed;
            CollisionFlags collisions = characterController.Move(velocity * deltaTime);
            if ((collisions & CollisionFlags.Below) != 0 && verticalSpeed < 0f)
                verticalSpeed = -2f;
        }

        private Vector3 ConstrainToBoundary(Vector3 horizontalVelocity, float deltaTime)
        {
            if (movementBoundary == null || deltaTime <= 0f)
                return horizontalVelocity;

            float radius = GetBoundaryRadius() - boundaryInset - characterController.radius;
            radius = Mathf.Max(0.1f, radius);

            Vector3 center = movementBoundary.position;
            Vector3 nextPosition = transform.position + horizontalVelocity * deltaTime;
            Vector2 offset = new Vector2(nextPosition.x - center.x, nextPosition.z - center.z);
            if (offset.sqrMagnitude <= radius * radius)
                return horizontalVelocity;

            Vector2 clampedOffset = offset.normalized * radius;
            Vector3 clampedPosition = new Vector3(center.x + clampedOffset.x, nextPosition.y, center.z + clampedOffset.y);
            return (clampedPosition - transform.position) / deltaTime;
        }

        private float GetBoundaryRadius()
        {
            Renderer boundaryRenderer = movementBoundary.GetComponent<Renderer>();
            if (boundaryRenderer != null)
                return Mathf.Min(boundaryRenderer.bounds.extents.x, boundaryRenderer.bounds.extents.z);

            MeshFilter meshFilter = movementBoundary.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                return fallbackBoundaryRadius;

            Vector3 scale = movementBoundary.lossyScale;
            Vector3 extents = meshFilter.sharedMesh.bounds.extents;
            return Mathf.Min(extents.x * Mathf.Abs(scale.x), extents.z * Mathf.Abs(scale.z));
        }
    }
}
