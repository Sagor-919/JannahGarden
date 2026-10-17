using JannahGarden.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace JannahGarden.UI
{
    public sealed class MobileGraphicsSettings : MonoBehaviour
    {
        private const string ShadowsKey = "JannahGarden.Graphics.Shadows";
        private const string AntiAliasingKey = "JannahGarden.Graphics.AntiAliasing";
        private const string RenderScaleKey = "JannahGarden.Graphics.HighRenderScale";

        [SerializeField] private Button settingsButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject panel;
        [SerializeField] private Toggle shadowsToggle;
        [SerializeField] private Toggle antiAliasingToggle;
        [SerializeField] private Toggle renderScaleToggle;

        private bool pausedBeforeOpening;
        private float defaultShadowDistance;
        private UniversalRenderPipelineAsset renderPipelineAsset;

        public void Configure(Button openButton, Button dismissButton, GameObject settingsPanel,
            Toggle shadows, Toggle antiAliasing, Toggle renderScale)
        {
            settingsButton = openButton;
            closeButton = dismissButton;
            panel = settingsPanel;
            shadowsToggle = shadows;
            antiAliasingToggle = antiAliasing;
            renderScaleToggle = renderScale;
        }

        private void Awake()
        {
            defaultShadowDistance = QualitySettings.shadowDistance > 0f ? QualitySettings.shadowDistance : 50f;
            renderPipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (panel != null)
                panel.SetActive(false);

            SetInitialToggle(shadowsToggle, ShadowsKey, QualitySettings.shadows != UnityEngine.ShadowQuality.Disable);
            SetInitialToggle(antiAliasingToggle, AntiAliasingKey, true);
            SetInitialToggle(renderScaleToggle, RenderScaleKey, true);
            ApplyShadows(shadowsToggle != null && shadowsToggle.isOn);
            ApplyAntiAliasing(antiAliasingToggle != null && antiAliasingToggle.isOn);
            ApplyRenderScale(renderScaleToggle != null && renderScaleToggle.isOn);
        }

        private void OnEnable()
        {
            if (settingsButton != null)
                settingsButton.onClick.AddListener(Open);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
            if (shadowsToggle != null)
                shadowsToggle.onValueChanged.AddListener(OnShadowsChanged);
            if (antiAliasingToggle != null)
                antiAliasingToggle.onValueChanged.AddListener(OnAntiAliasingChanged);
            if (renderScaleToggle != null)
                renderScaleToggle.onValueChanged.AddListener(OnRenderScaleChanged);
        }

        private void OnDisable()
        {
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(Open);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
            if (shadowsToggle != null)
                shadowsToggle.onValueChanged.RemoveListener(OnShadowsChanged);
            if (antiAliasingToggle != null)
                antiAliasingToggle.onValueChanged.RemoveListener(OnAntiAliasingChanged);
            if (renderScaleToggle != null)
                renderScaleToggle.onValueChanged.RemoveListener(OnRenderScaleChanged);
        }

        private static void SetInitialToggle(Toggle toggle, string key, bool defaultValue)
        {
            if (toggle != null)
                toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0);
        }

        private void Open()
        {
            if (panel == null)
                return;

            pausedBeforeOpening = GameStateManager.Instance != null && GameStateManager.Instance.IsPaused;
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetPaused(true);
            panel.SetActive(true);
        }

        private void Close()
        {
            if (panel != null)
                panel.SetActive(false);
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetPaused(pausedBeforeOpening);
        }

        private void OnShadowsChanged(bool enabled)
        {
            PlayerPrefs.SetInt(ShadowsKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyShadows(enabled);
        }

        private void OnAntiAliasingChanged(bool enabled)
        {
            PlayerPrefs.SetInt(AntiAliasingKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyAntiAliasing(enabled);
        }

        private void OnRenderScaleChanged(bool enabled)
        {
            PlayerPrefs.SetInt(RenderScaleKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyRenderScale(enabled);
        }

        private void ApplyShadows(bool enabled)
        {
            QualitySettings.shadows = enabled ? UnityEngine.ShadowQuality.All : UnityEngine.ShadowQuality.Disable;
            if (enabled)
                QualitySettings.shadowDistance = defaultShadowDistance;
        }

        private void ApplyAntiAliasing(bool enabled)
        {
            if (renderPipelineAsset != null)
                renderPipelineAsset.msaaSampleCount = enabled ? 4 : 1;
        }

        private void ApplyRenderScale(bool enabled)
        {
            if (renderPipelineAsset != null)
                renderPipelineAsset.renderScale = enabled ? 1f : 0.75f;
        }
    }
}
