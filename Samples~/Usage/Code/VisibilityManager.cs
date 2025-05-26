using System.Net.Sockets;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    [HideInInspector] public ElementGroupsManager elementGroupsManager;
    [SerializeField] private ElementGroupSelector groupSelector;

    private void Start()
    {
        ConnectionManager.Instance.SubscribeToMessages(OnMessageReceived);
    }

    public void ToggleVisibility()
    {
        if (groupSelector.groupsDropdown.value >= elementGroupsManager.selectableGroups.Length)
            return;

        string groupID = elementGroupsManager.selectableGroups[groupSelector.groupsDropdown.value].groupID;
        groupSelector.selectedGroup = groupID;

        if (!elementGroupsManager.selectableGroups[groupSelector.groupsDropdown.value].hidable)
        {
            OnScreenLog.TryLog("Group is not hidable.");
            return;
        }

        OnScreenLog.TryLog("Toggle Group: " + groupID);
        ConnectionManager.Instance.SendMessageToAllClients(NetworkConstants.MsgToggleGroup + "|" + groupID);

    }

    private void OnMessageReceived(string message, UdpReceiveResult result)
    {
        string[] parts = message.Split('|');

        if (parts.Length == 2 && parts[0] == NetworkConstants.MsgToggleGroup)
        {
            ToggleGroup(parts[1]);
        }
    }

    private void ToggleGroup(string groupID)
    {
        if (!elementGroupsManager.groups.TryGetValue(groupID, out GameObject[] groupElements))
        {
            Debug.LogWarning($"Group '{groupID}' not found in dictionary.");
            return;
        }

        foreach (GameObject element in groupElements)
        {
            element.SetActive(!element.activeSelf);
        }
    }
}
