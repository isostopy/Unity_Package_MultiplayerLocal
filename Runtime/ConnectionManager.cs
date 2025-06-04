using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;
    
    [SerializeField] private bool useAutoDiscovery = false;
    [SerializeField] private DeviceRol rol;

    private NetworkHandler networkHandler;
    private ClientManager clientManager;

    private IPEndPoint serverEndPoint;
    private IPEndPoint selectedClient;
    private bool isSelected;

    public bool IsSelected() => isSelected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        networkHandler = new NetworkHandler();
        clientManager = new ClientManager();

        networkHandler.OnMessageReceived += HandleMessage;
        clientManager.OnClientsUpdated.AddListener(() =>
            Debug.Log($"Clients count: {clientManager.GetClients().Count}")
        );

        networkHandler.StartListening();

        if (rol == DeviceRol.Server)
        {
            var localIP = GetLocalIPAddress();
            if (IPAddress.TryParse(localIP, out var ip))
            {
                var localEndpoint = new IPEndPoint(ip, NetworkConstants.ServerPort);
                clientManager.AddClient(localEndpoint);
                Debug.Log($"Servidor registrado como cliente en {localEndpoint}");
            }
            else
            {
                Debug.LogWarning("No se pudo registrar el servidor como cliente: IP no válida");
            }

            if (useAutoDiscovery)
            {
                StartCoroutine(AutoBroadcastPing());
            }
        }
    }

    private IEnumerator AutoBroadcastPing()
    {
        var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, NetworkConstants.ServerPort);

        while (true)
        {
            networkHandler.SendMessage(NetworkConstants.MsgPing, broadcastEndpoint);
            Debug.Log("[Server][AutoDiscovery] Sent Ping broadcast");
            yield return new WaitForSeconds(2f);
        }
    }

    private void HandleMessage(string message, UdpReceiveResult result)
    {
        if (rol == DeviceRol.Server && message == NetworkConstants.MsgClientHello)
        {
            var existingClients = clientManager.GetClients();
            if (!existingClients.Contains(result.RemoteEndPoint))
            {
                clientManager.AddClient(result.RemoteEndPoint);
                Debug.Log($"Registered client: {result.RemoteEndPoint}");
            }
        }

        if (useAutoDiscovery && message == NetworkConstants.MsgPing && rol == DeviceRol.Client)
        {
            SendMessageToServer(NetworkConstants.MsgClientHello);
            Debug.Log("[Client][AutoDiscovery] Received Ping, sent ClientHello");
        }

        if (message == NetworkConstants.MsgSelect && rol == DeviceRol.Client)
        {
            isSelected = true;
            Debug.Log("This client is now SELECTED.");
        }

        if (message == NetworkConstants.MsgDeselect && rol == DeviceRol.Client)
        {
            isSelected = false;
            Debug.Log("This client is now DESELECTED.");
        }
    }

    public void SubscribeToMessages(NetworkHandler.MessageReceivedHandler handler)
    {
        networkHandler.OnMessageReceived -= handler;
        networkHandler.OnMessageReceived += handler;
    }

    public void UnsubscribeFromMessages(NetworkHandler.MessageReceivedHandler handler)
    {
        networkHandler.OnMessageReceived -= handler;
    }

    public void SubscribeToClientsUpdated(UnityAction callback)
    {
        clientManager.OnClientsUpdated.RemoveListener(callback);
        clientManager.OnClientsUpdated.AddListener(callback);
    }

    public void SendMessageToAllClients(string message)
    {
        foreach (var client in clientManager.GetClients())
        {
            networkHandler.SendMessage(message, client);
        }
    }

    public void SendMessageToServer(string message)
    {
        if (serverEndPoint != null)
        {
            networkHandler.SendMessage(message, serverEndPoint);
        }
        else
        {
            Debug.LogWarning("No server endpoint defined.");
        }
    }

    public void SetServerEndpoint(string ipString)
    {
        if (IPAddress.TryParse(ipString, out var ip))
        {
            serverEndPoint = new IPEndPoint(ip, NetworkConstants.ServerPort);
            Debug.Log($"Server endpoint set to {serverEndPoint}");
        }
        else
        {
            Debug.LogError($"Invalid IP entered: {ipString}");
        }
    }

    public IPEndPoint GetServerEndPoint()
    {
        return serverEndPoint;
    }

    public DeviceRol GetRole()
    {
        return rol;
    }

    public void SetRole(DeviceRol newRole)
    {
        rol = newRole;
    }

    public void SelectClientByIndex(int index)
    {
        var clients = clientManager.GetClients();
        if (index >= 0 && index < clients.Count)
        {
            var client = clients[index];

            if (selectedClient != null)
            {
                networkHandler.SendMessage(NetworkConstants.MsgDeselect, selectedClient);
            }

            selectedClient = client;
            networkHandler.SendMessage(NetworkConstants.MsgSelect, selectedClient);
            Debug.Log($"Selected client: {client}");
        }
    }

    public List<string> GetClientIPList()
    {
        List<string> ipList = new();
        foreach (var client in clientManager.GetClients())
        {
            ipList.Add(client.Address.ToString());
        }
        return ipList;
    }

    public string GetLocalIPAddress()
    {
        foreach (var host in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (host.AddressFamily == AddressFamily.InterNetwork)
            {
                return host.ToString();
            }
        }
        return "127.0.0.1";
    }
}
