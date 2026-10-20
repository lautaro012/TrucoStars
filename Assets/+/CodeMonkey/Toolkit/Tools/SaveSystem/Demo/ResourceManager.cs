using System;
using UnityEngine;

namespace CodeMonkey.Toolkit.TSaveSystem.Demo {

    public class ResourceManager : MonoBehaviour {


        public static ResourceManager Instance { get; private set; }


        private int coinAmount;



        private void Awake() {
            Instance = this;
        }

        public void AddCoin() {
            coinAmount++;
        }

        public int GetCoinAmount() {
            return coinAmount;
        }


        [Serializable]
        public class SaveObject {
            public int coinAmount;
        }


        public SaveObject Save() {
            return new SaveObject {
                coinAmount = coinAmount,
            };
        }

        public void Load(SaveObject saveObject) {
            coinAmount = saveObject.coinAmount;
        }





    }

}