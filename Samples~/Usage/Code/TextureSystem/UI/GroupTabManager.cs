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

    private Button selectedButton;
    private Color defaultColor = Color.white;
    private Color selectedColor = Color.grey;

    void Start()
    {
        foreach (var tab in groupTabs)
        {
            string capturedID = tab.groupID;
            tab.button.onClick.AddListener(() => ShowGroup(capturedID));
        }

    }

    public void ShowGroup(string groupID)
    {
        CurrentGroupID = groupID;

        foreach (var tab in groupTabs)
        {
            bool isActive = tab.groupID == groupID;
            tab.panel.SetActive(isActive);

            // Cambia color del botón
            ColorBlock colors = tab.button.colors;
            colors.normalColor = isActive ? selectedColor : defaultColor;
            tab.button.colors = colors;

            if (isActive)
                selectedButton = tab.button;
        }

        OnGroupChanged?.Invoke(groupID);
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
