using UnityEngine;
using static TextureManager;

public class ElementGroups : MonoBehaviour
{
    public SelectableGroup[] selectableGroups;

    private void Awake()
    {
        FindFirstObjectByType<TextureManager>().UpdateGroups(selectableGroups);
    }
}
