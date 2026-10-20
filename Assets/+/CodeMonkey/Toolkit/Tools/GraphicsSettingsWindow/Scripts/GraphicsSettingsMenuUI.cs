using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

namespace CodeMonkey.Toolkit.TGraphicsSettings {

    /// <summary>
    /// ** Graphics Settings Menu UI **
    ///
    /// This default window wires every setting control (Toggle, Slider, TMP_Dropdown)
    /// to the static GraphicsSettings class.
    /// 
    /// On Start: populates dropdowns, reads current values via GraphicsSettings getters 
    /// to initialize every control without firing callbacks,
    /// then registers listeners so user interactions are immediately applied and persisted.
    /// 
    /// On Destroy: all listeners are removed to prevent stale callback references.
    /// </summary>
    public class GraphicsSettingsMenuUI : MonoBehaviour {


        // ─────────────────────────────────────────────────────────────
        // Slider Range Constants
        // ─────────────────────────────────────────────────────────────
        private const float BRIGHTNESS_MIN      = -2f;
        private const float BRIGHTNESS_MAX      =  2f;
        private const float GAMMA_MIN           = -1f;
        private const float GAMMA_MAX           =  1f;
        private const float BLOOM_INTENSITY_MIN =  0f;
        private const float BLOOM_INTENSITY_MAX =  5f;
        private const float RENDER_SCALE_MIN    =  0.5f;
        private const float RENDER_SCALE_MAX    =  2f;
        private const float SHADOW_DISTANCE_MIN =  10f;
        private const float SHADOW_DISTANCE_MAX =  150f;

        private const int MSAA_SAMPLE_COUNT_1 = 1;
        private const int MSAA_SAMPLE_COUNT_2 = 2;
        private const int MSAA_SAMPLE_COUNT_4 = 4;
        private const int MSAA_SAMPLE_COUNT_8 = 8;

        private const int SHADOW_CASCADE_MIN = 1;
        private const int SHADOW_CASCADE_MAX = 4;


        // ─────────────────────────────────────────────────────────────
        // Scene References
        // ─────────────────────────────────────────────────────────────
        [Header("Scene References")]
        [Tooltip("Global post-processing Volume for colour-grade overrides.")]
        [SerializeField] private Volume globalVolume;

        [Tooltip("Camera receiving post-processing and anti-aliasing settings.")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("SSAO renderer feature; null means the AO toggle logs a warning and is skipped gracefully.")]
        [SerializeField] private ScriptableRendererFeature ambientOcclusionFeature;


        // ─────────────────────────────────────────────────────────────
        // Display Toggles
        // ─────────────────────────────────────────────────────────────
        [Header("Display Toggles")]
        [Tooltip("Fullscreen / windowed toggle.")]
        [SerializeField] private Toggle fullscreenToggle;

        [Tooltip("VSync on/off toggle.")]
        [SerializeField] private Toggle vSyncToggle;


        // ─────────────────────────────────────────────────────────────
        // Quality Toggles
        // ─────────────────────────────────────────────────────────────
        [Header("Quality Toggle")]
        [Tooltip("Enables or disables post-processing on the target camera.")]
        [SerializeField] private Toggle postProcessingToggle;


        // ─────────────────────────────────────────────────────────────
        // Post-Processing Toggles
        // ─────────────────────────────────────────────────────────────
        [Header("Post-Processing Toggles")]
        [Tooltip("Enable / disable the Bloom volume override.")]
        [SerializeField] private Toggle bloomToggle;

        [Tooltip("Enable / disable the Motion Blur volume override.")]
        [SerializeField] private Toggle motionBlurToggle;

        [Tooltip("Enable / disable the Vignette volume override.")]
        [SerializeField] private Toggle vignetteToggle;

        [Tooltip("Enable / disable the Depth of Field volume override.")]
        [SerializeField] private Toggle depthOfFieldToggle;

        [Tooltip("Enable / disable the SSAO renderer feature (null-safe; feature must be assigned).")]
        [SerializeField] private Toggle ambientOcclusionToggle;


        // ─────────────────────────────────────────────────────────────
        // Post-Processing Sliders
        // ─────────────────────────────────────────────────────────────

