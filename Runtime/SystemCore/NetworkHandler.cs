using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkHandler
{
    private UdpClient udpClient;
    private IPEndPoint listenEndPoint;

    public delegate void MessageReceivedHandler(string message, UdpReceiveResult result);
    public event MessageReceivedHandler OnMessageReceived;

    private CancellationTokenSource cancellationTokenSource;
    public bool IsListening { get; private set; } = false;

#if UNITY_ANDROID
    private AndroidJavaObject multicastLock;
#endif

    public NetworkHandler()
    {
        InitializeSocket();
    }

    private void InitializeSocket()
    {
        listenEndPoint = new IPEndPoint(IPAddress.Any, NetworkConstants.ServerPort);
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        udpClient.Client.Bind(listenEndPoint);

#if UNITY_ANDROID
        try
        {
            using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (AndroidJavaObject wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
                {
                    multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "unityMulticastLock");
                    multicastLock.Call("setReferenceCounted", false);
                    multicastLock.Call("acquire");
                    Debug.Log("[Android] MulticastLock acquired");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to acquire MulticastLock: " + ex.Message);
        }
#endif

        IsListening = false;
    }

    public void StartListening()
    {
        if (IsListening) return;

        cancellationTokenSource = new CancellationTokenSource();
        _ = ListenAsync(cancellationTokenSource.Token);
        IsListening = true;
        Debug.Log("[NetworkHandler] Listening for UDP messages...");
    }

    public void StopListening()
    {
        cancellationTokenSource?.Cancel();
        udpClient?.Close();
        IsListening = false;

#if UNITY_ANDROID
        if (multicastLock != null)
        {
            multicastLock.Call("release");
            multicastLock = null;
            Debug.Log("[Android] MulticastLock released");
        }
#endif
    }

    private async Task ListenAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);

                try
                {
                    OnMessageReceived?.Invoke(message, result);
                }
                catch (Exception handlerEx)
                {
                    Debug.LogError("Error in OnMessageReceived handler: " + handlerEx.Message);
                }
            }
            catch (SocketException e)
            {
                Debug.LogWarning("Socket error: " + e.Message);
                RestartListening();
                break;
            }
            catch (ObjectDisposedException)
            {
                Debug.LogWarning("Socket closed.");
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError("Unexpected error in ListenAsync: " + ex.Message);
                RestartListening();
                break;
            }
        }
    }

    public void SendMessage(string message, IPEndPoint endPoint)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, endPoint);
    }

    public void RestartListening(bool clearListeners = false)
    {
        StopListening();
        InitializeSocket();

        if (clearListeners)
        {
            OnMessageReceived = null;
            Debug.Log("[NetworkHandler] Cleared all message listeners.");
        }

        StartListening();
    }

}
