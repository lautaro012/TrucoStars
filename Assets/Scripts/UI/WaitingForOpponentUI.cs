using System;
using UnityEngine;

public class WaitingForOpponentUI : MonoBehaviour
{
    private void Start()
    {
        GameClientManager.Instance.HideLoadingScreen += GameManager_HideLoadingScreen;
        Show();
    }
    private void OnDestroy()
    {
        GameClientManager.Instance.HideLoadingScreen -= GameManager_HideLoadingScreen;
    }
    private void GameManager_HideLoadingScreen(object sender, EventArgs e)
    {
        Hide();
    }
    private void Show() {
    gameObject.SetActive(true);
   }
   private void Hide() {    
    gameObject.SetActive(false);
   }
}
