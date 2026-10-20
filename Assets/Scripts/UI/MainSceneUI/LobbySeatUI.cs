using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LobbySeatUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button seatButton;
    
    [SerializeField] private TextMeshProUGUI nameText;
    [Header("Colores de Estado")]
    [SerializeField] private Color emptyColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color occupiedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    public int TeamIndex { get; private set; }
    public int SeatIndex { get; private set; }
    public event Action<int, int> OnSeatClicked; 
    public void Setup(int teamIndex, int seatIndex)
    {
        TeamIndex = teamIndex;
        SeatIndex = seatIndex;
        
        SetEmpty(); // Arranca vacío por defecto

        seatButton.onClick.RemoveAllListeners();
        seatButton.onClick.AddListener(() => 
        {
            OnSeatClicked?.Invoke(TeamIndex, SeatIndex);
            Debug.Log("Click!");
        });
    }

    public void SetEmpty()
    {
        nameText.text = "+";
        backgroundImage.color = emptyColor;
        seatButton.interactable = true; // Se puede clickear para ocuparlo
    }

    public void SetOccupied(string playerName, bool isMyPlayer)
    {
        nameText.text = playerName;
        
        // Podés usar un color distinto si sos vos, o solo oscurecerlo
        backgroundImage.color = occupiedColor; 
        
        // Bloqueamos el botón para que otros no te clickeen encima
        // (Opcional: Si querés dejar que te echen, lo dejás en true)
        seatButton.interactable = false; 
    }
}