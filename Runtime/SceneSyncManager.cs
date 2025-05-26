using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSyncManager : MonoBehaviour
{

    private void Start()
    {
        ConnectionManager.Instance?.SubscribeToMessages(OnMessageReceived);
    }

    private void OnMessageReceived(string message, UdpReceiveResult result)
    {
        if (message.StartsWith(NetworkConstants.MsgLoadScene + "|"))
        {
            string[] parts = message.Split('|');
            if (parts.Length == 2)
            {
                string sceneName = parts[1];
                SceneManager.LoadScene(sceneName);
                
            }
        }
    }

    public void RequestSceneLoad(string sceneName)
    {
        if (ConnectionManager.Instance.GetRole() == DeviceRol.Server)
        {
            ConnectionManager.Instance.SendMessageToAllClients(NetworkConstants.MsgLoadScene + "|" + sceneName);
        }
    }

    public void RequestLocalSceneLoad(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


    private void OnDestroy()
    {
        ConnectionManager.Instance?.UnsubscribeFromMessages(OnMessageReceived);
    }
}
