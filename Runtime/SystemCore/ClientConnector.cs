using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientConnector : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button connectButton;

    private void Start()
    {
        connectButton.onClick.AddListener(ConnectToServer);
    }

    private void ConnectToServer()
    {
        string ip = ipInputField.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogWarning("IP field is empty.");
            return;
        }

        ConnectionManager.Instance.SetServerEndpoint(ip);
        ConnectionManager.Instance.SendMessageToServer(NetworkConstants.MsgClientHello);
        Debug.Log($"Sent ClientHello to {ip}");
    }

    private void OnDestroy()
    {
        connectButton.onClick.RemoveListener(ConnectToServer);
    }
}
