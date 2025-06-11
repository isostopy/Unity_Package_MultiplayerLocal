using UnityEngine;
using UnityEngine.UI;

public class ClientPreviewDisplay : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RawImage previewImage;
    [SerializeField] private RenderTexture renderTexture;

    public void InitializePreview()
    {
        previewImage.texture = renderTexture;
        previewImage.enabled = true;
        Debug.Log("[ClientPreview] Vista vinculada a RenderTexture.");
    }

    public void ClearPreview()
    {
        previewImage.texture = null;
        previewImage.enabled = false;
    }
}
