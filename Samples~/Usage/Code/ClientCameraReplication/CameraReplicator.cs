using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class CameraReplicator : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    public float lerpSpeed = 5f;

    private Vector3 cameraTargetPosition;
    private Quaternion cameraTargetRotation;

    private float timer;

    private IEnumerator Start()
    {
        cameraTargetPosition = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
        cameraTargetRotation = targetCamera != null ? targetCamera.transform.rotation : Quaternion.identity;

        // Espera hasta que ConnectionManager esté disponible
        yield return new WaitUntil(() => ConnectionManager.Instance != null);

        ConnectionManager.Instance.SubscribeToMessages(OnMessageReceived);
    }

    private void Update()
    {
        if (ConnectionManager.Instance.GetRole() == DeviceRol.Client &&
            ConnectionManager.Instance.IsSelected() &&
            ConnectionManager.Instance.GetServerEndPoint() != null)
        {
            timer += Time.deltaTime;
            if (timer >= 0.05f)
            {
                SendCameraToServer();
                timer = 0f;
            }
        }
    }

    private void SendCameraToServer()
    {
        if (Camera.main == null) return;

        Vector3 pos = Camera.main.transform.position;
        Quaternion rot = Camera.main.transform.rotation;

        string message = SerializeCameraData(pos, rot);
        ConnectionManager.Instance.SendMessageToServer(message);
    }

    private void OnMessageReceived(string message, UdpReceiveResult result)
    {
        if (ConnectionManager.Instance.GetRole() != DeviceRol.Server) return;

        string[] parts = message.Split('|');
        if (parts.Length != 3 || parts[0] != "Camera") return;

        if (TryParseVector3(parts[1], out Vector3 position) && TryParseQuaternion(parts[2], out Quaternion rotation))
        {
            ApplyTransform(position, rotation);
        }
    }

    private void ApplyTransform(Vector3 position, Quaternion rotation)
    {
        if (targetCamera == null) return;

        cameraTargetPosition = position;
        cameraTargetRotation = rotation;
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, cameraTargetPosition, Time.deltaTime * lerpSpeed);
        targetCamera.transform.rotation = Quaternion.Slerp(targetCamera.transform.rotation, cameraTargetRotation, Time.deltaTime * lerpSpeed);
    }

    private string SerializeCameraData(Vector3 pos, Quaternion rot)
    {
        return $"Camera|{pos.x};{pos.y};{pos.z}|{rot.x};{rot.y};{rot.z};{rot.w}";
    }

    private bool TryParseVector3(string data, out Vector3 result)
    {
        string[] values = data.Split(';');
        result = Vector3.zero;

        return values.Length == 3 &&
               float.TryParse(values[0], out result.x) &&
               float.TryParse(values[1], out result.y) &&
               float.TryParse(values[2], out result.z);
    }

    private bool TryParseQuaternion(string data, out Quaternion result)
    {
        string[] values = data.Split(';');
        result = Quaternion.identity;

        return values.Length == 4 &&
               float.TryParse(values[0], out result.x) &&
               float.TryParse(values[1], out result.y) &&
               float.TryParse(values[2], out result.z) &&
               float.TryParse(values[3], out result.w);
    }

    private void OnDestroy()
    {
        if (ConnectionManager.Instance != null)
        {
            ConnectionManager.Instance.UnsubscribeFromMessages(OnMessageReceived);
        }
    }

}
