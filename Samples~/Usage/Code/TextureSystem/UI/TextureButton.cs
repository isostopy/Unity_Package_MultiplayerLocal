using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TextureButton : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image textureImage;
    [SerializeField] private TMP_Text labelText; // Opcional

    private TextureManager textureManager;
    private string groupID;
    private string textureName;

    [SerializeField] private GameObject selectionFrame;


    public void Setup(TextureManager manager, string groupID, string textureName, Sprite textureSprite)
    {
        this.textureManager = manager;
        this.groupID = groupID;
        this.textureName = textureName;

        if (textureImage != null)
            textureImage.sprite = textureSprite;

        if (labelText != null)
            labelText.text = Path.GetFileNameWithoutExtension(textureName);

        if (selectionFrame != null)
            selectionFrame.SetActive(false);

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        textureManager?.SetSelectedTexture(groupID, textureName);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionFrame != null)
            selectionFrame.SetActive(isSelected);
    }


    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(OnClick);
    }
}
