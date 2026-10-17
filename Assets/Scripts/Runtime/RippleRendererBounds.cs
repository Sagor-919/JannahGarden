using UnityEngine;

namespace JannahGarden.Vfx
{
    /// <summary>Keeps the flat ripple mesh visible while any part of the ring is on screen.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RippleRendererBounds : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh configuredMesh;

        private void OnEnable()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            Configure();
        }

        private void LateUpdate()
        {
            if (meshFilter != null && meshFilter.sharedMesh != configuredMesh)
                Configure();
        }

        private void Configure()
        {
            if (meshFilter == null || meshRenderer == null || meshFilter.sharedMesh == null)
                return;

            Mesh mesh = meshFilter.sharedMesh;
            Bounds bounds = mesh.bounds;
            float diameter = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            bounds.Expand(diameter * 2f);
            meshRenderer.localBounds = bounds;
            meshRenderer.allowOcclusionWhenDynamic = false;
            configuredMesh = mesh;
        }
    }
}
