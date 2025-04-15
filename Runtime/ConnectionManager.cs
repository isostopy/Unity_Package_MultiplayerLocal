using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine.Events;

[System.Serializable]
public class MessageEvent : UnityEvent<string, UdpReceiveResult> { }

public enum DeviceRol
{
    Server,
    Client
}


public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    public DeviceRol rol;
    public List<string> selectableScenes = new List<string>();

    private string localIP;
    private string broadcastIP;

    [HideInInspector] public UdpClient udpClient;

    [HideInInspector] public List<IPEndPoint> clients = new List<IPEndPoint>();
    [HideInInspector] public IPEndPoint broadcastEndpoint;
    [HideInInspector] public IPEndPoint serverEndPoint;
    [HideInInspector] public IPEndPoint selectedClient;

    private AndroidJavaObject multicastLock;

    [HideInInspector] public MessageEvent messageReceived;
    [HideInInspector] public UnityEvent clientsFound;

    [HideInInspector] public bool selected;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Initialize();

    }

    private void Initialize()
    {
        try
        {
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 50000));
            Debug.Log("Port 50000 is accessible.");
        }
        catch
        {
            Debug.Log("Port 50000 is not accessible.");
        }

        if(rol == DeviceRol.Server)
        {
            udpClient.EnableBroadcast = true;
            localIP = GetLocalIPAddress();
            broadcastIP = localIP.Substring(0, localIP.LastIndexOf('.')) + ".255";
            broadcastEndpoint = new IPEndPoint(IPAddress.Parse(broadcastIP), 50000);

            EnableMulticastLock();
        }

        ListenForMessages();
    }

    private string GetLocalIPAddress()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !ip.Address.ToString().StartsWith("127."))
                    {
                        return ip.Address.ToString();
                    }
                }
            }
        }
        return "192.168.43.255";
    }

    private void EnableMulticastLock()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (AndroidJavaObject wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
                {
                    multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "mylock");
                    multicastLock.Call("setReferenceCounted", true);
                    multicastLock.Call("acquire");
                    Debug.Log("Multicast lock acquired!");
                }
            }
        }
    }

    async void ListenForMessages()
    {
        while (true)
        {
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                Debug.Log($"Server Received: {receivedMessage} from {result.RemoteEndPoint}");

                messageReceived.Invoke(receivedMessage, result);
                ProcessMessage(receivedMessage, result);
            }
            catch (SocketException ex)
            {
                Debug.Log($"UDP Receive Error: {ex.Message}");
                break;
            }
        }
    }

    void ProcessMessage(string message, UdpReceiveResult result)
    {
        string[] parts = message.Split('|');

        if (parts.Length >= 1 && parts[0] == "ClientConected")
        {
            if (rol != DeviceRol.Server) return;
            AddClient(result.RemoteEndPoint);
            return;
        }

        if (parts.Length >= 1 && parts[0] == "DiscoverClients")
        {
            if(rol != DeviceRol.Client) return;
            serverEndPoint = result.RemoteEndPoint;
            SendClientData();
            return;
        }

        if (parts.Length >= 1 && parts[0] == "Select")
        {
            selected = true;
            return;
        }

        if (parts.Length >= 1 && parts[0] == "Deselect")
        {
            selected = false;
            return;
        }
    }

    public void FindClients()
    {
        if (udpClient == null || rol != DeviceRol.Server) return;

        if (selectedClient != null) SendMessageToEndpoint("Deselect", selectedClient);
        selectedClient = null;

        clients.Clear();
        clientsFound.Invoke();
        SendMessageToEndpoint("DiscoverClients", broadcastEndpoint);
    }

    private void AddClient(IPEndPoint clientEndPoint)
    {
        clients.Add(clientEndPoint);
        Debug.Log("Client Conected: " + clientEndPoint.Address.ToString());
        clientsFound.Invoke();
    }

    public void SelectClient(IPEndPoint client)
    {
        if (client == null) return;

        Debug.Log("Selected Client: " + client.Address.ToString());

        if (selectedClient != null) SendMessageToEndpoint("Deselect", selectedClient);
        selectedClient = client;
        SendMessageToEndpoint("Select", selectedClient);
    }

    public void SendMessageToEndpoint(string message, IPEndPoint endPoint)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        try
        {
            udpClient.Send(data, data.Length, endPoint);
            Debug.Log($"Message sent: {message} to {endPoint.Address.ToString()}");
        }
        catch (SocketException ex)
        {
            Debug.Log("Message failed: " + ex.Message);
        }
    }

    void SendClientData()
    {
        SendMessageToEndpoint("ClientConected", serverEndPoint);
    }

    private void OnDestroy()
    {
        messageReceived.RemoveAllListeners();
        clientsFound.RemoveAllListeners();
    }
}
