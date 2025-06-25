using System.Collections.Generic;
using UnityEngine;

public class ElementGroupsManager : MonoBehaviour
{
    public SelectableGroup[] selectableGroups;
    public Dictionary<string, GroupElement[]> groups = new();


    [HideInInspector] public TextureManager textureManager;
    [HideInInspector] public ElementGroupSelector elementGroupSelector;

    private void Awake()
    {
        UpdateGroups();
    }

    public void UpdateGroups()
    {
        groups.Clear();

        foreach (SelectableGroup group in selectableGroups)
        {
            if (!groups.ContainsKey(group.groupID))
                groups.Add(group.groupID, group.elements);
            else
                Debug.LogWarning($"Duplicate group ID detected: {group.groupID}. Skipping.");
        }

        textureManager = FindFirstObjectByType<TextureManager>();
        elementGroupSelector = FindFirstObjectByType<ElementGroupSelector>();

        if (textureManager != null)
            textureManager.elementGroupsManager = this;

        if (elementGroupSelector != null)
        {
            elementGroupSelector.elementGroupsManager = this;
            elementGroupSelector.UpdateDropdownsOptions();
        }
        else
        {
            Debug.LogWarning("ElementGroupSelector not found in scene.");
        }
    }
}


[System.Serializable]
public class SelectableGroup
{
    public string groupID;
    public string url;
    public bool hidable;
    public GroupElement[] elements;

}

[System.Serializable]
public class GroupElement
{
    public GameObject element;
    [Tooltip("Índice del material dentro del array de materiales.")]
    public int materialIndex;
}
