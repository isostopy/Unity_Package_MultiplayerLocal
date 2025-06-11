using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class TextureSelector : MonoBehaviour
{
    [HideInInspector] public string selectedTexture;
    [HideInInspector] public TextureManager textureManager;

    public TMP_Dropdown texturesDropdown;

    public void UpdateDropdownsOptions()
    {
        texturesDropdown.ClearOptions();

        if (textureManager == null || textureManager.elementGroupsManager == null)
            return;

        var groupSelector = textureManager.elementGroupsManager.elementGroupSelector;
        if (groupSelector == null || groupSelector.groupsDropdown.options.Count == 0)
            return;

        string selectedGroup = textureManager.elementGroupsManager.selectableGroups[groupSelector.groupsDropdown.value].groupID;
        List<string> groupTextures = textureManager.GetTexturesForGroup(selectedGroup);

        List<string> textureOptions = new();
        foreach (var file in groupTextures)
        {
            textureOptions.Add(Path.GetFileName(file));
        }

        texturesDropdown.AddOptions(textureOptions);
    }
}
