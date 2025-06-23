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
        textureManager.OnTextureSelected.AddListener(OnTextureSelected);

        LoadTexturesForGroup(groupTabManager.CurrentGroupID);
    }

    private void LoadTexturesForGroup(string groupID)
    {
        ClearButtons();

        List<string> texturePaths = textureManager.GetTexturesForGroup(groupID);

        foreach (string path in texturePaths)
        {
            GameObject btnObj = Instantiate(textureButtonPrefab, contentContainer);
            TextureButton btn = btnObj.GetComponent<TextureButton>();

            string fileName = Path.GetFileName(path);
            btn.Setup(textureManager, groupID, fileName, LoadSprite(path));

            activeButtons.Add(btnObj);
        }
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
