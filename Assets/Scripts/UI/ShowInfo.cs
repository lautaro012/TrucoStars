using UnityEngine;
using UnityEngine.UI;

public class ShowInfo : MonoBehaviour
{
    [SerializeField] Button InfoButton;
    [SerializeField] Button CloseButton;

    void Start()
    {
        InfoButton.onClick.AddListener(() =>
        {
            Show();
        });
        CloseButton.onClick.AddListener(() =>
        {
            Hide();
        });
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
