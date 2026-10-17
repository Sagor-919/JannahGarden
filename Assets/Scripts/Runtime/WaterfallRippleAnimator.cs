using UnityEngine;

namespace JannahGarden.Vfx
{
    /// <summary>Cycles mesh ripples while the Shader Graph scrolls their annular UVs.</summary>
    public sealed class WaterfallRippleAnimator : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private MeshRenderer[] rings;
        [SerializeField, Min(0.1f)] private float lifetime = 2.7f;
        [SerializeField, Min(0.01f)] private float startScale = 21f;
        [SerializeField, Min(0.01f)] private float endScale = 43f;
        [SerializeField, Range(0f, 1f)] private float opacity = 0.55f;
        [SerializeField, ColorUsage(true, true)] private Color rippleTint = new Color(2f, 2.5f, 3f, 1f);

        private MaterialPropertyBlock propertyBlock;

        public void Configure(MeshRenderer[] rippleRenderers)
        {
            rings = rippleRenderers;
        }

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (rings == null || rings.Length == 0)
                return;

            propertyBlock ??= new MaterialPropertyBlock();

            for (int i = 0; i < rings.Length; i++)
            {
                MeshRenderer ring = rings[i];
                if (ring == null)
                    continue;

                float phase = Mathf.Repeat(Time.time / lifetime + (float)i / rings.Length, 1f);
                float scale = Mathf.Lerp(startScale, endScale, phase);
                ring.transform.localScale = new Vector3(scale, scale, scale);

                float fadeIn = Mathf.Clamp01(phase / 0.2f);
                float fadeOut = Mathf.Clamp01((1f - phase) / 0.55f);
                Color color = rippleTint;
                color.a = opacity * fadeIn * fadeOut;
                propertyBlock.SetColor(ColorId, color);
                ring.SetPropertyBlock(propertyBlock);
            }
        }

    }
}
