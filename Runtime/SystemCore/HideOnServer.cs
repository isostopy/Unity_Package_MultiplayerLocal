using UnityEngine;

public class HideOnServer : MonoBehaviour
{
    [Tooltip("Estos objetos se desactivarán si el rol actual es Server.")]
    public GameObject[] objectsToHide;

    void Start()
    {
        if (ConnectionManager.Instance != null &&
            ConnectionManager.Instance.GetRole() == DeviceRol.Server)
        {
            foreach (GameObject obj in objectsToHide)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}
