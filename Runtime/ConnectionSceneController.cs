using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectionSceneController : MonoBehaviour
{
    public GameObject serverUI;
    public GameObject clientUI;

    void Start()
    {
        if (ConnectionManager.Instance.GetRole() == DeviceRol.Server)
        {
            serverUI.SetActive(true);
            clientUI.SetActive(false);
        }
        else
        {
            serverUI.SetActive(false);
            clientUI.SetActive(true);
        }
    }
}