        [Header("Post-Processing Sliders")]
        [Tooltip("Post-exposure brightness offset in EV. Range set to BRIGHTNESS_MIN..BRIGHTNESS_MAX.")]
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private TextMeshProUGUI brightnessSliderTextMesh;

        [Tooltip("LiftGammaGain midtone gamma offset. Range set to GAMMA_MIN..GAMMA_MAX.")]
        [SerializeField] private Slider gammaSlider;
        [SerializeField] private TextMeshProUGUI gammaSliderTextMesh;

        [Tooltip("Bloom intensity. Range set to BLOOM_INTENSITY_MIN..BLOOM_INTENSITY_MAX.")]
        [SerializeField] private Slider bloomIntensitySlider;
        [SerializeField] private TextMeshProUGUI bloomIntensitySliderTextMesh;


        // ─────────────────────────────────────────────────────────────
        // Quality Sliders
        // ─────────────────────────────────────────────────────────────

        [Header("Quality Sliders")]
        [Tooltip("URP render scale. Range set to RENDER_SCALE_MIN..RENDER_SCALE_MAX.")]
        [SerializeField] private Slider renderScaleSlider;
        [SerializeField] private TextMeshProUGUI renderScaleSliderTextMesh;

        [Tooltip("URP shadow draw distance. Range set to SHADOW_DISTANCE_MIN..SHADOW_DISTANCE_MAX.")]
        [SerializeField] private Slider shadowDistanceSlider;
        [SerializeField] private TextMeshProUGUI shadowDistanceSliderTextMesh;


        // ─────────────────────────────────────────────────────────────
        // Display Dropdowns
        // ─────────────────────────────────────────────────────────────

        [Header("Display Dropdowns")]
        [Tooltip("Populated at runtime from GraphicsSettings.GetAvailableResolutions().")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;


        // ─────────────────────────────────────────────────────────────
        // Quality Dropdowns
        // ─────────────────────────────────────────────────────────────

        [Header("Quality Dropdowns")]
        [Tooltip("MSAA sample count (1 / 2 / 4 / 8).")]
        [SerializeField] private TMP_Dropdown msaaDropdown;

        [Tooltip("Post-process AA mode populated from AntialiasingMode enum names.")]
        [SerializeField] private TMP_Dropdown antiAliasingModeDropdown;

        [Tooltip("Post-process AA quality populated from AntialiasingQuality enum names.")]
        [SerializeField] private TMP_Dropdown antiAliasingQualityDropdown;

        [Tooltip("Directional shadow cascade count (1–4).")]
        [SerializeField] private TMP_Dropdown shadowCascadeDropdown;



        private Resolution[] resolutionArray;
        private int[] msaaSampleCountArray;


        private void Start() {
            ConfigureSliderRanges();
            PopulateDropdowns();
            InitializeControlValues();
            RegisterListeners();
        }

        private void OnDestroy() {
            RemoveListeners();
        }


        private void ConfigureSliderRanges() {
            SetSliderRange(brightnessSlider,     BRIGHTNESS_MIN,      BRIGHTNESS_MAX);
            SetSliderRange(gammaSlider,           GAMMA_MIN,           GAMMA_MAX);
            SetSliderRange(bloomIntensitySlider,  BLOOM_INTENSITY_MIN, BLOOM_INTENSITY_MAX);
            SetSliderRange(renderScaleSlider,     RENDER_SCALE_MIN,    RENDER_SCALE_MAX);
            SetSliderRange(shadowDistanceSlider,  SHADOW_DISTANCE_MIN, SHADOW_DISTANCE_MAX);
        }

        private static void SetSliderRange(Slider slider, float min, float max) {
            if (slider == null) return;
            slider.minValue = min;
            slider.maxValue = max;
        }


        private void PopulateDropdowns() {
            PopulateResolutionDropdown();
            PopulateMsaaDropdown();
            PopulateAntiAliasingModeDropdown();
            PopulateAntiAliasingQualityDropdown();
            PopulateShadowCascadeDropdown();
        }

        private void PopulateResolutionDropdown() {
            if (resolutionDropdown == null) {
                return;
            }
            resolutionArray = GraphicsSettings.GetAvailableResolutions();
            List<string> resolutionOptionList = new List<string>();
            foreach (Resolution resolution in resolutionArray) {
                resolutionOptionList.Add($"{resolution.width} x {resolution.height}");
            }
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutionOptionList);
        }

