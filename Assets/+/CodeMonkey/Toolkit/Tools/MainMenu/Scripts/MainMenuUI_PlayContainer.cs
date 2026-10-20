using CodeMonkey.Toolkit.TBlockerUI;
using CodeMonkey.Toolkit.TQuestionDialog;
using CodeMonkey.Toolkit.TSceneLoadingSystem;
using UnityEngine;
using UnityEngine.UI;

namespace CodeMonkey.Toolkit.TMainMenu {

    public class MainMenuUI_PlayContainer : MonoBehaviour {



        [SerializeField] private Vector2 startAnchoredPosition;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button backButton;


        private void Awake() {
            continueButton.onClick.AddListener(() => {
                // Continue logic...
                BlockerUI.Show();
                QuestionDialogUI.ShowQuestionOk("Write the Continue logic...", BlockerUI.Hide);
            });

            newGameButton.onClick.AddListener(() => {
                // New game logic...
                BlockerUI.Show();
                QuestionDialogUI.ShowQuestionOk("Write the New Game logic...", BlockerUI.Hide);
                // Maybe: SceneLoader.Load(SceneLoader.Scene.Demo_GameScene);
            });

            loadGameButton.onClick.AddListener(() => {
                // Load logic...
                BlockerUI.Show();
                QuestionDialogUI.ShowQuestionOk("Write the Load Game logic...", BlockerUI.Hide);
            });

            backButton.onClick.AddListener(() => {
                Hide();
            });

            GetComponent<RectTransform>().anchoredPosition = startAnchoredPosition;

            Hide();
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

    }

}