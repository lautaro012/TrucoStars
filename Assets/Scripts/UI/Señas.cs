using UnityEngine;
using UnityEngine.InputSystem;

public class Señas : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame) Show();
        else Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
