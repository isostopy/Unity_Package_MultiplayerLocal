using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    private string textureFolder;

    [HideInInspector] public List<string> textureFiles = new List<string>();
    [HideInInspector] public ElementGroupsManager elementGroupsManager;

    [HideInInspector] public TextureDownloader textureDownloader;
    [HideInInspector] public TextureSelector textureSelector;
    [SerializeField] ElementGroupSelector groupSelector;

    private void Start()
    {
        if (textureSelector != null) textureSelector.textureManager = this;

        textureDownloader.texturesUpdated.AddListener(LoadTextures);

        textureFolder = Path.Combine(Application.persistentDataPath, "Textures");
        LoadTextures();

        ConnectionManager.Instance.SubscribeToMessages(ProcessMessage);
    }

    public void ApplySelection()
    {
        textureSelector.selectedTexture = Path.GetFileName(textureSelector.texturesDropdown.options[textureSelector.texturesDropdown.value].text);
        groupSelector.selectedGroup = elementGroupsManager.selectableGroups[groupSelector.groupsDropdown.value].groupID;
        OnScreenLog.TryLog("Texture " + textureSelector.selectedTexture + " selected for group: " + groupSelector.selectedGroup);

        ConnectionManager.Instance.SendMessageToAllClients(NetworkConstants.MsgChangeMaterials + "|" + groupSelector.selectedGroup + "|" + textureSelector.selectedTexture);
    }

    void ProcessMessage(string message, UdpReceiveResult result)
    {
        string[] parts = message.Split('|');

        if (parts.Length >= 3 && parts[0] == NetworkConstants.MsgChangeMaterials)
        {
            string groupID = parts[1];
            string textureName = parts[2];
            ChangeTextureToGroup(groupID, textureName);
            return;
        }

        if (parts.Length >= 1 && parts[0] == NetworkConstants.MsgUpdateTextures)
        {
            textureDownloader.UpdateTextures();
            return;
        }
    }

    public void LoadTextures()
    {
        if (!Directory.Exists(textureFolder))
        {
            OnScreenLog.TryLog("Texture folder not found!");
            return;
        }

        textureFiles.Clear();
        textureFiles.AddRange(Directory.GetFiles(textureFolder, "*.png"));
        textureFiles.AddRange(Directory.GetFiles(textureFolder, "*.jpg"));

        textureSelector?.UpdateDropdownsOptions();
    }

    public List<string> GetTexturesForGroup(string groupID)
    {
        string groupPath = Path.Combine(textureFolder, groupID);
        if (!Directory.Exists(groupPath))
            return new List<string>();

        List<string> textures = new();
        textures.AddRange(Directory.GetFiles(groupPath, "*.png"));
        textures.AddRange(Directory.GetFiles(groupPath, "*.jpg"));

        return textures;
    }

    public void ChangeTextureToGroup(string groupID, string textureName)
    {
        if (elementGroupsManager.selectableGroups.Length == 0) return;

        GameObject[] retrievedGroup = elementGroupsManager.groups[groupID];

        foreach (GameObject element in retrievedGroup)
        {
            Renderer elementRenderer = element.GetComponent<Renderer>();
            string path = Path.Combine(textureFolder, groupID, textureName);
            StartCoroutine(SetTexture(path, elementRenderer));
        }
    }

    IEnumerator SetTexture(string path, Renderer targetRenderer)
    {
        byte[] textureBytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(textureBytes);
        targetRenderer.material.mainTexture = texture;
        yield return null;
    }

    private void OnDestroy()
    {
        ConnectionManager.Instance?.UnsubscribeFromMessages(ProcessMessage);
    }

}
