using System;
using UnityEngine;

public class WaitingForOpponentUI : MonoBehaviour
{
    private void Start()
    {
        Show();
    }
    public void Show() {
        gameObject.SetActive(true);
    }
    public void Hide() {    
        gameObject.SetActive(false);
    }
}
