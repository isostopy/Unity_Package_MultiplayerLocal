using System.Collections.Generic;
using System.Net;
using UnityEngine.Events;

public class ClientManager
{
    private readonly List<IPEndPoint> clients = new();
    public UnityEvent OnClientsUpdated = new();

    public void AddClient(IPEndPoint client)
    {
        if (!clients.Contains(client))
        {
            clients.Add(client);
            OnClientsUpdated.Invoke();
        }
    }

    public void RemoveClient(IPEndPoint client)
    {
        if (clients.Contains(client))
        {
            clients.Remove(client);
            OnClientsUpdated.Invoke();
        }
    }


    public void ClearClients()
    {
        clients.Clear();
        OnClientsUpdated.Invoke();
    }

    public IReadOnlyList<IPEndPoint> GetClients() => clients.AsReadOnly();
}