        private void PopulateMsaaDropdown() {
            if (msaaDropdown == null) return;
            msaaSampleCountArray = new int[] {
                MSAA_SAMPLE_COUNT_1, MSAA_SAMPLE_COUNT_2,
                MSAA_SAMPLE_COUNT_4, MSAA_SAMPLE_COUNT_8
            };
            List<string> msaaOptionList = new List<string> { "Off (1\u00d7)", "2\u00d7 MSAA", "4\u00d7 MSAA", "8\u00d7 MSAA" };
            msaaDropdown.ClearOptions();
            msaaDropdown.AddOptions(msaaOptionList);
        }

        private void PopulateAntiAliasingModeDropdown() {
            if (antiAliasingModeDropdown == null) return;
            List<string> antialiasingModeOptionList = new List<string>(Enum.GetNames(typeof(AntialiasingMode)));
            antiAliasingModeDropdown.ClearOptions();
            antiAliasingModeDropdown.AddOptions(antialiasingModeOptionList);
        }

        private void PopulateAntiAliasingQualityDropdown() {
            if (antiAliasingQualityDropdown == null) return;
            List<string> antialiasingQualityOptionList = new List<string>(Enum.GetNames(typeof(AntialiasingQuality)));
            antiAliasingQualityDropdown.ClearOptions();
            antiAliasingQualityDropdown.AddOptions(antialiasingQualityOptionList);
        }

        private void PopulateShadowCascadeDropdown() {
            if (shadowCascadeDropdown == null) return;
            List<string> shadowCascadeOptionList = new List<string>();
            for (int cascadeCount = SHADOW_CASCADE_MIN; cascadeCount <= SHADOW_CASCADE_MAX; cascadeCount++) {
                shadowCascadeOptionList.Add(cascadeCount == 1 ? "1 Cascade" : $"{cascadeCount} Cascades");
            }
            shadowCascadeDropdown.ClearOptions();
            shadowCascadeDropdown.AddOptions(shadowCascadeOptionList);
        }


        private void InitializeControlValues() {
            InitializeToggles();
            InitializeSliders();
            InitializeDropdowns();
        }

        private void InitializeToggles() {
            bool isFullscreen = GraphicsSettings.GetFullscreenMode() != FullScreenMode.Windowed;
            fullscreenToggle?.SetIsOnWithoutNotify(isFullscreen);
            vSyncToggle?.SetIsOnWithoutNotify(GraphicsSettings.IsVSyncEnabled());
            postProcessingToggle?.SetIsOnWithoutNotify(GraphicsSettings.IsPostProcessingEnabled());
            bloomToggle?.SetIsOnWithoutNotify(GraphicsSettings.GetBloomEnabled());
            motionBlurToggle?.SetIsOnWithoutNotify(GraphicsSettings.GetMotionBlurEnabled());
            vignetteToggle?.SetIsOnWithoutNotify(GraphicsSettings.GetVignetteEnabled());
            depthOfFieldToggle?.SetIsOnWithoutNotify(GraphicsSettings.GetDepthOfFieldEnabled());
            ambientOcclusionToggle?.SetIsOnWithoutNotify(GraphicsSettings.IsAmbientOcclusionEnabled());
        }

        private void InitializeSliders() {
            brightnessSlider?.SetValueWithoutNotify(GraphicsSettings.GetBrightness());
            gammaSlider?.SetValueWithoutNotify(GraphicsSettings.GetGamma());
            bloomIntensitySlider?.SetValueWithoutNotify(GraphicsSettings.GetBloomIntensity());
            renderScaleSlider?.SetValueWithoutNotify(GraphicsSettings.GetRenderScale());
            shadowDistanceSlider?.SetValueWithoutNotify(GraphicsSettings.GetShadowDistance());

            brightnessSliderTextMesh.SetText(GraphicsSettings.GetBrightness().ToString("F2"));
            gammaSliderTextMesh.SetText(GraphicsSettings.GetGamma().ToString("F2"));
            bloomIntensitySliderTextMesh.SetText(GraphicsSettings.GetBloomIntensity().ToString("F2"));
            renderScaleSliderTextMesh.SetText(GraphicsSettings.GetRenderScale().ToString("F2"));
            shadowDistanceSliderTextMesh.SetText(GraphicsSettings.GetShadowDistance().ToString("F2"));
        }

