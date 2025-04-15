using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;


public class TextureManager : MonoBehaviour
{
    private string textureFolder;

    [HideInInspector] public List<string> textureFiles = new List<string>();
    [HideInInspector] public SelectableGroup[] selectableGroups;

    Dictionary<string, GameObject[]> groups = new Dictionary<string, GameObject[]>();

    [SerializeField] TextureDownloader textureDownloader;
    [SerializeField] TextureSelector textureSelector;

    private void Start()
    {

        if(textureSelector != null) textureSelector.textureManager = this;

        textureDownloader.texturesUpdated.AddListener(LoadTextures);

        textureFolder = Path.Combine(Application.persistentDataPath, "Textures");   

        LoadTextures();

        ConnectionManager.Instance.messageReceived.AddListener(ProcessMessage);
    }

    public void UpdateGroups(SelectableGroup[] selectableGroups)
    {
        this.selectableGroups = selectableGroups;

        groups.Clear();
        foreach (SelectableGroup group in selectableGroups)
        {
            groups.Add(group.groupID, group.elements);
        }

        if (textureSelector != null) textureSelector.UpdateDropdownsOptions();
    }

    void ProcessMessage(string message, UdpReceiveResult result)
    {
        string[] parts = message.Split('|');

        if (parts.Length >= 3 && parts[0] == "ChangeMaterials") // Example: "ChangeMaterials|GroupID|MaterialID"
        {
            string groupID = parts[1];
            string textureName = parts[2];
            ChangeTextureToGroup(groupID, textureName);
            return;
        }

        if (parts.Length >= 3 && parts[0] == "UpdateTextures")
        {
            textureDownloader.UpdateTextures();
            return;
        }

    }

    public void LoadTextures()
    {
        if (!Directory.Exists(textureFolder))
        {
            OnScreenLog.Instance.Log("Texture folder not found!");
            return;
        }

        textureFiles.Clear();
        textureFiles.AddRange(Directory.GetFiles(textureFolder, "*.png"));
        textureFiles.AddRange(Directory.GetFiles(textureFolder, "*.jpg"));

        if(textureSelector != null) textureSelector.UpdateDropdownsOptions();
    }

    public void ChangeTextureToGroup(string groupID, string textureName)
    {
        if (textureFiles.Count == 0 || selectableGroups.Length == 0) return;

        GameObject[] retrievedGroup = groups[groupID];

        foreach (GameObject element in retrievedGroup)
        {
            Renderer elementRenderer = element.GetComponent<Renderer>();
            string path = Path.Combine(textureFolder, textureName);

            OnScreenLog.Instance.Log("Start Coroutine Set texture");
            StartCoroutine(SetTexture(path, elementRenderer));
        }
    }

    IEnumerator SetTexture(string path, Renderer targetRenderer)
    {
        OnScreenLog.Instance.Log("texture to load path: " + path);

        byte[] textureBytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(textureBytes);
        targetRenderer.material.mainTexture = texture;
        yield return null;
    }

    
    #region Aditional Classes

    [System.Serializable]
    public class SelectableGroup
    {
        public string groupID;
        public GameObject[] elements;
    }

    #endregion

}