using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientSelector : MonoBehaviour
{
    [SerializeField] TMP_Dropdown clientsDropdown;

    private void Start()
    {
        ConnectionManager.Instance.clientsFound.AddListener(UpdateDropdown);
        clientsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void UpdateDropdown()
    {
        clientsDropdown.ClearOptions();

        List<string> clientOptions = new List<string>();

        foreach (var client in ConnectionManager.Instance.clients)
        {
            clientOptions.Add(client.Address.ToString());
        }

        clientsDropdown.AddOptions(clientOptions);
        if (clientOptions.Count == 1) ConnectionManager.Instance.SelectClient(ConnectionManager.Instance.clients[0]);
    }

    void OnDropdownValueChanged(int selectedOptionIndex)
    {
        if(ConnectionManager.Instance.clients.Count == 0 || ConnectionManager.Instance.clients[selectedOptionIndex] == null) return;
        OnScreenLog.Instance.Log("Client: " + ConnectionManager.Instance.clients[selectedOptionIndex] + " selected");
        ConnectionManager.Instance.SelectClient(ConnectionManager.Instance.clients[selectedOptionIndex]);
    }

    private void OnDestroy()
    {
        clientsDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
}
