using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextureSelector : MonoBehaviour
{
    [HideInInspector] public string selectedTexture; // Este seguirá almacenando el fileName
    [HideInInspector] public TextureManager textureManager;

    public TMP_Dropdown texturesDropdown;

    private Dictionary<string, string> nameToFileMap = new(); // name ? fileName

    public void UpdateDropdownsOptions()
    {
        texturesDropdown.ClearOptions();
        nameToFileMap.Clear();

        if (textureManager == null || textureManager.elementGroupsManager == null)
            return;

        var groupSelector = textureManager.elementGroupsManager.elementGroupSelector;
        if (groupSelector == null || groupSelector.groupsDropdown.options.Count == 0)
            return;

        string selectedGroup = textureManager.elementGroupsManager
            .selectableGroups[groupSelector.groupsDropdown.value].groupID;

        List<TextureDownloader.TextureData> groupTextures = textureManager.GetTexturesForGroup(selectedGroup);

        List<string> textureOptions = new();
        foreach (var tex in groupTextures)
        {
            textureOptions.Add(tex.name); // Mostrar name en el dropdown
            nameToFileMap[tex.name] = tex.fileName;
        }

        texturesDropdown.AddOptions(textureOptions);

        // Opción: seleccionar automáticamente la primera
        if (textureOptions.Count > 0)
        {
            OnDropdownValueChanged(0);
            texturesDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        string selectedDisplayName = texturesDropdown.options[index].text;

        if (nameToFileMap.TryGetValue(selectedDisplayName, out var fileName))
        {
            selectedTexture = fileName;
            Debug.Log($"[TextureSelector] Seleccionado: {selectedDisplayName} ? {fileName}");
        }
    }
}
