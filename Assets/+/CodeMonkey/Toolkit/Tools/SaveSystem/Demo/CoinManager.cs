using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeMonkey.Toolkit.TSaveSystem.Demo {

    public class CoinManager : MonoBehaviour {


        public static CoinManager Instance { get; private set; }


        [SerializeField] private Transform coinPrefab;


        private void Awake() {
            Instance = this;
        }

        public void SpawnCoins() {
            for (int i = 0; i < 10; i++) {
                Vector2 randomPosition = new Vector2(UnityEngine.Random.Range(-7, +7), UnityEngine.Random.Range(-4, +2));
                Instantiate(coinPrefab, randomPosition, Quaternion.identity);
            }
        }




        [Serializable]
        public class SaveObject {
            public Coin.SaveObject[] coinSaveObjectArray;
        }

        public SaveObject Save() {
            List<Coin.SaveObject> coinSaveObjectList = new List<Coin.SaveObject>();
            Coin[] coinArray = FindObjectsByType<Coin>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Coin coin in coinArray) {
                coinSaveObjectList.Add(coin.Save());
            }

            return new SaveObject {
                coinSaveObjectArray = coinSaveObjectList.ToArray(),
            };
        }

        public void Load(SaveObject saveObject) {
            foreach (Coin.SaveObject coinSaveObject in saveObject.coinSaveObjectArray) {
                Transform coinTransform = Instantiate(coinPrefab);
                coinTransform.GetComponent<Coin>().Load(coinSaveObject);
            }
        }



    }

}