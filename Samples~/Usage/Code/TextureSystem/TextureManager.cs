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

    private void Start()
    {
        textureDownloader.texturesUpdated.AddListener(LoadTextures);

        textureFolder = Path.Combine(Application.persistentDataPath, "Textures");
        LoadTextures();

        ConnectionManager.Instance.SubscribeToMessages(ProcessMessage);
    }

    public void SetSelectedTexture(string groupID, string textureName)
    {
        selectedGroup = groupID;
        selectedTexture = textureName;

        OnScreenLog.TryLog("Selected texture: " + textureName + " for group: " + groupID);
        OnTextureSelected.Invoke(groupID, textureName);

        ConnectionManager.Instance.SendMessageToAllClients(
            $"{NetworkConstants.MsgChangeMaterials}|{groupID}|{textureName}"
        );
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
        if (!Directory.Exists(textureFolder))
        {
            OnScreenLog.TryLog("Texture folder not found!");
            return;
        }

        textureFiles.Clear();
        textureFiles.AddRange(Directory.GetFiles(textureFolder, "*.png"));
        textureFiles.AddRange(Directory.GetFiles(textureFolder, "*.jpg"));
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
            Renderer renderer = element.GetComponent<Renderer>();
            string path = Path.Combine(textureFolder, groupID, textureName);
            StartCoroutine(SetTexture(path, renderer));
        }
    }

    IEnumerator SetTexture(string path, Renderer renderer)
    {
        byte[] textureBytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(textureBytes);
        renderer.material.mainTexture = texture;
        yield return null;
    }

    private void OnDestroy()
    {
        ConnectionManager.Instance?.UnsubscribeFromMessages(ProcessMessage);
    }
}
