using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeMonkey.Toolkit.TDayNightCycle {

    public class DayNightCycleUI : MonoBehaviour {



        [SerializeField] private Image clockImage;
        [SerializeField] private TextMeshProUGUI timeTextMesh;


        private void Update() {
            clockImage.fillAmount = DayNightCycle.Instance.GetDayTime();
            int hours = Mathf.FloorToInt(DayNightCycle.Instance.GetDayTime24());
            int minutes = Mathf.RoundToInt((DayNightCycle.Instance.GetDayTime24() - hours) * 60f);
            timeTextMesh.text = hours.ToString("00") + ":" + minutes.ToString("00");
        }


    }

}