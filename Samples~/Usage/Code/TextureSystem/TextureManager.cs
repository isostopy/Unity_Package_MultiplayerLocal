using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class TextureManager : MonoBehaviour
{
    private string textureFolder;
    private string selectedGroup;
    private string selectedTexture;

    [HideInInspector] public List<string> textureFiles = new();

    [Header("Gestores externos")]
    public ElementGroupsManager elementGroupsManager;
    public TextureDownloader textureDownloader;

    public UnityEvent<string, string> OnTextureSelected = new();

    private Dictionary<string, List<TextureDownloader.TextureData>> texturesByGroup = new();


    private void Start()
    {
        textureDownloader.texturesUpdated.AddListener(LoadTextures);

        textureFolder = Path.Combine(Application.persistentDataPath, "Textures");
        LoadTextures();

        ConnectionManager.Instance.SubscribeToMessages(ProcessMessage);
    }

    public void SetDownloadedTextures(List<TextureDownloader.TextureData> textures)
    {
        texturesByGroup.Clear();
        foreach (var tex in textures)
        {
            if (!texturesByGroup.ContainsKey(tex.groupID))
                texturesByGroup[tex.groupID] = new List<TextureDownloader.TextureData>();

            texturesByGroup[tex.groupID].Add(tex);
        }
    }


    public void SetSelectedTexture(string groupID, string textureName)
    {
        selectedGroup = groupID;
        selectedTexture = textureName;

        OnScreenLog.TryLog("Selected texture: " + textureName + " for group: " + groupID);
        OnTextureSelected.Invoke(groupID, textureName);

        ConnectionManager.Instance.SendMessageToAllClients($"{NetworkConstants.MsgChangeMaterials}|{groupID}|{textureName}");
    }

    public (string groupID, string textureName) GetSelected()
    {
        return (selectedGroup, selectedTexture);
    }

    void ProcessMessage(string message, System.Net.Sockets.UdpReceiveResult result)
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
        textureFiles.Clear();

        foreach (var group in elementGroupsManager.selectableGroups)
        {
            string groupPath = Path.Combine(textureFolder, group.groupID);
            if (Directory.Exists(groupPath))
            {
                textureFiles.AddRange(Directory.GetFiles(groupPath, "*.png"));
                textureFiles.AddRange(Directory.GetFiles(groupPath, "*.jpg"));
            }
        }
    }

    public List<TextureDownloader.TextureData> GetTexturesForGroup(string groupID)
    {
        if (texturesByGroup.ContainsKey(groupID))
            return texturesByGroup[groupID];

        return new List<TextureDownloader.TextureData>();
    }


    public void ChangeTextureToGroup(string groupID, string textureName)
    {
        if (elementGroupsManager.selectableGroups.Length == 0) return;

        GroupElement[] retrievedGroup = elementGroupsManager.groups[groupID];

        foreach (var item in retrievedGroup)
        {
            Renderer renderer = item.element.GetComponent<Renderer>();
            if (renderer == null) continue;

            string path = Path.Combine(textureFolder, groupID, textureName);
            StartCoroutine(SetTexture(path, renderer, item.materialIndex));
        }
    }


    IEnumerator SetTexture(string path, Renderer renderer, int materialIndex)
    {
        if (materialIndex < 0 || materialIndex >= renderer.materials.Length)
        {
            Debug.LogWarning($"Material index {materialIndex} out of range for {renderer.gameObject.name}");
            yield break;
        }

        byte[] textureBytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(textureBytes);

        Material[] materials = renderer.materials;
        materials[materialIndex].mainTexture = texture;
        renderer.materials = materials;

        yield return null;
    }

}
