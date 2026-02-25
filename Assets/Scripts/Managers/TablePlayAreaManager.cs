using System;
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

}
