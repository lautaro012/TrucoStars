using CodeMonkey.Toolkit.TFunctionPeriodic;
using CodeMonkey.Toolkit.TTextPopup;
using UnityEngine;

namespace PopupDemo {

    /// <summary>
    /// Auto cube: spawns a world-space text popup showing an increasing
    /// number every [interval] seconds, without any player input.
    /// </summary>
    public class AutoPopupCube : MonoBehaviour {

        [SerializeField] private float interval = 1f;
        [SerializeField] private float popupHeight = 1.5f;

        private int counter;

        private void Start() {
            FunctionPeriodic.Create(SpawnPopup, interval);
        }

        private void SpawnPopup() {
            counter++;
            TextPopupWorld.Create(transform.position + Vector3.up * popupHeight, counter.ToString());
        }

    }

}
