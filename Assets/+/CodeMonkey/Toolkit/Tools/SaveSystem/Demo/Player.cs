using System;
using UnityEngine;

namespace CodeMonkey.Toolkit.TSaveSystem.Demo {

    public class Player : MonoBehaviour {


        public static Player Instance { get; private set; }


        public event EventHandler OnCoinPickedUp;


        private void Awake() {
            Instance = this;
        }



        private void OnTriggerEnter2D(Collider2D collider2D) {
            if (collider2D.TryGetComponent(out Coin coin)) {
                coin.DestroySelf();
                OnCoinPickedUp?.Invoke(this, EventArgs.Empty);
            }
        }




        [Serializable]
        public class SaveObject {
            public Vector3 position;
        }

        public SaveObject Save() {
            return new SaveObject {
                position = transform.position,
            };
        }

        public void Load(SaveObject saveObject) {
            transform.position = saveObject.position;
        }


    }

}