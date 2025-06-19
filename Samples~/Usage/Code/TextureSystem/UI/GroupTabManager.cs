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
        public TMP_Text labelText;
    }

    [Header("Configuración de tabs")]
    public List<GroupTab> groupTabs;

    public string CurrentGroupID { get; private set; }

    public delegate void GroupChanged(string newGroupID);
    public event GroupChanged OnGroupChanged;

    private Button selectedButton;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.black;
    [SerializeField] private Color defaultTextColor = Color.black;
    [SerializeField] private Color selectedTextColor = Color.white;

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

            ColorBlock colors = tab.button.colors;
            colors.normalColor = isActive ? selectedColor : defaultColor;
            colors.highlightedColor = isActive ? selectedColor : defaultColor;
            colors.selectedColor = isActive ? selectedColor : defaultColor;
            tab.button.colors = colors;
            tab.labelText = isActive ? selectedTextColor : defaulTexttColor;

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
