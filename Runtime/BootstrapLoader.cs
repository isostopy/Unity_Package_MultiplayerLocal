using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MainMenu";

    private void Start()
    {
        if (ConnectionManager.Instance == null)
        {
            Debug.Log("No ConnectionManager found, staying in bootstrap.");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
