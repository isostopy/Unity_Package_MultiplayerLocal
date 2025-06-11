using UnityEngine;

public class HideOnClient : MonoBehaviour
{
    [Tooltip("Estos objetos se desactivarán si el rol actual es Cliente.")]
    public GameObject[] objectsToHide;

    void Start()
    {
        if (ConnectionManager.Instance != null &&
            ConnectionManager.Instance.GetRole() == DeviceRol.Client)
        {
            foreach (GameObject obj in objectsToHide)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}
