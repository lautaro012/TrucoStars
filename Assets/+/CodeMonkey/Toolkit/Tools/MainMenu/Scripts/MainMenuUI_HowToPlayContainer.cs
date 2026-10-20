using UnityEngine;
using UnityEngine.UI;

namespace CodeMonkey.Toolkit.TMainMenu {

    public class MainMenuUI_HowToPlayContainer : MonoBehaviour {



        [SerializeField] private Vector2 startAnchoredPosition;
        [SerializeField] private Button backButton;


        private void Awake() {
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