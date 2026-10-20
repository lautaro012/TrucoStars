using System.Collections.Generic;
using CodeMonkey.Toolkit.TInteractionSystemProximity;
using CodeMonkey.Toolkit.TTextPopup;
using UnityEngine;

namespace PopupDemo {

    /// <summary>
    /// Interactable cube: each time the player presses E within range,
    /// spawns a world-space text popup showing an incrementing counter.
    /// </summary>
    public class CounterCubeInteractable : MonoBehaviour, IInteractable {

        [SerializeField] private float popupHeight = 1.5f;

        private int counter;

        public void Interact(IInteractable.InteractAction interactAction, Transform interactorTransform) {
            counter++;
            TextPopupWorld.Create(transform.position + Vector3.up * popupHeight, counter.ToString());
        }

        public Dictionary<IInteractable.InteractAction, string> GetInteractTextDictionary() {
            return new Dictionary<IInteractable.InteractAction, string> {
                { IInteractable.InteractAction.Primary, "Spawn number" }
            };
        }

        public Transform GetTransform() {
            return transform;
        }

        public bool CanDoInteractAction(IInteractable.InteractAction interactAction) {
            return interactAction == IInteractable.InteractAction.Primary;
        }

    }

}
