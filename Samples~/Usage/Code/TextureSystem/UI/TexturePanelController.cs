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

    private Button selectedTextureButton;
    private Color defaultColor = Color.white;
    private Color selectedColor = Color.grey;

    void Start()
    {
        groupTabManager.OnGroupChanged += LoadTexturesForGroup;

        if(textureManager == null ) textureManager = FindFirstObjectByType<TextureManager>();
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

            Button buttonComponent = btn.GetComponent<Button>();

            buttonComponent.onClick.AddListener(() =>
            {
                
                if (selectedTextureButton != null)
                {
                    var previousColors = selectedTextureButton.colors;
                    previousColors.normalColor = defaultColor;
                    selectedTextureButton.colors = previousColors;
                }

                var selectedColors = buttonComponent.colors;
                selectedColors.normalColor = selectedColor;
                buttonComponent.colors = selectedColors;

                selectedTextureButton = buttonComponent;
            });

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

    private void OnDestroy()
    {
        groupTabManager.OnGroupChanged -= LoadTexturesForGroup;
    }
}
