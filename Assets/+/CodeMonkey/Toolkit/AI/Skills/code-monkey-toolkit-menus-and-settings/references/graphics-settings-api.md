# GraphicsSettings — Full Static API

Reference for `CodeMonkey.Toolkit.TGraphicsSettings.GraphicsSettings`. Read this when the common setters in the SKILL.md API quick reference do not cover the setting you need. Every setter persists the value through `PlayerPrefs`; the matching getter reads the applied/saved value.

Add `using CodeMonkey.Toolkit.TGraphicsSettings;`. Volume-based and camera-based effects require the Universal Render Pipeline (URP); `Volume`, `AntialiasingMode`, `AntialiasingQuality`, and `ScriptableRendererFeature` come from `UnityEngine.Rendering` / `UnityEngine.Rendering.Universal`.

## Display

| Method | Notes |
|---|---|
| `void SetFullscreenMode(FullScreenMode fullScreenMode)` | Set fullscreen mode directly |
| `FullScreenMode GetFullscreenMode()` | Current fullscreen mode |
| `void SetFullscreen(bool isFullscreen)` | Fullscreen on/off |
| `void SetWindowed(bool isWindowed)` | Windowed on/off |
| `void SetResolution(int width, int height, FullScreenMode fullScreenMode)` | Set resolution + mode |
| `Resolution[] GetAvailableResolutions()` | Supported resolutions for the display |
| `void SetVSync(bool isVSyncEnabled)` | VSync on/off |
| `bool IsVSyncEnabled()` | VSync state |
| `void SetTargetFrameRate(int targetFrameRate)` | Frame-rate cap |
| `int GetTargetFrameRate()` | Current cap |

## Quality — anti-aliasing, shadows, render scale

| Method | Notes |
|---|---|
| `void SetAntiAliasingMSAA(int msaaSampleCount)` | MSAA sample count (0/2/4/8) |
| `int GetAntiAliasingMSAA()` | Current MSAA sample count |
| `void SetShadowDistance(float shadowDistance)` | Shadow draw distance |
| `float GetShadowDistance()` | Current shadow distance |
| `void SetShadowCascadeCount(int shadowCascadeCount)` | URP shadow cascade count |
| `int GetShadowCascadeCount()` | Current cascade count |
| `void SetRenderScale(float renderScale)` | URP render scale |
| `float GetRenderScale()` | Current render scale |

## Post-processing — camera-based (URP)

| Method | Notes |
|---|---|
| `void SetPostProcessingEnabled(Camera camera, bool isPostProcessingEnabled)` | Toggle post-processing on a camera |
| `bool IsPostProcessingEnabled()` | Saved state |
| `void SetAntiAliasingMode(Camera camera, AntialiasingMode antialiasingMode)` | FXAA / SMAA / TAA / None |
| `AntialiasingMode GetAntiAliasingMode()` | Current AA mode |
| `void SetAntiAliasingQuality(Camera camera, AntialiasingQuality antialiasingQuality)` | AA quality level |
| `AntialiasingQuality GetAntiAliasingQuality()` | Current AA quality |

## Post-processing — Volume-based (URP)

| Method | Notes |
|---|---|
| `void SetBloomEnabled(Volume volume, bool isBloomEnabled)` | Toggle bloom |
| `bool GetBloomEnabled()` | Bloom state |
| `void SetBloomIntensity(Volume volume, float bloomIntensity)` | Bloom intensity |
| `float GetBloomIntensity()` | Current bloom intensity |
| `void SetMotionBlurEnabled(Volume volume, bool isMotionBlurEnabled)` | Toggle motion blur |
| `bool GetMotionBlurEnabled()` | Motion blur state |
| `void SetMotionBlurIntensity(Volume volume, float motionBlurIntensity)` | Motion blur intensity |
| `float GetMotionBlurIntensity()` | Current motion blur intensity |
| `void SetVignetteEnabled(Volume volume, bool isVignetteEnabled)` | Toggle vignette |
| `bool GetVignetteEnabled()` | Vignette state |
| `void SetDepthOfFieldEnabled(Volume volume, bool isDepthOfFieldEnabled)` | Toggle depth of field |
| `bool GetDepthOfFieldEnabled()` | Depth of field state |
| `void SetBrightness(Volume volume, float brightness)` | Brightness (ColorAdjustments) |
| `float GetBrightness()` | Current brightness |
| `void SetGamma(Volume volume, float gamma)` | Gamma (LiftGammaGain) |
| `float GetGamma()` | Current gamma |

## Ambient occlusion (URP renderer feature)

| Method | Notes |
|---|---|
| `void SetAmbientOcclusionEnabled(ScriptableRendererFeature ambientOcclusionFeature, bool isAmbientOcclusionEnabled)` | Toggle the URP AO renderer feature |
| `bool IsAmbientOcclusionEnabled()` | Saved AO state |

## Aggregate

| Method | Notes |
|---|---|
| `void LoadAndApplyAll(Volume volume, Camera camera, ScriptableRendererFeature ambientOcclusionFeature)` | Reads every saved `PlayerPrefs` value and applies it at once. Used by `GraphicsSettingsBootstrap` on startup. |
