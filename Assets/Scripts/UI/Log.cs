using TMPro;
using UnityEngine;

public class Log : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void AddText(string msg) => text.text = msg;
}
