using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PopupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text resultadosText;
    [SerializeField] private TMP_Text erroresText;
    [SerializeField] private TMP_Text tiempoText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button empezarIntermedioButton; // NUEVO: Botón para proceso intermedio
    [SerializeField] private Button empezarApagadoButton;
    [SerializeField] private Button finalizarButton;
    [SerializeField] private string menuSceneName = "Menu";

    [SerializeField] private GameObject otroPopupPanel;
    [SerializeField] private Button abrirOtroPopupButton;
    private Vector3 escalaOriginalOtroPopup;
    public SequenceManager sequenceManager;
    public SimulationManager simulationManager;
    [Header("Referencias Popup Proceso")]
    public ProcessPopupController processPopup;

    // NUEVO: Modos de operación
    public enum ProcessMode { Startup, Intermediate, Shutdown }
    private ProcessMode currentMode = ProcessMode.Startup;

    [Header("Referencias de Cámara")]
    public MoveCamera moveCamera;
    public Transform popupCameraTarget;
    [Header("Objeto a activar/desactivar")]
    public GameObject objetoEspecial;

    void Start()
    {
        popupPanel.SetActive(false);
        otroPopupPanel.SetActive(false);

        if (objetoEspecial != null)
            objetoEspecial.SetActive(false);

        escalaOriginalOtroPopup = otroPopupPanel.transform.localScale;
        otroPopupPanel.transform.localScale = Vector3.zero;

        // NUEVO: Suscribirse a todos los eventos de completado
        if (simulationManager != null)
            simulationManager.OnSimulationFinished.AddListener(StartPopupSequence);

        if (sequenceManager != null)
        {
            sequenceManager.OnStartupComplete.AddListener(() => ShowResultsPopup(ProcessMode.Startup));
            sequenceManager.OnIntermediateComplete.AddListener(() => ShowResultsPopup(ProcessMode.Intermediate));
            sequenceManager.OnShutdownComplete.AddListener(() => ShowResultsPopup(ProcessMode.Shutdown));
        }

        if (moveCamera != null && popupCameraTarget != null)
        {
            moveCamera.popupCameraPosition = popupCameraTarget;
            moveCamera.OnCameraReachedPopupPosition.RemoveListener(ShowPopupAfterDelay);
            moveCamera.OnCameraReachedPopupPosition.AddListener(ShowPopupAfterDelay);
        }

        closeButton.onClick.AddListener(() => { ReturnToMenu(); });
        empezarIntermedioButton.onClick.AddListener(() => { EmpezarIntermedio(); });
        empezarApagadoButton.onClick.AddListener(() => { EmpezarApagado(); });
        finalizarButton.onClick.AddListener(() => { FinalizarSimulacion(); });
        abrirOtroPopupButton.onClick.AddListener(() => { AbrirOtroPopup(); });
        if (processPopup == null)
            processPopup = FindObjectOfType<ProcessPopupController>();
        UpdateButtons();
        if (objetoEspecial != null)
            objetoEspecial.SetActive(false);
    }

    private bool popupSequenceStarted = false;

    // NUEVO: Método para mostrar resultados según el modo
    public void ShowResultsPopup(ProcessMode mode)
    {
        currentMode = mode;
        StartPopupSequence();
    }

    public void ShowPopup()
    {
        float tiempo = 0f;
        string procesoNombre = "";
        bool procesoCompleto = false;

        // NUEVO: Determinar datos según el modo actual
        switch (currentMode)
        {
            case ProcessMode.Startup:
                tiempo = sequenceManager.GetStartupTime();
                procesoNombre = "ENCENDIDO";
                procesoCompleto = sequenceManager.isStartupComplete;
                break;
            case ProcessMode.Intermediate:
                tiempo = sequenceManager.GetIntermediateTime();
                procesoNombre = "OPERACIÓN";
                procesoCompleto = sequenceManager.isIntermediateComplete;
                break;
            case ProcessMode.Shutdown:
                tiempo = sequenceManager.GetCompletionTime();
                procesoNombre = "APAGADO";
                procesoCompleto = sequenceManager.isShutdownComplete;
                break;
        }

        int totalErrores = simulationManager != null ? simulationManager.ActiveErrors.Count : 0;

        string status = (totalErrores == 0 && procesoCompleto) ?
            $"<color=green>¡{procesoNombre} EXITOSO!</color>" :
            $"<color=red>¡{procesoNombre} CON ERRORES!</color>";

        resultadosText.text = $"{status}\n\n" +
                             $"PROCESO: {procesoNombre}\n" +
                             $"PASOS CORRECTOS: {(procesoCompleto ? "COMPLETADOS" : "INCOMPLETOS")}\n" +
                             $"ERRORES COMETIDOS: {totalErrores}";

        tiempoText.text = $"TIEMPO: {tiempo:F2} segundos";

        if (totalErrores == 0)
        {
            erroresText.gameObject.SetActive(false);
        }
        else
        {
            erroresText.gameObject.SetActive(true);
            erroresText.text = "ERRORES:\n";
            foreach (string error in simulationManager.ActiveErrors)
            {
                erroresText.text += $"- {error}\n";
            }
        }

        UpdateButtons();
    }

    // NUEVO: Método para proceso intermedio
    private void EmpezarIntermedio()
    {
        Debug.Log("Iniciando proceso intermedio...");

        // PRIMERO: Cerrar el popup de resultados si está activo
        if (popupPanel != null && popupPanel.activeSelf)
        {
            Debug.Log("Cerrando popup de resultados antes de iniciar intermedio...");
            popupPanel.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => {
                    popupPanel.SetActive(false);
                    ContinueIntermedioProcess();
                });
        }
        else
        {
            ContinueIntermedioProcess();
        }
    }

    private void ContinueIntermedioProcess()
    {
        popupSequenceStarted = false;
        currentMode = ProcessMode.Intermediate;

        if (objetoEspecial != null)
            objetoEspecial.SetActive(false);

        if (moveCamera != null)
        {
            moveCamera.EnableControls();
            moveCamera.MoveToSpecificPosition(1);
        }

        if (simulationManager != null)
            simulationManager.ActiveErrors.Clear();

        if (sequenceManager != null)
            sequenceManager.StartIntermediateProcess();

        // NUEVO: Mostrar popup de proceso intermedio
        if (processPopup != null)
        {
            Debug.Log("Llamando ShowIntermediatePopup...");
            processPopup.ShowIntermediatePopup();
        }
        else
        {
            Debug.LogError("¡processPopup no asignado!");
        }
    }
    private void EmpezarApagado()
    {
        Debug.Log("Iniciando proceso de apagado...");

        // PRIMERO: Cerrar el popup de resultados si está activo
        if (popupPanel != null && popupPanel.activeSelf)
        {
            Debug.Log("Cerrando popup de resultados antes de iniciar apagado...");
            popupPanel.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => {
                    popupPanel.SetActive(false);
                    ContinueApagadoProcess();
                });
        }
        else
        {
            ContinueApagadoProcess();
        }
    }
    private void ContinueApagadoProcess()
    {
        popupSequenceStarted = false;
        currentMode = ProcessMode.Shutdown;

        if (objetoEspecial != null)
            objetoEspecial.SetActive(false);

        if (moveCamera != null)
        {
            moveCamera.EnableControls();
            moveCamera.MoveToSpecificPosition(1);
        }

        if (simulationManager != null)
            simulationManager.ActiveErrors.Clear();

        if (sequenceManager != null)
            sequenceManager.StartShutdownProcess();

        // NUEVO: Mostrar popup de proceso de apagado
        if (processPopup != null)
        {
            Debug.Log("Llamando ShowShutdownPopup...");
            processPopup.ShowShutdownPopup();
        }
        else
        {
            Debug.LogError("¡processPopup no asignado!");
        }
    }

    public void AbrirOtroPopup()
    {
        otroPopupPanel.SetActive(true);
        otroPopupPanel.transform.localScale = Vector3.zero;
        otroPopupPanel.transform.DOScale(escalaOriginalOtroPopup, 0.4f).SetEase(Ease.OutBack);
    }

    private void FinalizarSimulacion()
    {
        ReturnToMenu();
    }

    public void StartPopupSequence()
    {
        Debug.Log("StartPopupSequence llamado");

        if (popupSequenceStarted)
        {
            Debug.Log("Popup sequence ya estaba iniciada, ignorando...");
            return;
        }

        popupSequenceStarted = true;

        if (objetoEspecial != null)
        {
            objetoEspecial.SetActive(true);
            Debug.Log("Objeto especial activado inmediatamente");
        }

        if (moveCamera != null && !moveCamera.IsMovingToPopup())
        {
            Debug.Log("Moviendo cámara a posición popup...");
            moveCamera.MoveToPopupPosition();
        }
        else
        {
            Debug.Log("Fallback: mostrando popup directamente");
            ShowPopupConAnimacion();
        }
    }

    private void ShowPopupAfterDelay()
    {
        Debug.Log("ShowPopupAfterDelay llamado - mostrando popup");
        ShowPopupConAnimacion();
    }

    private void UpdateButtons()
    {
        // NUEVO: Lógica actualizada para los 3 procesos
        switch (currentMode)
        {
            case ProcessMode.Startup:
                empezarIntermedioButton.gameObject.SetActive(true);
                empezarApagadoButton.gameObject.SetActive(false);
                finalizarButton.gameObject.SetActive(false);
                break;
            case ProcessMode.Intermediate:
                empezarIntermedioButton.gameObject.SetActive(false);
                empezarApagadoButton.gameObject.SetActive(true);
                finalizarButton.gameObject.SetActive(false);
                break;
            case ProcessMode.Shutdown:
                empezarIntermedioButton.gameObject.SetActive(false);
                empezarApagadoButton.gameObject.SetActive(false);
                finalizarButton.gameObject.SetActive(true);
                break;
        }
    }

    public void CerrarOtroPopup()
    {
        otroPopupPanel.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                otroPopupPanel.SetActive(false);
                otroPopupPanel.transform.localScale = escalaOriginalOtroPopup;
            });
    }

    private void ShowPopupConAnimacion()
    {
        ShowPopup();
        if (objetoEspecial != null)
            objetoEspecial.SetActive(true);

        popupPanel.SetActive(true);
        popupPanel.transform.localScale = Vector3.zero;
        popupPanel.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void ReturnToMenu()
    {
        popupSequenceStarted = false;

        if (objetoEspecial != null)
            objetoEspecial.SetActive(false);

        if (moveCamera != null)
            moveCamera.EnableControls();

        popupPanel.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                if (simulationManager != null)
                    simulationManager.ResetSimulation();

                sequenceManager.ResetSequence();
                currentMode = ProcessMode.Startup;
                SceneManager.LoadScene(menuSceneName);
                Time.timeScale = 1f;
            });
    }
}