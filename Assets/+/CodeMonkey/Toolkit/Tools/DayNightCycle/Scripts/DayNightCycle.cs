using UnityEngine;

namespace CodeMonkey.Toolkit.TDayNightCycle {

    /// <summary>
    /// ** Day Night Cycle **
    /// 
    /// Simple Day Night Cycle keeping track of time (24 hours) and rotating the sun in the sky
    /// There are IsDayTime() and IsNightTime() functions in case you want to do logic at Day/Night
    /// 
    /// Just drag the Prefab onto a scene, 
    /// and edit the sun animation if you want a different angle, 
    /// and edit the secondsPerDay in the DayNightCycle script.
    /// </summary>
    public class DayNightCycle : MonoBehaviour {


        public static DayNightCycle Instance { get; private set; }



        [SerializeField] private float secondsPerDay = 30f;



        private float dayTime;


        private void Awake() {
            Instance = this;
        }

        private void Update() {
            dayTime += Time.deltaTime / secondsPerDay;
            dayTime = dayTime % 1f;

            Debug.Log(GetDayTime24());
        }

        public float GetDayTime() {
            return dayTime;
        }

        public float GetDayTime24() {
            return dayTime * 24f;
        }

        public bool IsDayTime() {
            float dayTime24 = GetDayTime24();
            return dayTime >= 8 && dayTime <= 20f;
        }

        public bool IsNightTime() {
            return !IsDayTime();
        }

    }

}