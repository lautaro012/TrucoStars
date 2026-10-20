using System;
using UnityEngine;

namespace CodeMonkey.Toolkit.TSaveSystem.Demo {

    public class Coin : MonoBehaviour {


        public void DestroySelf() {
            Destroy(gameObject);
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