using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PopupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text resultadosText;
    [SerializeField] private TMP_Text erroresText;
    [SerializeField] private TMP_Text tiempoText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button empezarApagadoButton;
    [SerializeField] private Button finalizarButton;
    [SerializeField] private string menuSceneName = "Menu";
    public SequenceManager sequenceManager;
    public SimulationManager simulationManager;
    private bool isApagadoMode = false;

    void Start()
    {
        popupPanel.SetActive(false);

        if (simulationManager != null)
            simulationManager.OnSimulationFinished.AddListener(ShowPopup);

        closeButton.onClick.AddListener(() => { ReturnToMenu(); });
        empezarApagadoButton.onClick.AddListener(() => { EmpezarApagado(); });
        finalizarButton.onClick.AddListener(() => { FinalizarSimulacion(); });

        UpdateButtons();
    }

    public void ShowPopup()
    {

        popupPanel.SetActive(true);

        float tiempo = isApagadoMode ?
            sequenceManager.GetCompletionTime() :
            sequenceManager.GetStartupTime();

        int totalErrores = simulationManager != null ? simulationManager.ActiveErrors.Count : 0;
        bool procesoCompleto = isApagadoMode ?
            sequenceManager.isShutdownComplete :
            sequenceManager.isStartupComplete;


        string procesoNombre = isApagadoMode ? "APAGADO" : "ENCENDIDO";

        string status = (totalErrores == 0 && procesoCompleto) ?
            $"<color=green>¡{procesoNombre} EXITOSO!</color>" :
            $"<color=red>¡{procesoNombre} CON ERRORES!</color>";

        resultadosText.text = $"{status}\n\n" +
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

    private void EmpezarApagado()
    {
        isApagadoMode = true;
        popupPanel.SetActive(false);

        if (simulationManager != null)
            simulationManager.ActiveErrors.Clear();

        sequenceManager.ResetApagadoTime();
    }

    private void FinalizarSimulacion()
    {
        ReturnToMenu();
    }

    private void UpdateButtons()
    {
        if (isApagadoMode)
        {
            empezarApagadoButton.gameObject.SetActive(false);
            finalizarButton.gameObject.SetActive(true);
        }
        else
        {
            empezarApagadoButton.gameObject.SetActive(true);
            finalizarButton.gameObject.SetActive(false);
        }
    }

    private void ReturnToMenu()
    {
        if (simulationManager != null)
            simulationManager.ResetSimulation();

        sequenceManager.ResetSequence();
        isApagadoMode = false;
        SceneManager.LoadScene(menuSceneName);
        Time.timeScale = 1f;
    }
}