using UnityEngine;

namespace JannahGarden.Player
{
    /// <summary>Legacy follow behavior retained for scenes that still reference the component.</summary>
    public sealed class CameraFollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.8f, -5.5f);
        [SerializeField, Min(0f)] private float lookHeight = 0.9f;
        [SerializeField, Min(0f)] private float followSharpness = 12f;

        public void Configure(Transform followTarget, Vector3 followOffset)
        {
            target = followTarget;
            offset = followOffset;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            float deltaTime = Time.unscaledDeltaTime;
            float blend = followSharpness <= 0f ? 1f : 1f - Mathf.Exp(-followSharpness * deltaTime);
            transform.position = Vector3.Lerp(transform.position, target.position + offset, blend);
            transform.LookAt(target.position + Vector3.up * lookHeight, Vector3.up);
        }
    }
}
