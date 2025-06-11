using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElementGroupSelector : MonoBehaviour
{
    [HideInInspector] public string selectedGroup;
    [HideInInspector] public ElementGroupsManager elementGroupsManager;

    public TMP_Dropdown groupsDropdown;

    private void Start()
    {
        groupsDropdown.onValueChanged.AddListener(OnGroupChanged);
    }

    private void OnGroupChanged(int index)
    {
       // if (elementGroupsManager?.textureManager?.textureSelector != null)
        //{
         //   elementGroupsManager.textureManager.textureSelector.UpdateDropdownsOptions();
        //}
    }

    public void UpdateDropdownsOptions()
    {
        groupsDropdown.ClearOptions();

        List<string> groupOptions = new List<string>();
        foreach (var group in elementGroupsManager.selectableGroups)
        {
            groupOptions.Add(group.groupID);
        }

        groupsDropdown.AddOptions(groupOptions);
    }
}
