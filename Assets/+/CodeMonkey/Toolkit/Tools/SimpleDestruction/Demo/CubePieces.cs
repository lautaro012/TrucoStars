using UnityEngine;


namespace CodeMonkey.Toolkit.TSimpleDestruction.Demo {

    public class CubePieces : MonoBehaviour {



        private void Start() {
            foreach (Transform child in transform) {
                if (child.TryGetComponent(out Rigidbody piecesRigidbody)) {
                    piecesRigidbody.AddExplosionForce(300f, transform.position, 10f, 1f);
                }
            }
        }

    }

}