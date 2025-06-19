public class ClientDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button clienteActivoButton;
    [SerializeField] private TMP_Text clienteActivoText;
    [SerializeField] private GameObject clientesPanel;
    [SerializeField] private GameObject clienteButtonPrefab;
    [SerializeField] private Transform clientesContainer;

    private Dictionary<string, string> ipToUserName = new();
    private List<Button> clienteButtons = new();
    private string clienteSeleccionado;

    private void Start()
    {
        clienteActivoButton.onClick.AddListener(TogglePanelClientes);
        ConnectionManager.Instance.SubscribeToClientsUpdated(ActualizarListaClientes);
        ActualizarListaClientes();
    }

    private void TogglePanelClientes()
    {
        clientesPanel.SetActive(!clientesPanel.activeSelf);
    }

    private void ActualizarListaClientes()
    {
        LimpiarClientes();

        var listaIPs = ConnectionManager.Instance.GetClientIPList();
        if (listaIPs.Count == 0) return;

        int counter = 1;
        foreach (var ip in listaIPs)
        {
            string nombre = $"Usuario {counter++}";
            ipToUserName[ip] = nombre;

            GameObject botonObj = Instantiate(clienteButtonPrefab, clientesContainer);
            TMP_Text texto = botonObj.GetComponentInChildren<TMP_Text>();
            texto.text = nombre;

            Button boton = botonObj.GetComponent<Button>();
            string ipCapturada = ip;
            boton.onClick.AddListener(() => SeleccionarCliente(ipCapturada));

            clienteButtons.Add(boton);
        }

        // Selecciona el primero por defecto si aún no hay selección
        if (string.IsNullOrEmpty(clienteSeleccionado) && listaIPs.Count > 0)
        {
            SeleccionarCliente(listaIPs[0]);
        }
    }

    private void LimpiarClientes()
    {
        foreach (var btn in clienteButtons)
        {
            Destroy(btn.gameObject);
        }
        clienteButtons.Clear();
    }

    private void SeleccionarCliente(string ip)
    {
        clienteSeleccionado = ip;
        string nombre = ipToUserName[ip];
        clienteActivoText.text = nombre;

        clientesPanel.SetActive(false); // Oculta el panel

        ConnectionManager.Instance.SelectClientByIP(ip);
    }

    private void OnDestroy()
    {
        clienteActivoButton.onClick.RemoveAllListeners();
        ConnectionManager.Instance?.UnsubscribeFromMessages(ActualizarListaClientes);
    }
}
