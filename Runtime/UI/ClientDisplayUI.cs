using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    private Dictionary<string, Button> ipToButton = new();
    private Dictionary<string, string> ipToUserName = new();
    private List<Button> currentButtons = new();
    private string selectedIP;

    private void Start()
    {
        ConnectionManager.Instance.SubscribeToClientsUpdated(RefreshClientList);
        RefreshClientList();
    }

    private void RefreshClientList()
    {
        ClearButtons();

        var clientIPs = ConnectionManager.Instance.GetClientIPList();
        int counter = 1;

        foreach (var ip in clientIPs)
        {
            string userName = $"Usuario {counter++}";
            ipToUserName[ip] = userName;

            GameObject buttonObj = Instantiate(buttonPrefab, contentContainer);
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            buttonText.text = userName;

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => ShowClient(ip));
            currentButtons.Add(button);
            ipToButton[ip] = button;
        }

        if (!string.IsNullOrEmpty(selectedIP) && ipToButton.ContainsKey(selectedIP))
            ShowClient(selectedIP);
    }

    public void ShowClient(string ip)
    {
        selectedIP = ip;

        foreach (var kvp in ipToButton)
        {
            bool isSelected = kvp.Key == ip;
            Button button = kvp.Value;

            ColorBlock colors = button.colors;
            colors.normalColor = isSelected ? selectedColor : defaultColor;
            colors.highlightedColor = isSelected ? selectedColor : defaultColor;
            colors.selectedColor = isSelected ? selectedColor : defaultColor;
            button.colors = colors;
        }

        ConnectionManager.Instance.SelectClientByIP(ip);
        Debug.Log($"[ClientDisplayUI] Cliente seleccionado: {ip}");
    }

    private void ClearButtons()
    {
        foreach (Button btn in currentButtons)
        {
            Destroy(btn.gameObject);
        }

        currentButtons.Clear();
        ipToButton.Clear();
        ipToUserName.Clear();
    }

    private void OnDestroy()
    {
        // limpieza de listeners si lo deseas
    }
}
