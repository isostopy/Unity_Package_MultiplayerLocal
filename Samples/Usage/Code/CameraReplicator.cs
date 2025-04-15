using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class CameraReplicator : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    public float lerpSpeed = 5f;
    private Vector3 cameraTargetPosition;
    private Quaternion cameraTargetRotation;

    float timer;

    private void Start()
    {
        cameraTargetPosition = targetCamera.transform.position;
        cameraTargetRotation = targetCamera.transform.rotation;

        ConnectionManager.Instance.messageReceived.AddListener(ProcessMessage);
    }

    private void Update()
    {
        if (ConnectionManager.Instance.selected)
        {
            timer += Time.deltaTime;
            if (timer >= 0.05f)
            {
                SendCameraTransform();
                timer = 0f;
            }
        }
    }

    void ProcessMessage(string message, UdpReceiveResult result)
    {
        string[] parts = message.Split('|');

        if (parts.Length == 3 && parts[0] == "Camera" && ConnectionManager.Instance.rol == DeviceRol.Server)
        {
            string[] posData = parts[1].Split(';');
            string[] rotData = parts[2].Split(';');

            if (posData.Length == 3 && rotData.Length == 4)
            {
                Vector3 position = new Vector3(
                    float.Parse(posData[0]),
                    float.Parse(posData[1]),
                    float.Parse(posData[2])
                );

                Quaternion rotation = new Quaternion(
                    float.Parse(rotData[0]),
                    float.Parse(rotData[1]),
                    float.Parse(rotData[2]),
                    float.Parse(rotData[3])
                );

                ApplyTransform(position, rotation);
            }
        }
    }

    void SendCameraTransform()
    {
        OnScreenLog.Instance.Log("Send Camera Transform");

        Vector3 pos = Camera.main.transform.position;
        Quaternion rot = Camera.main.transform.rotation;

        string message = $"Camera|{pos.x};{pos.y};{pos.z}|{rot.x};{rot.y};{rot.z};{rot.w}";

        ConnectionManager.Instance.SendMessageToEndpoint(message, ConnectionManager.Instance.serverEndPoint);
    }

    private void ApplyTransform(Vector3 position, Quaternion rotation)
    {
        if (targetCamera == null) return;
        
        cameraTargetPosition = position;
        cameraTargetRotation = rotation;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;
        targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, cameraTargetPosition, Time.deltaTime * lerpSpeed);
        targetCamera.transform.rotation = Quaternion.Slerp(targetCamera.transform.rotation, cameraTargetRotation, Time.deltaTime * lerpSpeed);
    }

}
