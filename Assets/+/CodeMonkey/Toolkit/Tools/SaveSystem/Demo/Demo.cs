using System;
using UnityEngine;


namespace CodeMonkey.Toolkit.TSaveSystem.Demo {

    public class Demo : MonoBehaviour {


        public static readonly string SAVE_FILE_PATH = Application.dataPath + "/CodeMonkey/Toolkit/Tools/SaveSystem/Demo/Saves/";


        public static Demo Instance { get; private set; }



        private void Awake() {
            Instance = this;
        }

        private void Start() {
            Player.Instance.OnCoinPickedUp += Player_OnCoinPickedUp;
        }

        private void Player_OnCoinPickedUp(object sender, System.EventArgs e) {
            ResourceManager.Instance.AddCoin();
        }


        public void NewGame() {
            CoinManager.Instance.SpawnCoins();
        }

        public void ContinueGame() {
            DemoMainSaveObject demoMainSaveObject = SaveSystem.LoadMostRecent<DemoMainSaveObject>(SAVE_FILE_PATH);
            if (demoMainSaveObject != null) {
                Load(demoMainSaveObject);
            }
        }

        public void SaveGame() {
            DemoMainSaveObject demoMainSaveObject = Save();
            SaveSystem.Save("MySaveName", demoMainSaveObject, false, SAVE_FILE_PATH);
        }




        [Serializable]
        public class DemoMainSaveObject {
            public ResourceManager.SaveObject resourceManagerSaveObject;
            public CoinManager.SaveObject coinManagerSaveObject;
            public Player.SaveObject playerSaveObject;
        }

        public DemoMainSaveObject Save() {
            return new DemoMainSaveObject {
                resourceManagerSaveObject = ResourceManager.Instance.Save(),
                coinManagerSaveObject = CoinManager.Instance.Save(),
                playerSaveObject = Player.Instance.Save(),
            };
        }

        public void Load(DemoMainSaveObject demoMainSaveObject) {
            ResourceManager.Instance.Load(demoMainSaveObject.resourceManagerSaveObject);
            CoinManager.Instance.Load(demoMainSaveObject.coinManagerSaveObject);
            Player.Instance.Load(demoMainSaveObject.playerSaveObject);
        }

    }

}