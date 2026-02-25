using System;
using System.Collections.Generic;
using UnityEngine;
public class SeatCreatedEventArgs : EventArgs
{
    public int SeatIndex;
    public SeatController Seat;
    public Transform SeatTransform;
}

public class SeatsLaidOutEventArgs : EventArgs
{
    public Dictionary<int, SeatController> SeatsByIndex;
}
public class SeatLayoutManager : MonoBehaviour
{
    [SerializeField] GameObject seatPrefab;
    [SerializeField] Transform SeatRoot;
    public EventHandler<SeatCreatedEventArgs> OnSeatCreated;
    public EventHandler<SeatsLaidOutEventArgs> OnSeatsLaidOut;
    public void CreateSeats(int totalPlayers, Vector3 center, float radius, float heightY, float angleOffsetDeg = 0f)
    {
        Dictionary<int, SeatController> Seats_Index = new();

        float anglePerSeat = 360f / totalPlayers;

        for (int i = 0; i < totalPlayers; i++)
        {
            float angleDeg = angleOffsetDeg + i * anglePerSeat;
            float rad = angleDeg * Mathf.Deg2Rad;

            //* Posición en el plano XZ con altura fija
            Vector3 pos = new Vector3(
                center.x + Mathf.Cos(rad) * radius,
                heightY,
                center.z + Mathf.Sin(rad) * radius
            );

            //* Rotación mirando al centro (a la misma altura para evitar tilt)
            Vector3 lookAt = new Vector3(center.x, heightY, center.z);
            Vector3 dir = (lookAt - pos);
            dir.y = 0f;
            
            Quaternion rot = dir.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(dir, Vector3.up)
                : Quaternion.identity;

            //* Instanciación (seatPrefab es Transform → devuelve Transform)
            GameObject seatTf = Instantiate(seatPrefab, pos, rot);
            //*Seteo como padre a la mesa
            seatTf.transform.SetParent(SeatRoot, true);
            //* Seats en el root del prefab (ajustá si está en un hijo)
            var hv = seatTf.GetComponent<SeatController>();
            Seats_Index[i] = hv;

            OnSeatCreated?.Invoke(this, new SeatCreatedEventArgs
            {
                SeatIndex = i,
                Seat = hv,
                SeatTransform = seatTf.transform
            });
        }
        GameManager.Instance.LayoutReadyServerRpc();
    }

}
