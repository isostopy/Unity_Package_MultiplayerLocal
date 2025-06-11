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
                    string textureName = dropdown.options[index].text;

                    ConnectionManager.Instance.SendMessageToAllClients(NetworkConstants.MsgChangeMaterials + "|" + groupID + "|" + textureName);
                    Debug.Log($"[{groupID}] → {textureName} aplicado");
                }
            });
        }
    }

    void PopulateDropdown(string groupID, TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();

        List<string> textures = textureManager.GetTexturesForGroup(groupID);
        List<string> options = new();
        foreach (var path in textures)
        {
            options.Add(System.IO.Path.GetFileName(path));
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
