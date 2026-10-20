using UnityEngine;

public class LogChat : MonoBehaviour
{
    [SerializeField] Log newLog;
    [SerializeField] GameObject content;



    public void GenerateNewLog(string msg)
    {
        GameObject nl = Instantiate(newLog.gameObject, content.transform);
        nl.GetComponent<Log>().AddText(msg);
    }


}
