using UnityEngine;
using UnityEngine.SceneManagement;

public class RoleSelector : MonoBehaviour
{
    [SerializeField] private string sceneName = "GameScene1";

    public void SetAsServer()
    {
        ConnectionManager.Instance.SetRole(DeviceRol.Server);
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void SetAsClient()
    {
        ConnectionManager.Instance.SetRole(DeviceRol.Client);
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
