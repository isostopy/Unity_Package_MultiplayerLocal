using TMPro;
using UnityEngine;

public class OnScreenLog : MonoBehaviour
{
    public static OnScreenLog Instance;

    [SerializeField] TextMeshProUGUI logText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    public static void TryLog(string message)
    {
        Instance?.Log(message);
    }

    public void Log(string message) // <-- Cambiado a public
    {
        if (logText != null)
        {
            string prevLog = logText.text;
            logText.text = message + "\n" + prevLog;
        }
        Debug.Log(message);
    }

    public void ClearLog()
    {
        logText.text = "";
    }
}
