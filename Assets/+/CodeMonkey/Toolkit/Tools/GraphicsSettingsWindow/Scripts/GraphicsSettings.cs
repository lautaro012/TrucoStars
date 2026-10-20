using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CodeMonkey.Toolkit.TGraphicsSettings {

    /// <summary>
    /// ** Graphics Settings **
    ///
    /// Static utility class for reading and writing common graphics settings at runtime.
    /// Covers display (fullscreen mode, resolution, vsync, target frame rate),
    /// URP pipeline (MSAA, shadow distance, shadow cascades, render scale),
    /// camera post-processing toggle and anti-aliasing mode/quality,
    /// volume-based overrides (bloom, motion blur, vignette, depth of field,
    /// brightness via post-exposure, gamma via LiftGammaGain), and ambient occlusion
    /// (URP SSAO renderer feature).
    ///
    /// Every setting is persisted to PlayerPrefs using named constant keys so
    /// values survive sessions. 
    /// 
    /// Call LoadAndApplyAll() on startup to restore them, or just use the included prefab.
    /// </summary>
    public static class GraphicsSettings {


        // ─────────────────────────────────────────────────────────────
        // PlayerPrefs Keys
        // ─────────────────────────────────────────────────────────────

        private const string FULLSCREEN_MODE_KEY          = "GraphicsSettings_FullscreenMode";
        private const string RESOLUTION_WIDTH_KEY         = "GraphicsSettings_ResolutionWidth";
        private const string RESOLUTION_HEIGHT_KEY        = "GraphicsSettings_ResolutionHeight";
        private const string VSYNC_KEY                    = "GraphicsSettings_VSync";
        private const string TARGET_FRAME_RATE_KEY        = "GraphicsSettings_TargetFrameRate";
        private const string ANTI_ALIASING_MSAA_KEY       = "GraphicsSettings_AntiAliasingMSAA";
        private const string SHADOW_DISTANCE_KEY          = "GraphicsSettings_ShadowDistance";
        private const string SHADOW_CASCADE_COUNT_KEY     = "GraphicsSettings_ShadowCascadeCount";
        private const string RENDER_SCALE_KEY             = "GraphicsSettings_RenderScale";
        private const string POST_PROCESSING_ENABLED_KEY  = "GraphicsSettings_PostProcessingEnabled";
        private const string ANTI_ALIASING_MODE_KEY       = "GraphicsSettings_AntiAliasingMode";
        private const string ANTI_ALIASING_QUALITY_KEY    = "GraphicsSettings_AntiAliasingQuality";
        private const string BLOOM_ENABLED_KEY            = "GraphicsSettings_BloomEnabled";
        private const string BLOOM_INTENSITY_KEY          = "GraphicsSettings_BloomIntensity";
        private const string MOTION_BLUR_ENABLED_KEY      = "GraphicsSettings_MotionBlurEnabled";
        private const string MOTION_BLUR_INTENSITY_KEY    = "GraphicsSettings_MotionBlurIntensity";
        private const string VIGNETTE_ENABLED_KEY         = "GraphicsSettings_VignetteEnabled";
        private const string DEPTH_OF_FIELD_ENABLED_KEY   = "GraphicsSettings_DepthOfFieldEnabled";
        private const string BRIGHTNESS_KEY               = "GraphicsSettings_Brightness";
        private const string GAMMA_KEY                    = "GraphicsSettings_Gamma";
        private const string AMBIENT_OCCLUSION_ENABLED_KEY = "GraphicsSettings_AmbientOcclusionEnabled";


        // ─────────────────────────────────────────────────────────────
        // Default Values
        // ─────────────────────────────────────────────────────────────

        private const int   DEFAULT_FULLSCREEN_MODE             = (int)FullScreenMode.FullScreenWindow;
        private const int   DEFAULT_VSYNC_ENABLED               = 1;   // 1 = on, 0 = off
        private const int   DEFAULT_TARGET_FRAME_RATE           = -1;  // unlimited
        private const int   DEFAULT_ANTI_ALIASING_MSAA          = 1;   // 1 sample = MSAA disabled
        private const float DEFAULT_SHADOW_DISTANCE             = 50f;
        private const int   DEFAULT_SHADOW_CASCADE_COUNT        = 4;
        private const float DEFAULT_RENDER_SCALE                = 1f;
        private const int   DEFAULT_POST_PROCESSING_ENABLED     = 1;
        private const int   DEFAULT_ANTI_ALIASING_MODE          = (int)AntialiasingMode.None;
        private const int   DEFAULT_ANTI_ALIASING_QUALITY       = (int)AntialiasingQuality.High;
        private const int   DEFAULT_BLOOM_ENABLED               = 1;
        private const float DEFAULT_BLOOM_INTENSITY             = 1f;
        private const int   DEFAULT_MOTION_BLUR_ENABLED         = 0;
        private const float DEFAULT_MOTION_BLUR_INTENSITY       = 0.5f;
        private const int   DEFAULT_VIGNETTE_ENABLED            = 0;
        private const int   DEFAULT_DEPTH_OF_FIELD_ENABLED      = 0;
        private const float DEFAULT_BRIGHTNESS                  = 0f;  // postExposure: 0 EV = neutral
        private const float DEFAULT_GAMMA                       = 0f;  // LiftGammaGain w-channel: 0 = neutral
        private const int   DEFAULT_AMBIENT_OCCLUSION_ENABLED   = 1;

        private const float RENDER_SCALE_MIN          = 0.1f;
        private const float RENDER_SCALE_MAX          = 2f;
        private const int   SHADOW_CASCADE_COUNT_MIN  = 1;
        private const int   SHADOW_CASCADE_COUNT_MAX  = 4;
        private const int   VSYNC_DISABLED            = 0;
        private const int   VSYNC_ENABLED             = 1;
        private const int   BOOL_TRUE                 = 1;
        private const int   BOOL_FALSE                = 0;


        // ─────────────────────────────────────────────────────────────
        // Private Helpers
        // ─────────────────────────────────────────────────────────────

        private static UniversalRenderPipelineAsset GetURPAsset() {
            return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        }

        private static void SavePrefs() {
            PlayerPrefs.Save();
        }


        // ─────────────────────────────────────────────────────────────
        // Display / Window Settings
        // ─────────────────────────────────────────────────────────────

        public static void SetFullscreenMode(FullScreenMode fullScreenMode) {
            Screen.fullScreenMode = fullScreenMode;
            PlayerPrefs.SetInt(FULLSCREEN_MODE_KEY, (int)fullScreenMode);
            SavePrefs();
        }

        public static FullScreenMode GetFullscreenMode() {
            return (FullScreenMode)PlayerPrefs.GetInt(FULLSCREEN_MODE_KEY, DEFAULT_FULLSCREEN_MODE);
        }

        /// <summary>
        /// Convenience wrapper: true maps to FullScreenWindow, false maps to Windowed.
        /// </summary>
        public static void SetFullscreen(bool isFullscreen) {
            FullScreenMode fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            SetFullscreenMode(fullScreenMode);
        }

        /// <summary>
        /// Convenience wrapper: true maps to Windowed, false maps to FullScreenWindow.
        /// </summary>
        public static void SetWindowed(bool isWindowed) {
            SetFullscreen(!isWindowed);
        }

        public static void SetResolution(int width, int height, FullScreenMode fullScreenMode) {
            Screen.SetResolution(width, height, fullScreenMode);
            PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY,  width);
            PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, height);
            PlayerPrefs.SetInt(FULLSCREEN_MODE_KEY,   (int)fullScreenMode);
            SavePrefs();
        }

        /// <summary>Returns all resolutions supported by the current display.</summary>
        public static Resolution[] GetAvailableResolutions() {
            return Screen.resolutions;
        }

        public static void SetVSync(bool isVSyncEnabled) {
            QualitySettings.vSyncCount = isVSyncEnabled ? VSYNC_ENABLED : VSYNC_DISABLED;
            PlayerPrefs.SetInt(VSYNC_KEY, isVSyncEnabled ? BOOL_TRUE : BOOL_FALSE);
            SavePrefs();
        }

        public static bool IsVSyncEnabled() {
            return PlayerPrefs.GetInt(VSYNC_KEY, DEFAULT_VSYNC_ENABLED) != BOOL_FALSE;
        }

        public static void SetTargetFrameRate(int targetFrameRate) {
            Application.targetFrameRate = targetFrameRate;
            PlayerPrefs.SetInt(TARGET_FRAME_RATE_KEY, targetFrameRate);
            SavePrefs();
        }

        public static int GetTargetFrameRate() {
            return PlayerPrefs.GetInt(TARGET_FRAME_RATE_KEY, DEFAULT_TARGET_FRAME_RATE);
        }


        // ─────────────────────────────────────────────────────────────
        // URP Pipeline Asset Settings
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Sets hardware MSAA on the active URP asset. Valid sample counts: 1 (off), 2, 4, 8.
        /// </summary>
        public static void SetAntiAliasingMSAA(int msaaSampleCount) {
            UniversalRenderPipelineAsset urpAsset = GetURPAsset();
            if (urpAsset == null) {
                Debug.LogWarning("GraphicsSettings: URP asset not found — cannot apply MSAA setting.");
                return;
            }
            urpAsset.msaaSampleCount = msaaSampleCount;
            PlayerPrefs.SetInt(ANTI_ALIASING_MSAA_KEY, msaaSampleCount);
            SavePrefs();
        }

        public static int GetAntiAliasingMSAA() {
            return PlayerPrefs.GetInt(ANTI_ALIASING_MSAA_KEY, DEFAULT_ANTI_ALIASING_MSAA);
        }

        public static void SetShadowDistance(float shadowDistance) {
            UniversalRenderPipelineAsset urpAsset = GetURPAsset();
            if (urpAsset == null) {
                Debug.LogWarning("GraphicsSettings: URP asset not found — cannot apply shadow distance.");
                return;
            }
            urpAsset.shadowDistance = shadowDistance;
            PlayerPrefs.SetFloat(SHADOW_DISTANCE_KEY, shadowDistance);
            SavePrefs();
        }

        public static float GetShadowDistance() {
            return PlayerPrefs.GetFloat(SHADOW_DISTANCE_KEY, DEFAULT_SHADOW_DISTANCE);
        }

        /// <summary>
        /// Sets the directional shadow cascade count. Value is clamped to 1–4.
        /// </summary>
        public static void SetShadowCascadeCount(int shadowCascadeCount) {
            UniversalRenderPipelineAsset urpAsset = GetURPAsset();
            if (urpAsset == null) {
                Debug.LogWarning("GraphicsSettings: URP asset not found — cannot apply shadow cascade count.");
                return;
            }
            int clampedShadowCascadeCount = Mathf.Clamp(shadowCascadeCount, SHADOW_CASCADE_COUNT_MIN, SHADOW_CASCADE_COUNT_MAX);
            urpAsset.shadowCascadeCount = clampedShadowCascadeCount;
            PlayerPrefs.SetInt(SHADOW_CASCADE_COUNT_KEY, clampedShadowCascadeCount);
            SavePrefs();
        }

        public static int GetShadowCascadeCount() {
            return PlayerPrefs.GetInt(SHADOW_CASCADE_COUNT_KEY, DEFAULT_SHADOW_CASCADE_COUNT);
        }

        /// <summary>
        /// Sets the render resolution scale on the URP asset. Value is clamped to 0.1–2.0.
        /// </summary>
        public static void SetRenderScale(float renderScale) {
            UniversalRenderPipelineAsset urpAsset = GetURPAsset();
            if (urpAsset == null) {
                Debug.LogWarning("GraphicsSettings: URP asset not found — cannot apply render scale.");
                return;
            }
            float clampedRenderScale = Mathf.Clamp(renderScale, RENDER_SCALE_MIN, RENDER_SCALE_MAX);
            urpAsset.renderScale = clampedRenderScale;
            PlayerPrefs.SetFloat(RENDER_SCALE_KEY, clampedRenderScale);
            SavePrefs();
        }

        public static float GetRenderScale() {
            return PlayerPrefs.GetFloat(RENDER_SCALE_KEY, DEFAULT_RENDER_SCALE);
        }


        // ─────────────────────────────────────────────────────────────
        // Camera Settings
        // ─────────────────────────────────────────────────────────────

        public static void SetPostProcessingEnabled(Camera camera, bool isPostProcessingEnabled) {
            if (camera == null) {
                Debug.LogWarning("GraphicsSettings: Camera is null — cannot set post-processing enabled.");
                return;
            }
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            if (cameraData == null) {
                Debug.LogWarning("GraphicsSettings: UniversalAdditionalCameraData not found on camera.");
                return;
            }
            cameraData.renderPostProcessing = isPostProcessingEnabled;
            PlayerPrefs.SetInt(POST_PROCESSING_ENABLED_KEY, isPostProcessingEnabled ? BOOL_TRUE : BOOL_FALSE);
            SavePrefs();
        }

        public static bool IsPostProcessingEnabled() {
            return PlayerPrefs.GetInt(POST_PROCESSING_ENABLED_KEY, DEFAULT_POST_PROCESSING_ENABLED) != BOOL_FALSE;
        }

        /// <summary>
        /// Sets the post-process anti-aliasing mode on the camera (FXAA, SMAA, TAA, or None).
        /// This is independent of hardware MSAA set via SetAntiAliasingMSAA.
        /// </summary>
        public static void SetAntiAliasingMode(Camera camera, AntialiasingMode antialiasingMode) {
            if (camera == null) {
                Debug.LogWarning("GraphicsSettings: Camera is null — cannot set anti-aliasing mode.");
                return;
            }
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            if (cameraData == null) {
                Debug.LogWarning("GraphicsSettings: UniversalAdditionalCameraData not found on camera.");
                return;
            }
            cameraData.antialiasing = antialiasingMode;
            PlayerPrefs.SetInt(ANTI_ALIASING_MODE_KEY, (int)antialiasingMode);
            SavePrefs();
        }

        /// <summary>Returns the stored post-AA mode without requiring a Camera reference.</summary>
        public static AntialiasingMode GetAntiAliasingMode() {
            return (AntialiasingMode)PlayerPrefs.GetInt(ANTI_ALIASING_MODE_KEY, DEFAULT_ANTI_ALIASING_MODE);
        }

        public static void SetAntiAliasingQuality(Camera camera, AntialiasingQuality antialiasingQuality) {
            if (camera == null) {
                Debug.LogWarning("GraphicsSettings: Camera is null — cannot set anti-aliasing quality.");
                return;
            }
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            if (cameraData == null) {
                Debug.LogWarning("GraphicsSettings: UniversalAdditionalCameraData not found on camera.");
                return;
            }
            cameraData.antialiasingQuality = antialiasingQuality;
            PlayerPrefs.SetInt(ANTI_ALIASING_QUALITY_KEY, (int)antialiasingQuality);
            SavePrefs();
        }

        public static AntialiasingQuality GetAntiAliasingQuality() {
            return (AntialiasingQuality)PlayerPrefs.GetInt(ANTI_ALIASING_QUALITY_KEY, DEFAULT_ANTI_ALIASING_QUALITY);
        }


        // ─────────────────────────────────────────────────────────────
        // Volume-Based Post-Processing
        // ─────────────────────────────────────────────────────────────

        public static void SetBloomEnabled(Volume volume, bool isBloomEnabled) {
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out Bloom bloom)) return;
            bloom.active = isBloomEnabled;
            PlayerPrefs.SetInt(BLOOM_ENABLED_KEY, isBloomEnabled ? BOOL_TRUE : BOOL_FALSE);
            SavePrefs();
        }

        public static bool GetBloomEnabled() {
            return PlayerPrefs.GetInt(BLOOM_ENABLED_KEY, DEFAULT_BLOOM_ENABLED) != BOOL_FALSE;
        }

        public static void SetBloomIntensity(Volume volume, float bloomIntensity) {
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out Bloom bloom)) return;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = bloomIntensity;
            PlayerPrefs.SetFloat(BLOOM_INTENSITY_KEY, bloomIntensity);
            SavePrefs();
        }

        public static float GetBloomIntensity() {
            return PlayerPrefs.GetFloat(BLOOM_INTENSITY_KEY, DEFAULT_BLOOM_INTENSITY);
        }

        public static void SetMotionBlurEnabled(Volume volume, bool isMotionBlurEnabled) {
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out MotionBlur motionBlur)) return;
            motionBlur.active = isMotionBlurEnabled;
            PlayerPrefs.SetInt(MOTION_BLUR_ENABLED_KEY, isMotionBlurEnabled ? BOOL_TRUE : BOOL_FALSE);
            SavePrefs();
        }

        public static bool GetMotionBlurEnabled() {
            return PlayerPrefs.GetInt(MOTION_BLUR_ENABLED_KEY, DEFAULT_MOTION_BLUR_ENABLED) != BOOL_FALSE;
        }

        public static void SetMotionBlurIntensity(Volume volume, float motionBlurIntensity) {
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out MotionBlur motionBlur)) return;
            motionBlur.intensity.overrideState = true;
            motionBlur.intensity.value = motionBlurIntensity;
            PlayerPrefs.SetFloat(MOTION_BLUR_INTENSITY_KEY, motionBlurIntensity);
            SavePrefs();
        }

        public static float GetMotionBlurIntensity() {
            return PlayerPrefs.GetFloat(MOTION_BLUR_INTENSITY_KEY, DEFAULT_MOTION_BLUR_INTENSITY);
        }

        public static void SetVignetteEnabled(Volume volume, bool isVignetteEnabled) {
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out Vignette vignette)) return;
            vignette.active = isVignetteEnabled;
            PlayerPrefs.SetInt(VIGNETTE_ENABLED_KEY, isVignetteEnabled ? BOOL_TRUE : BOOL_FALSE);
            SavePrefs();
        }

        public static bool GetVignetteEnabled() {
            return PlayerPrefs.GetInt(VIGNETTE_ENABLED_KEY, DEFAULT_VIGNETTE_ENABLED) != BOOL_FALSE;
        }

        public static void SetDepthOfFieldEnabled(Volume volume, bool isDepthOfFieldEnabled) {
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out DepthOfField depthOfField)) return;
            depthOfField.active = isDepthOfFieldEnabled;
            PlayerPrefs.SetInt(DEPTH_OF_FIELD_ENABLED_KEY, isDepthOfFieldEnabled ? BOOL_TRUE : BOOL_FALSE);
            SavePrefs();
        }

        public static bool GetDepthOfFieldEnabled() {
            return PlayerPrefs.GetInt(DEPTH_OF_FIELD_ENABLED_KEY, DEFAULT_DEPTH_OF_FIELD_ENABLED) != BOOL_FALSE;
        }

        /// <summary>
        /// Controls screen brightness via ColorAdjustments.postExposure (EV offset; 0 = neutral).
        /// </summary>
        public static void SetBrightness(Volume volume, float brightness) {
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out ColorAdjustments colorAdjustments)) return;
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = brightness;
            PlayerPrefs.SetFloat(BRIGHTNESS_KEY, brightness);
            SavePrefs();
        }

        public static float GetBrightness() {
            return PlayerPrefs.GetFloat(BRIGHTNESS_KEY, DEFAULT_BRIGHTNESS);
        }

        /// <summary>
        /// Applies a uniform gamma adjustment via LiftGammaGain.gamma.
        /// The same float value is written to all four channels (x, y, z, w) of the Vector4.
        /// 0 is neutral; negative values darken midtones, positive values brighten them.
        /// </summary>
        public static void SetGamma(Volume volume, float gamma) {
            if (volume == null || volume.profile == null) return;
            if (!volume.profile.TryGet(out LiftGammaGain liftGammaGain)) return;
            liftGammaGain.gamma.overrideState = true;
            liftGammaGain.gamma.value = new Vector4(gamma, gamma, gamma, gamma);
            PlayerPrefs.SetFloat(GAMMA_KEY, gamma);
            SavePrefs();
        }

        public static float GetGamma() {
            return PlayerPrefs.GetFloat(GAMMA_KEY, DEFAULT_GAMMA);
        }


        // ─────────────────────────────────────────────────────────────
        // Ambient Occlusion
        // ─────────────────────────────────────────────────────────────

        // In URP, Screen Space Ambient Occlusion (SSAO) is implemented as a
        // ScriptableRendererFeature, not a Volume component override.  The caller
        // is responsible for obtaining the feature reference (e.g., from the active
        // renderer asset) and passing it here; this avoids reflection entirely.
        public static void SetAmbientOcclusionEnabled(ScriptableRendererFeature ambientOcclusionFeature, bool isAmbientOcclusionEnabled) {
            if (ambientOcclusionFeature == null) {
                Debug.LogWarning("GraphicsSettings: Ambient occlusion renderer feature is null — cannot apply setting.");
                return;
            }
            ambientOcclusionFeature.SetActive(isAmbientOcclusionEnabled);
            PlayerPrefs.SetInt(AMBIENT_OCCLUSION_ENABLED_KEY, isAmbientOcclusionEnabled ? BOOL_TRUE : BOOL_FALSE);
            SavePrefs();
        }

        public static bool IsAmbientOcclusionEnabled() {
            return PlayerPrefs.GetInt(AMBIENT_OCCLUSION_ENABLED_KEY, DEFAULT_AMBIENT_OCCLUSION_ENABLED) != BOOL_FALSE;
        }


        // ─────────────────────────────────────────────────────────────
        // Startup: Load and Apply All Persisted Settings
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads every persisted PlayerPrefs value and applies it to the engine/objects.
        /// Call this once during startup (e.g., from an initialisation MonoBehaviour).
        /// Pass null for any argument whose sub-system is not available in the current scene —
        /// that section will be skipped gracefully without throwing.
        /// </summary>
        public static void LoadAndApplyAll(Volume volume, Camera camera, ScriptableRendererFeature ambientOcclusionFeature) {
            // Display / Window
            FullScreenMode storedFullscreenMode = GetFullscreenMode();
            int storedResolutionWidth  = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY,  Screen.currentResolution.width);
            int storedResolutionHeight = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, Screen.currentResolution.height);
            Screen.SetResolution(storedResolutionWidth, storedResolutionHeight, storedFullscreenMode);

            QualitySettings.vSyncCount = IsVSyncEnabled() ? VSYNC_ENABLED : VSYNC_DISABLED;
            Application.targetFrameRate = GetTargetFrameRate();

            // URP Asset
            UniversalRenderPipelineAsset urpAsset = GetURPAsset();
            if (urpAsset != null) {
                urpAsset.msaaSampleCount    = GetAntiAliasingMSAA();
                urpAsset.shadowDistance     = GetShadowDistance();
                urpAsset.shadowCascadeCount = GetShadowCascadeCount();
                urpAsset.renderScale        = GetRenderScale();
            }

            // Camera
            if (camera != null) {
                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                if (cameraData != null) {
                    cameraData.renderPostProcessing  = IsPostProcessingEnabled();
                    cameraData.antialiasing          = GetAntiAliasingMode();
                    cameraData.antialiasingQuality   = GetAntiAliasingQuality();
                }
            }

            // Volume Post-Processing
            if (volume != null && volume.profile != null) {
                if (volume.profile.TryGet(out Bloom bloom)) {
                    bloom.active                   = GetBloomEnabled();
                    bloom.intensity.overrideState  = true;
                    bloom.intensity.value          = GetBloomIntensity();
                }
                if (volume.profile.TryGet(out MotionBlur motionBlur)) {
                    motionBlur.active                  = GetMotionBlurEnabled();
                    motionBlur.intensity.overrideState = true;
                    motionBlur.intensity.value         = GetMotionBlurIntensity();
                }
                if (volume.profile.TryGet(out Vignette vignette)) {
                    vignette.active = GetVignetteEnabled();
                }
                if (volume.profile.TryGet(out DepthOfField depthOfField)) {
                    depthOfField.active = GetDepthOfFieldEnabled();
                }
                if (volume.profile.TryGet(out ColorAdjustments colorAdjustments)) {
                    colorAdjustments.postExposure.overrideState = true;
                    colorAdjustments.postExposure.value         = GetBrightness();
                }
                if (volume.profile.TryGet(out LiftGammaGain liftGammaGain)) {
                    float storedGamma = GetGamma();
                    liftGammaGain.gamma.overrideState = true;
                    liftGammaGain.gamma.value         = new Vector4(storedGamma, storedGamma, storedGamma, storedGamma);
                }
            }

            // Ambient Occlusion
            if (ambientOcclusionFeature != null) {
                ambientOcclusionFeature.SetActive(IsAmbientOcclusionEnabled());
            }
        }


    }

}
