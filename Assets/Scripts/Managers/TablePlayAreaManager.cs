using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OnSlotsLaidOutArgs : EventArgs
{
    public Dictionary<int, PlaySlotView> PlayAreaBySeatIndex;
}
public class TablePlayAreaManager : MonoBehaviour
{
    [SerializeField] Transform playSlotPrefab;
    [SerializeField] Transform tablePlayArea;
    [SerializeField] GameObject cardPrefab;

    public EventHandler<OnSlotsLaidOutArgs> OnSlotsLaidOut;
    public void CreatePlayArea(int totalPlayers, Vector3 center, float radius, float angleOffsetDeg = 0f)
    {
        Dictionary<int, PlaySlotView> PlayAreaBySeatIndex = new();
        float anglePerSeat = 360f / totalPlayers;
        float tableHigh = 1.3f;
        for (int i = 0; i < totalPlayers; i++)
        {
            float angleDeg = angleOffsetDeg + i * anglePerSeat;
            float rad = angleDeg * Mathf.Deg2Rad;

            //* Posición en el plano XZ con altura fija
            Vector3 pos = new Vector3(
                center.x + Mathf.Cos(rad) * radius,
                tableHigh,
                center.z + Mathf.Sin(rad) * radius
            );

            //* Rotación mirando al centro (a la misma altura para evitar tilt)
            Vector3 lookAt = new(center.x, tableHigh, center.z);
            Vector3 dir = (lookAt - pos);
            dir.y = 0f;
            Quaternion rot = dir.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(dir, Vector3.up)
                : Quaternion.identity;

            //* Instanciación (seatPrefab es Transform → devuelve Transform)
            Transform seatTf = Instantiate(playSlotPrefab, pos, rot);
            //*Seteo como padre a la mesa
            seatTf.SetParent(tablePlayArea, true);
            //* Seats en el root del prefab (ajustá si está en un hijo)
            var playSlotView = seatTf.GetComponent<PlaySlotView>();
            PlayAreaBySeatIndex[i] = playSlotView;

        }
        OnSlotsLaidOut?.Invoke(this, new OnSlotsLaidOutArgs
        {
            PlayAreaBySeatIndex = PlayAreaBySeatIndex,
        });
    }
    public IEnumerator DealCardsAnimation(PlaySlotView dealerSlot, Transform[] playerAnchors, Action onDealFinished)
    {
        int totalPlayers = playerAnchors.Length;
        int totalCardsToDeal = totalPlayers * 3;
        
        Transform deckOrigin = dealerSlot.GetDeckPosition(); 
        
        float flightTime = 0.35f; // Tiempo de vuelo recto
        float staggerTime = 0.12f; // Pausa entre carta y carta

        for (int i = 0; i < totalCardsToDeal; i++)
        {
            int playerTargetIndex = i % totalPlayers; 
            Transform targetAnchor = playerAnchors[playerTargetIndex];

            // Disparamos el vuelo individual de esta carta
            StartCoroutine(FlyAndSpinCard(deckOrigin, targetAnchor, flightTime));

            yield return new WaitForSeconds(staggerTime);
        }

        // Esperamos a que la última carta termine de llegar
        yield return new WaitForSeconds(flightTime);

        // Disparamos el callback para prender las cartas reales de la mano
        onDealFinished?.Invoke();
    }

    // Mini-corrutina que maneja el vuelo recto y el giro de una sola carta
    private IEnumerator FlyAndSpinCard(Transform origin, Transform target, float duration)
    {
        // Instanciamos un "dorso" falso temporal
        GameObject dummyCard = Instantiate(cardPrefab, origin.position, origin.rotation);
        dummyCard.transform.rotation = Quaternion.Euler(-90f, origin.eulerAngles.y, 0f);
        float timeElapsed = 0f;
        float spinSpeed = 1200f; // Velocidad del giro ninja (grados por segundo)

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            
            // 1. Movimiento RECTO
            dummyCard.transform.position = Vector3.Lerp(origin.position, target.position, t);
            
            // 2. Giro HORIZONTAL constante
            dummyCard.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Destruimos la carta falsa ni bien llega al destino
        Destroy(dummyCard);
    }
}
