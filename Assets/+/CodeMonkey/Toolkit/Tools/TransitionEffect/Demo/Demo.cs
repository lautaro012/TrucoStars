using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CodeMonkey.Toolkit.TShaderTransitionEFfect.Demo {

    public class Demo : MonoBehaviour {


        private void Update() {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.tKey.wasPressedThisFrame) {
#else
            if (Input.GetKeyDown(KeyCode.T)) {
#endif
                ShaderTransitionEffect.Show(() => {
                    Debug.Log("Fully Black");
                });
            }
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.tKey.wasPressedThisFrame) {
#else
            if (Input.GetKeyDown(KeyCode.Y)) {
#endif
                ShaderTransitionEffect.Hide(() => {
                    Debug.Log("Fully Transparent");
                });
            }
        }

    }

}