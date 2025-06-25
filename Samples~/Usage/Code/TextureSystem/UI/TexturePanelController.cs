using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TexturePanelController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GroupTabManager groupTabManager;
    [SerializeField] private TextureManager textureManager;
    [SerializeField] private GameObject textureButtonPrefab;
    [SerializeField] private Transform contentContainer;

    private readonly List<GameObject> activeButtons = new();

    void Start()
    {
        groupTabManager.OnGroupChanged += LoadTexturesForGroup;
        textureManager.textureDownloader.texturesUpdated.AddListener(OnTexturesUpdated);
        textureManager.OnTextureSelected.AddListener(OnTextureSelected);
    }

    private void LoadTexturesForGroup(string groupID)
    {

        if (string.IsNullOrEmpty(groupID))
        {
            Debug.LogWarning("[TexturePanelController] GroupID is null or empty.");
            return;
        }

        ClearButtons();

        List<TextureDownloader.TextureData> textures = textureManager.GetTexturesForGroup(groupID);

        foreach (var tex in textures)
        {
            GameObject btnObj = Instantiate(textureButtonPrefab, contentContainer);
            TextureButton btn = btnObj.GetComponent<TextureButton>();

            string path = Path.Combine(Application.persistentDataPath, "Textures", groupID, tex.fileName);
            btn.Setup(textureManager, groupID, tex.fileName, LoadSprite(path), tex.name); // ? pasas name

            activeButtons.Add(btnObj);
        }

    }

    private void OnTexturesUpdated()
    {
        LoadTexturesForGroup(groupTabManager.CurrentGroupID);
    }

    private void ClearButtons()
    {
        foreach (GameObject btn in activeButtons)
        {
            Destroy(btn);
        }
        activeButtons.Clear();
    }

    private Sprite LoadSprite(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    private void OnTextureSelected(string selectedGroup, string selectedTexture)
    {
        foreach (GameObject btnObj in activeButtons)
        {
            TextureButton btn = btnObj.GetComponent<TextureButton>();

            bool isSelected =
                btn.groupID == selectedGroup &&
                btn.textureName == selectedTexture;

            btn.SetSelected(isSelected);
        }
    }

    private void OnDestroy()
    {
        groupTabManager.OnGroupChanged -= LoadTexturesForGroup;
    }
}
