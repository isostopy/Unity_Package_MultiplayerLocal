using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TextureDownloader : MonoBehaviour
{
    public string serverUrl;

    private string localPath;
    private List<TextureData> serverTextures;
    private List<string> localTextures;

    [HideInInspector] public UnityEvent texturesUpdated;

    void Start()
    {
        localPath = Path.Combine(Application.persistentDataPath, "Textures");
        if (!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            OnScreenLog.Instance.Log("No internet connection available.");
            return;
        }

        StartCoroutine(CheckForUpdates());

        ConnectionManager.Instance.messageReceived.AddListener(ProcessMessage);
    }

    private void ProcessMessage(string message, UdpReceiveResult result)
    {
        string[] parts = message.Split('|');

        if (parts.Length >= 1 && parts[0] == "UpdateTextures")
        {
            DownloadTextures();
            return;
        }

    }

    public void UpdateTextures()
    {
        StartCoroutine(CheckForUpdates());
    }

    IEnumerator CheckForUpdates()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                OnScreenLog.Instance.Log("Error fetching texture list: " + request.error);
                yield break;
            }

            TextureList textureList = JsonUtility.FromJson<TextureList>(request.downloadHandler.text);
            serverTextures = textureList.images;
            localTextures = new List<string>(Directory.GetFiles(localPath));

            if (NeedsUpdate())
            {
                OnScreenLog.Instance.Log("New textures found. Starting download...");
                StartCoroutine(DownloadAllTextures());
            }
            else
            {
                OnScreenLog.Instance.Log("All textures are up to date.");
                texturesUpdated.Invoke();
            }
        }
    }

    bool NeedsUpdate()
    {
        foreach (var texture in serverTextures)
        {
            string localFile = Path.Combine(localPath, texture.name);
            if (!localTextures.Contains(localFile))
            {
                return true;
            }
        }
        return false;
    }

    public void DownloadTextures()
    {
        StartCoroutine(DownloadAllTextures());
        ConnectionManager.Instance.SendMessageToEndpoint("UpdateTextures", ConnectionManager.Instance.broadcastEndpoint);
    }

    IEnumerator DownloadAllTextures()
    {
        foreach (var texture in serverTextures)
        {
            string filePath = Path.Combine(localPath, texture.name);

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(texture.url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    OnScreenLog.Instance.Log("Failed to download " + texture.name + ": " + request.error);
                    continue;
                }

                Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                File.WriteAllBytes(filePath, tex.EncodeToPNG());
                OnScreenLog.Instance.Log("Downloaded: " + texture.name);
            }
        }

        texturesUpdated.Invoke();

        OnScreenLog.Instance.Log("Textures updated!");
    }

    public void DeleteTextures()
    {
        if (Directory.Exists(localPath))
        {
            string[] files = Directory.GetFiles(localPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            texturesUpdated.Invoke();

            OnScreenLog.Instance.Log("All textures deleted.");
        }
        else
        {
            OnScreenLog.Instance.Log("No textures found to delete.");
        }

    }

    [System.Serializable]
    public class TextureData
    {
        public string name;
        public string url;
    }

    [System.Serializable]
    private class TextureList
    {
        public bool success;
        public List<TextureData> images;
    }
}