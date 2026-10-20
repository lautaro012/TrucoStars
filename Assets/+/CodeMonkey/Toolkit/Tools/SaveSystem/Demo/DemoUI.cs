using CodeMonkey.Toolkit.TTextPopup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeMonkey.Toolkit.TSaveSystem.Demo {

    public class DemoUI : MonoBehaviour {



        [SerializeField] private GameObject backgroundGameObject;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private TextMeshProUGUI coinAmountTextMesh;



        private void Awake() {
            newGameButton.onClick.AddListener(() => {
                Demo.Instance.NewGame();
                newGameButton.gameObject.SetActive(false);
                continueButton.gameObject.SetActive(false);
                saveButton.gameObject.SetActive(true);
                backgroundGameObject.SetActive(false);
                TextPopupUI.Create(Vector2.zero, "New Game!");
            });

            continueButton.onClick.AddListener(() => {
                Demo.Instance.ContinueGame();
                newGameButton.gameObject.SetActive(false);
                continueButton.gameObject.SetActive(false);
                saveButton.gameObject.SetActive(true);
                backgroundGameObject.SetActive(false);
                TextPopupUI.Create(Vector2.zero, "Game Loaded!");
            });

            saveButton.onClick.AddListener(() => {
                Demo.Instance.SaveGame();
                TextPopupUI.Create(Vector2.zero, "Game Saved!");
            });

            saveButton.gameObject.SetActive(false);
        }

        private void Update() {
            coinAmountTextMesh.SetText("COINS: " + ResourceManager.Instance.GetCoinAmount().ToString());
        }

    }

}