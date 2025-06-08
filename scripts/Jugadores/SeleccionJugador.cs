using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SeleccionJugador : MonoBehaviour
{
    [Header("Slots de jugador")]
    public Button[] botonesSlots;    // Botones para elegir o crear jugador
    public Text[] textosSlots;       // Textos que muestran nombre o "Vacío"

    [Header("Editar / Borrar")]
    public Button[] botonesEditar;   // Botones para abrir popup de edición
    public Button[] botonesBorrar;   // Botones para borrar jugador

    [Header("Popup Editar Nombre")]
    public GameObject panelEditar;   // Panel que contiene InputField y botones
    public InputField inputNombre;   // InputField para escribir nombre
    public Button botonConfirmar;    // Botón para guardar nuevo nombre
    public Button botonCancelar;     // Botón para cancelar la edición

    private int slotSeleccionado;    // Slot que estamos editando

    void Start()
    {
        // 1) Configurar cada slot: Mostrar nombre o "Vacío", y asignar listener al botón de selección
        for (int i = 0; i < botonesSlots.Length; i++)
        {
            int slot = i + 1;
            var jugador = DBManager.Instance.GetJugador(slot);

            textosSlots[i].text = jugador != null ? jugador.Nombre : "Vacío";

            // Listener para seleccionar slot (jugar o crear)
            botonesSlots[i].onClick.AddListener(() => SeleccionarSlot(slot));
        }

        // 2) Configurar botones de Editar y Borrar para cada slot
        for (int i = 0; i < botonesEditar.Length; i++)
        {
            int slot = i + 1;
            botonesEditar[i].onClick.AddListener(() => MostrarPopupEditar(slot));
        }

        for (int i = 0; i < botonesBorrar.Length; i++)
        {
            int slot = i + 1;
            botonesBorrar[i].onClick.AddListener(() => BorrarJugador(slot));
        }

        // 3) Configurar botones del popup: Confirmar y Cancelar
        botonConfirmar.onClick.AddListener(GuardarNombre);
        botonCancelar.onClick.AddListener(CancelarEdicion);

        // 4) Al inicio, ocultamos el panel de edición
        panelEditar.SetActive(false);
    }

    // ------------------------------
    //  SeleccionarSlot: si no existe el jugador, abre popup; si existe, guarda slot y carga menú
    void SeleccionarSlot(int slot)
    {
        slotSeleccionado = slot;
        var jugador = DBManager.Instance.GetJugador(slot);

        if (jugador == null)
        {
            // No existe: abrir popup para crear nombre
            inputNombre.text = "";
            panelEditar.SetActive(true);
        }
        else
        {
            // Ya existe: guardamos el slot activo y cargamos la escena de juego/menú
            PlayerPrefs.SetInt("SlotActivo", slot);
            SceneManager.LoadScene("menu");
        }
    }

    // ------------------------------
    //  MostrarPopupEditar: carga nombre existente (o vacío) y muestra el panel
    void MostrarPopupEditar(int slot)
    {
        slotSeleccionado = slot;
        var jugador = DBManager.Instance.GetJugador(slot);
        inputNombre.text = jugador != null ? jugador.Nombre : "";
        panelEditar.SetActive(true);
    }

    // ------------------------------
    //  GuardarNombre: al pulsar Confirmar en popup
    void GuardarNombre()
    {
        string nuevoNombre = inputNombre.text.Trim();
        if (string.IsNullOrEmpty(nuevoNombre))
        {
            // Si el campo está vacío, no hacemos nada
            return;
        }
        try {
            var jugadorExistente = DBManager.Instance.GetJugador(slotSeleccionado);
            if (jugadorExistente == null)
            {
                // Crear nuevo jugador con ese nombre
                Debug.Log($"[Salvar] Creando jugador Slot={slotSeleccionado}, Nombre={nuevoNombre}");
                DBManager.Instance.CrearJugador(slotSeleccionado, nuevoNombre);
            }
            else
            {
                // Actualizar nombre del jugador existente
                Debug.Log($"[Salvar] Actualizando jugador Slot={slotSeleccionado}, Nombre={nuevoNombre}");
                jugadorExistente.Nombre = nuevoNombre;
                DBManager.Instance.ActualizarJugador(jugadorExistente);
            }

            // Actualizar el texto del slot en la UI
            textosSlots[slotSeleccionado - 1].text = nuevoNombre;

            // Cerrar popup
            panelEditar.SetActive(false);
            inputNombre.text = "";
            var after = DBManager.Instance.GetJugador(slotSeleccionado);
            Debug.Log($"[Salvar] Después GetJugador({slotSeleccionado}) = {(after != null ? after.Nombre : "NULL")}");
        

        } catch (System.Exception ex) {
            Debug.LogError($"[Salvar] Error al guardar jugador Slot={slotSeleccionado}: {ex}");
        }
    }
    // ------------------------------
    //  CancelarEdicion: oculta el popup sin guardar cambios
    void CancelarEdicion()
    {
        panelEditar.SetActive(false);
        inputNombre.text = "";
    }

    // ------------------------------
    //  BorrarJugador: elimina de la base de datos y actualiza UI a "Vacío"
    void BorrarJugador(int slot)
    {
        var jugador = DBManager.Instance.GetJugador(slot);
        if (jugador != null)
        {
            DBManager.Instance.BorrarJugador(slot);
        }
        textosSlots[slot - 1].text = "Vacío";
    }
}
