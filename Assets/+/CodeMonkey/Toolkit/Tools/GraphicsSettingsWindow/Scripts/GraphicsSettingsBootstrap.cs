using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CodeMonkey.Toolkit.TGraphicsSettings {

    /// <summary>
    /// ** Graphics Settings Bootstrap **
    ///
    /// Startup MonoBehaviour that reads all persisted PlayerPrefs settings and
    /// applies them to the engine, URP pipeline asset, camera, global post-processing
    /// volume, and ambient occlusion renderer feature on Awake.
    /// Attach to a persistent GameObject in the first loaded scene.
    /// If targetCamera is not assigned in the Inspector, falls back to Camera.main.
    /// </summary>
    public class GraphicsSettingsBootstrap : MonoBehaviour {


        [Header("Scene References")]
        [Tooltip("Global post-processing Volume to apply colour-grading and effect settings to.")]
        [SerializeField] private Volume globalVolume;

        [Tooltip("Camera that receives post-processing and anti-aliasing settings. Falls back to Camera.main when left empty.")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("SSAO ScriptableRendererFeature on the active URP renderer. Leave null if the renderer has no SSAO feature — it is skipped gracefully.")]
        [SerializeField] private ScriptableRendererFeature ambientOcclusionFeature;


        private void Awake() {
            if (targetCamera == null) {
                targetCamera = Camera.main;
            }
            GraphicsSettings.LoadAndApplyAll(globalVolume, targetCamera, ambientOcclusionFeature);
        }

    }

}
