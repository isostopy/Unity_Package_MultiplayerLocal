using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] string sceneName;
    [SerializeField] float timeToLoad = 0.5f;

    void Start()
    {
        Invoke("LoadScene", timeToLoad);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
