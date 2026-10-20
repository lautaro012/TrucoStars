using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CodeMonkey.Toolkit.TShaderDissolve.Demo {

    public class Demo : MonoBehaviour {


        private const string DISSOLVE_AMOUNT = "_DissolveAmount";


        [SerializeField] private Material material;


        private float dissolveAmount;
        private bool dissolveActive;


        private void Start() {
            dissolveAmount = 0;
        }

        private void Update() {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.tKey.wasPressedThisFrame) {
#else
            if (Input.GetKeyDown(KeyCode.T)) {
#endif
                dissolveActive = !dissolveActive;
            }

            float dissolvespeed = 2f;
            if (dissolveActive) {
                dissolveAmount += dissolvespeed * Time.deltaTime;
            } else {
                dissolveAmount -= dissolvespeed * Time.deltaTime;
            }

            dissolveAmount = Mathf.Clamp(dissolveAmount, 0f, 1f);
            material.SetFloat(DISSOLVE_AMOUNT, dissolveAmount);
        }

    }

}