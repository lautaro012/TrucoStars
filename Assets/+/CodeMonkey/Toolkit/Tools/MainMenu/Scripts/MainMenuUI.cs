using CodeMonkey.Toolkit.TBlockerUI;
using CodeMonkey.Toolkit.TQuestionDialog;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace CodeMonkey.Toolkit.TMainMenu {

    /// <summary>
    /// ** Main Menu **
    /// 
    /// Simple main menu for you to quickly add to your games/prototypes
    /// Just delete the text object, replace the logo, and edit the script to load your GameScene
    /// </summary>
    public class MainMenuUI : MonoBehaviour {


        [SerializeField] private Button playButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private MainMenuUI_PlayContainer playContainer;
        [SerializeField] private MainMenuUI_HowToPlayContainer howToPlayContainer;


        private void Awake() {
            playButton.onClick.AddListener(() => {
                playContainer.Show();
            });

            howToPlayButton.onClick.AddListener(() => {
                howToPlayContainer.Show();
            });

            quitButton.onClick.AddListener(() => {
                BlockerUI.Show();
                QuestionDialogUI.ShowQuestionYesNo("Are you sure you want to quit the game?", () => {
                    BlockerUI.Hide();
                }, () => {
                    Application.Quit();
#if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
#endif
                });
            });
        }


    }

}