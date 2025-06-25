using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TextureButton : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image textureImage;
    [SerializeField] private TMP_Text labelText;

    private TextureManager textureManager;
    public string groupID;
    public string textureName;

    [SerializeField] private GameObject selectionFrame;


    public void Setup(TextureManager manager, string groupID, string textureFileName, Sprite textureSprite, string displayName)
    {
        this.textureManager = manager;
        this.groupID = groupID;
        this.textureName = textureFileName;

        if (textureImage != null)
            textureImage.sprite = textureSprite;

        if (labelText != null)
            labelText.text = displayName;


        if (selectionFrame != null)
            selectionFrame.SetActive(false);

        labelText.fontStyle = FontStyles.Normal;

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

        if (isSelected)
        {
            labelText.fontStyle = FontStyles.Bold;
        }
        else
        {
            labelText.fontStyle = FontStyles.Normal;
        }
    }


    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(OnClick);
    }
}