        private void InitializeDropdowns() {
            InitializeResolutionDropdown();
            InitializeMsaaDropdown();
            antiAliasingModeDropdown?.SetValueWithoutNotify((int)GraphicsSettings.GetAntiAliasingMode());
            antiAliasingQualityDropdown?.SetValueWithoutNotify((int)GraphicsSettings.GetAntiAliasingQuality());
            shadowCascadeDropdown?.SetValueWithoutNotify(GraphicsSettings.GetShadowCascadeCount() - SHADOW_CASCADE_MIN);
        }

        private void InitializeResolutionDropdown() {
            if (resolutionDropdown == null || resolutionArray == null) return;
            Resolution currentResolution = Screen.currentResolution;
            int bestMatchIndex = 0;
            for (int i = 0; i < resolutionArray.Length; i++) {
                if (resolutionArray[i].width  == currentResolution.width &&
                    resolutionArray[i].height == currentResolution.height) {
                    bestMatchIndex = i;
                    break;
                }
            }
            resolutionDropdown.SetValueWithoutNotify(bestMatchIndex);
        }

        private void InitializeMsaaDropdown() {
            if (msaaDropdown == null || msaaSampleCountArray == null) return;
            int currentMsaaSampleCount = GraphicsSettings.GetAntiAliasingMSAA();
            int bestMatchIndex = 0;
            for (int i = 0; i < msaaSampleCountArray.Length; i++) {
                if (msaaSampleCountArray[i] == currentMsaaSampleCount) {
                    bestMatchIndex = i;
                    break;
                }
            }
            msaaDropdown.SetValueWithoutNotify(bestMatchIndex);
        }


        private void RegisterListeners() {
            fullscreenToggle?.onValueChanged.AddListener(OnFullscreenToggleChanged);
            vSyncToggle?.onValueChanged.AddListener(OnVSyncToggleChanged);
            postProcessingToggle?.onValueChanged.AddListener(OnPostProcessingToggleChanged);
            bloomToggle?.onValueChanged.AddListener(OnBloomToggleChanged);
            motionBlurToggle?.onValueChanged.AddListener(OnMotionBlurToggleChanged);
            vignetteToggle?.onValueChanged.AddListener(OnVignetteToggleChanged);
            depthOfFieldToggle?.onValueChanged.AddListener(OnDepthOfFieldToggleChanged);
            ambientOcclusionToggle?.onValueChanged.AddListener(OnAmbientOcclusionToggleChanged);

            brightnessSlider?.onValueChanged.AddListener(OnBrightnessSliderChanged);
            gammaSlider?.onValueChanged.AddListener(OnGammaSliderChanged);
            bloomIntensitySlider?.onValueChanged.AddListener(OnBloomIntensitySliderChanged);
            renderScaleSlider?.onValueChanged.AddListener(OnRenderScaleSliderChanged);
            shadowDistanceSlider?.onValueChanged.AddListener(OnShadowDistanceSliderChanged);

            resolutionDropdown?.onValueChanged.AddListener(OnResolutionDropdownChanged);
            msaaDropdown?.onValueChanged.AddListener(OnMsaaDropdownChanged);
            antiAliasingModeDropdown?.onValueChanged.AddListener(OnAntiAliasingModeDropdownChanged);
            antiAliasingQualityDropdown?.onValueChanged.AddListener(OnAntiAliasingQualityDropdownChanged);
            shadowCascadeDropdown?.onValueChanged.AddListener(OnShadowCascadeDropdownChanged);
        }

