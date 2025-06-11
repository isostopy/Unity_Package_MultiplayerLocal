using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class TextureDownloader : MonoBehaviour
{
    private string localPath;
    private List<TextureData> serverTextures;

    public TextureManager textureManager;

    [HideInInspector] public UnityEvent texturesUpdated;

    void Start()
    {
        localPath = Path.Combine(Application.persistentDataPath, "Textures");
        if (!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            OnScreenLog.TryLog("No internet connection available.");
            return;
        }

        ConnectionManager.Instance.SubscribeToMessages(ProcessMessage);

        ConnectionManager.Instance.SendMessageToAllClients(NetworkConstants.MsgUpdateTextures);
    }


    private void ProcessMessage(string message, UdpReceiveResult result)
    {
        string[] parts = message.Split('|');

        if (parts.Length >= 1 && parts[0] == NetworkConstants.MsgUpdateTextures)
        {
            UpdateTextures();
        }
    }

    public void UpdateTextures()
    {
        StartCoroutine(CheckForUpdates());
    }

    IEnumerator CheckForUpdates()
    {
        if (textureManager == null || textureManager.elementGroupsManager == null)
        {
            OnScreenLog.TryLog("Error: TextureManager o ElementGroupsManager no están asignados.");
            yield break;
        }

        serverTextures = new List<TextureData>();

        foreach (var group in textureManager.elementGroupsManager.selectableGroups)
        {
            using UnityWebRequest request = UnityWebRequest.Get(group.url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                OnScreenLog.TryLog($"Error fetching textures from {group.groupID}: {request.error}");
                StartCoroutine(RetryGroupFetch(group, 5f));
                continue;
            }

            TextureList textureList = JsonUtility.FromJson<TextureList>(request.downloadHandler.text);
            if (textureList?.images != null)
            {
                foreach (var texture in textureList.images)
                {
                    texture.groupID = group.groupID;
                    serverTextures.Add(texture);
                }
            }
        }

        if (NeedsUpdate())
        {
            OnScreenLog.TryLog("New textures found. Starting download...");
            StartCoroutine(DownloadAllTextures());
        }
        else
        {
            OnScreenLog.TryLog("All textures are up to date.");
            texturesUpdated.Invoke();
        }
    }

    private IEnumerator RetryGroupFetch(SelectableGroup group, float delay)
    {
        yield return new WaitForSeconds(delay);
        OnScreenLog.TryLog($"Retrying texture list fetch for group {group.groupID}...");

        using UnityWebRequest request = UnityWebRequest.Get(group.url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            TextureList textureList = JsonUtility.FromJson<TextureList>(request.downloadHandler.text);
            if (textureList?.images != null)
            {
                foreach (var texture in textureList.images)
                {
                    texture.groupID = group.groupID;
                    serverTextures.Add(texture);
                }

                OnScreenLog.TryLog($"Recovered texture list for group {group.groupID}. Retrying full update...");
                StartCoroutine(DownloadAllTextures());
            }
        }
        else
        {
            StartCoroutine(RetryGroupFetch(group, delay));
        }
    }

    bool NeedsUpdate()
    {
        foreach (var texture in serverTextures)
        {
            string groupPath = Path.Combine(localPath, texture.groupID);
            string localFile = Path.Combine(groupPath, texture.name);
            if (!File.Exists(localFile))
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator DownloadAllTextures()
    {
        foreach (var texture in serverTextures)
        {
            string groupPath = Path.Combine(localPath, texture.groupID);
            if (!Directory.Exists(groupPath))
                Directory.CreateDirectory(groupPath);

            string filePath = Path.Combine(groupPath, texture.name);

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(texture.url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    OnScreenLog.TryLog($"Failed to download {texture.name}: {request.error}");
                    StartCoroutine(RetryDownloadTexture(texture, 5f));
                    continue;
                }

                Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                File.WriteAllBytes(filePath, tex.EncodeToPNG());
                OnScreenLog.TryLog($"Downloaded: {texture.name} to group: {texture.groupID}");
            }
        }

        texturesUpdated.Invoke();
        OnScreenLog.TryLog("Textures updated!");
    }

    private IEnumerator RetryDownloadTexture(TextureData texture, float delay)
    {
        yield return new WaitForSeconds(delay);
        OnScreenLog.TryLog($"Retrying download for texture {texture.name}...");

        string groupPath = Path.Combine(localPath, texture.groupID);
        string filePath = Path.Combine(groupPath, texture.name);

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(texture.url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (!Directory.Exists(groupPath))
                    Directory.CreateDirectory(groupPath);

                Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                File.WriteAllBytes(filePath, tex.EncodeToPNG());
                OnScreenLog.TryLog($"Recovered: {texture.name}");
                texturesUpdated.Invoke();
            }
            else
            {
                StartCoroutine(RetryDownloadTexture(texture, delay));
            }
        }
    }

    public void DeleteTextures()
    {
        if (Directory.Exists(localPath))
        {
            Directory.Delete(localPath, true);
            Directory.CreateDirectory(localPath);

            texturesUpdated.Invoke();
            OnScreenLog.TryLog("All textures deleted.");
        }
        else
        {
            OnScreenLog.TryLog("No textures found to delete.");
        }
    }

    private void OnDestroy()
    {
        ConnectionManager.Instance?.UnsubscribeFromMessages(ProcessMessage);
    }


    [System.Serializable]
    public class TextureData
    {
        public string name;
        public string url;
        [System.NonSerialized] public string groupID;
    }

    [System.Serializable]
    private class TextureList
    {
        public bool success;
        public List<TextureData> images;
    }
}
