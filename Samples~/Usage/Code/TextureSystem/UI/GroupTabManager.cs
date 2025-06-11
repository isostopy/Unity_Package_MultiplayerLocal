using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupTabManager : MonoBehaviour
{
    [System.Serializable]
    public class GroupTab
    {
        public string groupID;
        public Button button;
        public GameObject panel;
    }

    [Header("Configuración de tabs")]
    public List<GroupTab> groupTabs;

    public string CurrentGroupID { get; private set; }

    public delegate void GroupChanged(string newGroupID);
    public event GroupChanged OnGroupChanged;

    void Start()
    {
        foreach (var tab in groupTabs)
        {
            string capturedID = tab.groupID;
            tab.button.onClick.AddListener(() => ShowGroup(capturedID));
        }

        if (groupTabs.Count > 0)
        {
            ShowGroup(groupTabs[0].groupID); // Grupo inicial por defecto
        }
    }

    public void ShowGroup(string groupID)
    {
        CurrentGroupID = groupID;

        foreach (var tab in groupTabs)
        {
            bool isActive = tab.groupID == groupID;
            tab.panel.SetActive(isActive);
        }

        OnGroupChanged?.Invoke(groupID); // Notifica a quien escuche
        Debug.Log($"[GroupTabManager] Grupo activo cambiado a: {groupID}");
    }

    private void OnDestroy()
    {
        foreach (var tab in groupTabs)
        {
            tab.button.onClick.RemoveAllListeners();
        }
    }
}
