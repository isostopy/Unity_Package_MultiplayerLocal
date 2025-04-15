using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class TextureSelector : MonoBehaviour
{

    [SerializeField] TMP_Dropdown texturesDropdown;
    [SerializeField] TMP_Dropdown groupsDropdown;

    string selectedTexture;
    string selectedGroup;

    [HideInInspector] public TextureManager textureManager;

    public void UpdateDropdownsOptions()
    {
        texturesDropdown.ClearOptions();
        groupsDropdown.ClearOptions();


        List<string> textureOptions = new List<string>();
        foreach (var file in textureManager.textureFiles)
        {
            textureOptions.Add(Path.GetFileName(file));
        }
        texturesDropdown.AddOptions(textureOptions);


        List<string> groupOptions = new List<string>();
        foreach (var group in textureManager.selectableGroups)
        {
            groupOptions.Add(group.groupID);
        }
        groupsDropdown.AddOptions(groupOptions);
    }

    public void ApplySelection()
    {
        selectedTexture = Path.GetFileName(textureManager.textureFiles[texturesDropdown.value]);
        selectedGroup = textureManager.selectableGroups[groupsDropdown.value].groupID;
        OnScreenLog.Instance.Log("Texture " + selectedTexture + " selected for group: " + selectedGroup);

        ConnectionManager.Instance.SendMessageToEndpoint("ChangeMaterials|" + selectedGroup + "|" + selectedTexture, ConnectionManager.Instance.broadcastEndpoint);
    }

}
