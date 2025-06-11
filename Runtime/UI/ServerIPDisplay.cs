using TMPro;
using UnityEngine;

public class ServerIPDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text ipText;

    private void Start()
    {
        if (ConnectionManager.Instance != null &&
            ConnectionManager.Instance.GetRole() == DeviceRol.Server)
        {
            string ip = ConnectionManager.Instance.GetLocalIPAddress();
            ipText.text = $"Server IP: {ip}";
        }
        else
        {
            ipText.text = "";
        }
    }
}
