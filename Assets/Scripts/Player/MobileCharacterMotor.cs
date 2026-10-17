using JannahGarden.Gameplay;
using UnityEngine;
using UnityEngine.AI;

namespace JannahGarden.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class MobileCharacterMotor : MonoBehaviour
    {
        [SerializeField] private Transform movementBoundary;
        [SerializeField] private Camera movementCamera;
        [SerializeField] private Transform characterVisual;
        [SerializeField] private Collider walkableGround;
        [SerializeField, Min(0f)] private float moveSpeed = 3.5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField, Min(0f)] private float boundaryInset = 0.05f;
        [SerializeField, Min(0.1f)] private float fallbackBoundaryRadius = 2.5f;

        private CharacterController characterController;
        private float verticalSpeed;
        private Vector3 lastSupportedPosition;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            lastSupportedPosition = transform.position;
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

            if (transform.position.y < lastSupportedPosition.y - 0.35f)
            {
                ReturnToSupportedGround();
                return;
            }

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
            if (horizontal.sqrMagnitude > 0.001f && walkableGround != null)
            {
                Vector3 next = transform.position + horizontal * deltaTime;
                Vector3 forwardProbe = next + horizontal.normalized * characterController.radius * 0.8f;
                if (!HasGroundAt(next) || !HasGroundAt(forwardProbe))
                    horizontal = Vector3.zero;
            }

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

            if (characterController.isGrounded && HasGroundAt(transform.position))
                lastSupportedPosition = transform.position;
            else if (transform.position.y < lastSupportedPosition.y - 0.35f)
                ReturnToSupportedGround();
        }

        private bool HasGroundAt(Vector3 position)
        {
            if (walkableGround == null)
                return true;

            Ray ray = new Ray(position + Vector3.up * 1.5f, Vector3.down);
            return walkableGround.Raycast(ray, out _, 3f);
        }

        private void ReturnToSupportedGround()
        {
            characterController.enabled = false;
            transform.position = lastSupportedPosition;
            characterController.enabled = true;
            verticalSpeed = -2f;
        }

        private Vector3 ConstrainToBoundary(Vector3 horizontalVelocity, float deltaTime)
        {
            if (movementBoundary == null || deltaTime <= 0f)
                return horizontalVelocity;

            if (movementBoundary.GetComponent<Unity.AI.Navigation.NavMeshSurface>() == null)
                return ConstrainToCircle(horizontalVelocity, deltaTime);

            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit start, 0.75f, NavMesh.AllAreas))
                return Vector3.zero;

            Vector3 nextPosition = start.position + horizontalVelocity * deltaTime;
            if (!NavMesh.Raycast(start.position, nextPosition, out NavMeshHit edge, NavMesh.AllAreas))
                return horizontalVelocity;

            // Keep the input parallel to the NavMesh edge. Pulling the controller
            // back toward an inset point every frame makes it oscillate at seams.
            Vector3 slide = Vector3.ProjectOnPlane(horizontalVelocity * deltaTime, edge.normal);
            slide.y = 0f;
            if (slide.sqrMagnitude < 0.000001f)
                return Vector3.zero;

            if (NavMesh.Raycast(start.position, start.position + slide, out NavMeshHit slideEdge, NavMesh.AllAreas))
            {
                slide = slideEdge.position - start.position;
                slide.y = 0f;
                slide *= 0.98f;
            }

            return slide / deltaTime;
        }

        private Vector3 ConstrainToCircle(Vector3 horizontalVelocity, float deltaTime)
        {
            Renderer boundaryRenderer = movementBoundary.GetComponent<Renderer>();
            float radius = boundaryRenderer != null
                ? Mathf.Min(boundaryRenderer.bounds.extents.x, boundaryRenderer.bounds.extents.z)
                : fallbackBoundaryRadius;
            radius = Mathf.Max(0.1f, radius - boundaryInset - characterController.radius);

            Vector3 center = movementBoundary.position;
            Vector3 next = transform.position + horizontalVelocity * deltaTime;
            Vector2 offset = new Vector2(next.x - center.x, next.z - center.z);
            if (offset.sqrMagnitude <= radius * radius)
                return horizontalVelocity;

            Vector2 clamped = offset.normalized * radius;
            return (new Vector3(center.x + clamped.x, transform.position.y, center.z + clamped.y) - transform.position) / deltaTime;
        }
    }
}
