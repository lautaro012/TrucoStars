using UnityEngine;

namespace CodeMonkey.Toolkit.TDayNightCycle {

    public class DayNightCycleVisual : MonoBehaviour {


        private const string DAY_TIME = "DayTime";


        [SerializeField] private Animator dayNightCycleAnimator;


        private void Update() {
            dayNightCycleAnimator.SetFloat(DAY_TIME, DayNightCycle.Instance.GetDayTime());
        }

    }

}