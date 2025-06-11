using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform contentContainer;

    private Dictionary<string, string> ipToUserName = new();
    private List<Button> currentButtons = new();

    private void Start()
    {
        ConnectionManager.Instance.SubscribeToClientsUpdated(RefreshClientList);
        RefreshClientList();
    }

    private void RefreshClientList()
    {
        ClearButtons();

        List<string> clientIPs = ConnectionManager.Instance.GetClientIPList();
        int counter = 1;

        foreach (string ip in clientIPs)
        {
            string userName = $"Usuario {counter++}";
            ipToUserName[ip] = userName;

            GameObject buttonObj = Instantiate(buttonPrefab, contentContainer);
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            buttonText.text = userName;

            Button button = buttonObj.GetComponent<Button>();
            string capturedIP = ip;
            button.onClick.AddListener(() => OnClientButtonClicked(capturedIP));

            currentButtons.Add(button);
        }
    }

    private void ClearButtons()
    {
        foreach (Button btn in currentButtons)
        {
            Destroy(btn.gameObject);
        }
        currentButtons.Clear();
    }


    private void OnClientButtonClicked(string ip)
    {
        Debug.Log($"Selected client IP: {ip}");

        ConnectionManager.Instance.SelectClientByIP(ip);
    }

    private void OnDestroy()
    {
        
    }

}
