using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextureGroupUIBinder : MonoBehaviour
{
    [System.Serializable]
    public class GroupDropdown
    {
        public string groupID;
        public TMP_Dropdown dropdown;
    }

    public TextureManager textureManager;
    public List<GroupDropdown> groupDropdowns;

    private Dictionary<string, Dictionary<string, string>> nameToFileMapByGroup = new();


    void Start()
    {
        if (textureManager?.textureDownloader != null)
        {
            textureManager.textureDownloader.texturesUpdated.AddListener(PopulateAllDropdowns);
        }
    }

    void PopulateAllDropdowns()
    {
        foreach (var entry in groupDropdowns)
        {
            PopulateDropdown(entry.groupID, entry.dropdown);
        }

        foreach (var entry in groupDropdowns)
        {
            string groupID = entry.groupID;
            TMP_Dropdown dropdown = entry.dropdown;

            dropdown.onValueChanged.AddListener((index) =>
            {
                if (dropdown.options.Count > 0)
                {
                    string displayName = dropdown.options[index].text;
                    if (nameToFileMapByGroup.TryGetValue(groupID, out var map) && map.TryGetValue(displayName, out var fileName))
                    {
                        ConnectionManager.Instance.SendMessageToAllClients(NetworkConstants.MsgChangeMaterials + "|" + groupID + "|" + fileName);
                        Debug.Log($"[{groupID}] → {displayName} ({fileName}) aplicado");
                    }
                }
            });
        }
    }

    void PopulateDropdown(string groupID, TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        nameToFileMapByGroup[groupID] = new Dictionary<string, string>();

        List<TextureDownloader.TextureData> textures = textureManager.GetTexturesForGroup(groupID);
        List<string> options = new();

        foreach (var tex in textures)
        {
            string displayName = tex.name;
            string internalName = tex.fileName;

            options.Add(displayName);
            nameToFileMapByGroup[groupID][displayName] = internalName;
        }

        dropdown.AddOptions(options);
    }


    private void OnDestroy()
    {
        if (textureManager?.textureDownloader != null)
        {
            textureManager.textureDownloader.texturesUpdated.RemoveListener(PopulateAllDropdowns);
        }
    }
}
