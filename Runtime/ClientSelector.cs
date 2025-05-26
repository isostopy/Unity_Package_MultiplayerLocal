using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown clientsDropdown;

    private void Start()
    {
        if (ConnectionManager.Instance != null)
        {
            ConnectionManager.Instance.SubscribeToClientsUpdated(UpdateDropdown);
        }

        clientsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        UpdateDropdown(); // Iniciar con lista vacía
    }

    private void UpdateDropdown()
    {
        clientsDropdown.ClearOptions();

        var clientIPs = ConnectionManager.Instance.GetClientIPList();
        clientsDropdown.AddOptions(clientIPs);

        if (clientIPs.Count == 1)
        {
            ConnectionManager.Instance.SelectClientByIndex(0);
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        ConnectionManager.Instance.SelectClientByIndex(index);
        Debug.Log($"Client at index {index} selected.");
    }

    private void OnDestroy()
    {
        clientsDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
}
