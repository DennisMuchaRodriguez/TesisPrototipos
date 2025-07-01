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
    [SerializeField] private Button closeButton;
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Rangos de Éxito")]
    [SerializeField] private float minVoltageSuccess = 10f;
    [SerializeField] private float maxVoltageSuccess = 15f;
    [SerializeField] private float minFrequencySuccess = 55f;
    [SerializeField] private float maxFrequencySuccess = 65f;

    void Start()
    {
        popupPanel.SetActive(false);
        SimulationManager.Instance.OnSimulationFinished.AddListener(ShowPopup);

        closeButton.onClick.AddListener(() => { ReturnToMenu(); });
    }

    private void ShowPopup()
    {
        popupPanel.SetActive(true);


        float voltage = SimulationManager.Instance.CurrentGeneratorData.Voltage;
        float current = SimulationManager.Instance.CurrentGeneratorData.Current;
        float frequency = SimulationManager.Instance.CurrentGeneratorData.Frequency;

 
        bool voltageSuccess = voltage >= minVoltageSuccess && voltage <= maxVoltageSuccess;
        bool frequencySuccess = frequency >= minFrequencySuccess && frequency <= maxFrequencySuccess;
        bool simulationSuccess = voltageSuccess && frequencySuccess;

      
        string status = simulationSuccess ?
            "<color=green>¡SIMULACIÓN EXITOSA!</color>" :
            "<color=red>¡SIMULACIÓN FALLIDA!</color>";

        resultadosText.text = $"{status}\n\n" +
                             $"VOLTAJE: {voltage}V {(voltageSuccess)}\n" +
                             $"  (Rango ideal: {minVoltageSuccess}-{maxVoltageSuccess}V)\n" +
                             $"CORRIENTE: {current}A\n" +
                             $"FRECUENCIA: {frequency}Hz {(frequencySuccess)}\n" +
                             $"  (Rango ideal: {minFrequencySuccess}-{maxFrequencySuccess}Hz)";

        if (SimulationManager.Instance.ActiveErrors.Count == 0)
        {
            
            erroresText.gameObject.SetActive(false);
        }
        else
        {
            erroresText.gameObject.SetActive(true);
            erroresText.text = "ERRORES DETECTADOS:\n";
            foreach (string error in SimulationManager.Instance.ActiveErrors)
            {
                erroresText.text += $"- {error}\n";
            }
        }
    }

    private void ReturnToMenu()
    {
        SimulationManager.Instance.ResetSimulation();
        SceneManager.LoadScene(menuSceneName);
        Time.timeScale = 1f;
    }
}
