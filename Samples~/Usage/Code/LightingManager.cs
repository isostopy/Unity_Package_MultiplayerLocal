using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LightingPreset
{
    public string name;
    public Color SkyColor;
	public Color EquatorColor;
	public Color GroundColor;
}

public class LightingManager : MonoBehaviour
{
    [SerializeField] public List<LightingPreset> presets = new();
    private Dictionary<string, LightingPreset> presetMap;

    private void Awake()
    {
        presetMap = new();
        foreach (var preset in presets)
        {
            if (!presetMap.ContainsKey(preset.name))
                presetMap[preset.name] = preset;
        }
    }

    public void ApplyPreset(string name, float t = 0.5f)
    {
        if (!presetMap.TryGetValue(name, out var preset))
        {
            Debug.LogWarning($"Lighting preset '{name}' not found.");
            return;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = preset.SkyColor;
        RenderSettings.ambientEquatorColor = preset.EquatorColor;
		RenderSettings.ambientGroundColor = preset.GroundColor;

		Debug.Log($"Applied lighting preset: {name}");
    }
}