        private void RemoveListeners() {
            fullscreenToggle?.onValueChanged.RemoveAllListeners();
            vSyncToggle?.onValueChanged.RemoveAllListeners();
            postProcessingToggle?.onValueChanged.RemoveAllListeners();
            bloomToggle?.onValueChanged.RemoveAllListeners();
            motionBlurToggle?.onValueChanged.RemoveAllListeners();
            vignetteToggle?.onValueChanged.RemoveAllListeners();
            depthOfFieldToggle?.onValueChanged.RemoveAllListeners();
            ambientOcclusionToggle?.onValueChanged.RemoveAllListeners();

            brightnessSlider?.onValueChanged.RemoveAllListeners();
            gammaSlider?.onValueChanged.RemoveAllListeners();
            bloomIntensitySlider?.onValueChanged.RemoveAllListeners();
            renderScaleSlider?.onValueChanged.RemoveAllListeners();
            shadowDistanceSlider?.onValueChanged.RemoveAllListeners();

            resolutionDropdown?.onValueChanged.RemoveAllListeners();
            msaaDropdown?.onValueChanged.RemoveAllListeners();
            antiAliasingModeDropdown?.onValueChanged.RemoveAllListeners();
            antiAliasingQualityDropdown?.onValueChanged.RemoveAllListeners();
            shadowCascadeDropdown?.onValueChanged.RemoveAllListeners();
        }


        private void OnFullscreenToggleChanged(bool isOn) {
            GraphicsSettings.SetFullscreen(isOn);
        }

        private void OnVSyncToggleChanged(bool isOn) {
            GraphicsSettings.SetVSync(isOn);
        }

        private void OnPostProcessingToggleChanged(bool isOn) {
            GraphicsSettings.SetPostProcessingEnabled(targetCamera, isOn);
        }

        private void OnBloomToggleChanged(bool isOn) {
            GraphicsSettings.SetBloomEnabled(globalVolume, isOn);
        }

        private void OnMotionBlurToggleChanged(bool isOn) {
            GraphicsSettings.SetMotionBlurEnabled(globalVolume, isOn);
        }

        private void OnVignetteToggleChanged(bool isOn) {
            GraphicsSettings.SetVignetteEnabled(globalVolume, isOn);
        }

        private void OnDepthOfFieldToggleChanged(bool isOn) {
            GraphicsSettings.SetDepthOfFieldEnabled(globalVolume, isOn);
        }

        private void OnAmbientOcclusionToggleChanged(bool isOn) {
            GraphicsSettings.SetAmbientOcclusionEnabled(ambientOcclusionFeature, isOn);
        }


        private void OnBrightnessSliderChanged(float value) {
            GraphicsSettings.SetBrightness(globalVolume, value);
            brightnessSliderTextMesh.SetText(value.ToString("F2"));
        }

        private void OnGammaSliderChanged(float value) {
            GraphicsSettings.SetGamma(globalVolume, value);
            gammaSliderTextMesh.SetText(value.ToString("F2"));
        }

        private void OnBloomIntensitySliderChanged(float value) {
            GraphicsSettings.SetBloomIntensity(globalVolume, value);
            bloomIntensitySliderTextMesh.SetText(value.ToString("F2"));
        }

        private void OnRenderScaleSliderChanged(float value) {
            GraphicsSettings.SetRenderScale(value);
            renderScaleSliderTextMesh.SetText(value.ToString("F2"));
        }

        private void OnShadowDistanceSliderChanged(float value) {
            GraphicsSettings.SetShadowDistance(value);
            shadowDistanceSliderTextMesh.SetText(value.ToString("F2"));
        }


        private void OnResolutionDropdownChanged(int index) {
            if (resolutionArray == null || index < 0 || index >= resolutionArray.Length) return;
            Resolution selectedResolution = resolutionArray[index];
            GraphicsSettings.SetResolution(selectedResolution.width, selectedResolution.height, GraphicsSettings.GetFullscreenMode());
        }

        private void OnMsaaDropdownChanged(int index) {
            if (msaaSampleCountArray == null || index < 0 || index >= msaaSampleCountArray.Length) return;
            GraphicsSettings.SetAntiAliasingMSAA(msaaSampleCountArray[index]);
        }

        private void OnAntiAliasingModeDropdownChanged(int index) {
            GraphicsSettings.SetAntiAliasingMode(targetCamera, (AntialiasingMode)index);
        }

        private void OnAntiAliasingQualityDropdownChanged(int index) {
            GraphicsSettings.SetAntiAliasingQuality(targetCamera, (AntialiasingQuality)index);
        }

        private void OnShadowCascadeDropdownChanged(int index) {
            GraphicsSettings.SetShadowCascadeCount(index + SHADOW_CASCADE_MIN);
        }

    }

}
