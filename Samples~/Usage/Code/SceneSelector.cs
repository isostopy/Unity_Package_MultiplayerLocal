using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelector : MonoBehaviour
{
    [SerializeField] TMP_Dropdown scenesDropdown;

    private void Start()
    {
        if (scenesDropdown != null)
        {
            scenesDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            UpdateDropdown();
        }

        ConnectionManager.Instance.messageReceived.AddListener(ProcessMessage);
    }

    void ProcessMessage(string message, UdpReceiveResult result)
    {
        string[] parts = message.Split('|');

        if (parts.Length >= 2 && parts[0] == "LoadScene") // Example: "LoadScene|LevelName"
        {
            string levelName = parts[1];
            SceneManager.LoadScene(levelName);
            return;
        }
    }

    public void UpdateDropdown()
    {
        scenesDropdown.ClearOptions();

        List<string> sceneOptions = new List<string>();
        foreach (var scene in ConnectionManager.Instance.selectableScenes)
        {
            sceneOptions.Add(scene);
        }

        scenesDropdown.AddOptions(sceneOptions);
    }

    void OnDropdownValueChanged(int selectedOptionIndex)
    {
        OnScreenLog.Instance.Log("Scene "+ ConnectionManager.Instance.selectableScenes[selectedOptionIndex] + " selected");
        ConnectionManager.Instance.SendMessageToEndpoint("LoadScene|" + ConnectionManager.Instance.selectableScenes[selectedOptionIndex], ConnectionManager.Instance.broadcastEndpoint);
    }

    private void OnDestroy()
    {
        scenesDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
}
