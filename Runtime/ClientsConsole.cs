using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientsConsole : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button showClientsButton;
    [SerializeField] private TMP_Text outputText;

    [Header("Auto Update Settings")]
    [SerializeField] private bool autoUpdate = false;
    [SerializeField] private float autoUpdateInterval = 2f;

    private Coroutine displayCoroutine;
    private Coroutine autoUpdateCoroutine;
    private bool lastAutoUpdateState = false;

    void Start()
    {
        if(showClientsButton != null) showClientsButton.onClick.AddListener(OnShowClientsClicked);
        outputText.text = "";
    }

    void Update()
    {

        if (autoUpdate != lastAutoUpdateState)
        {
            lastAutoUpdateState = autoUpdate;

            if (autoUpdate)
            {
                autoUpdateCoroutine = StartCoroutine(AutoUpdateClients());
            }
            else
            {
                if (autoUpdateCoroutine != null)
                    StopCoroutine(autoUpdateCoroutine);

                outputText.text = "";
            }
        }
    }

    private void OnShowClientsClicked()
    {
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        displayCoroutine = StartCoroutine(DisplayClientsOnce());
    }

    private IEnumerator DisplayClientsOnce()
    {
        List<string> clients = ConnectionManager.Instance.GetClientIPList();
        outputText.text = string.Join("\n", clients);

        yield return new WaitForSeconds(5f);
        outputText.text = "";
        displayCoroutine = null;
    }

    private IEnumerator AutoUpdateClients()
    {
        while (true)
        {
            List<string> clients = ConnectionManager.Instance.GetClientIPList();
            outputText.text = string.Join("\n", clients);
            yield return new WaitForSeconds(autoUpdateInterval);
        }
    }

    private void OnDestroy()
    {
        if (showClientsButton != null) showClientsButton.onClick.RemoveListener(OnShowClientsClicked);
    }
}
