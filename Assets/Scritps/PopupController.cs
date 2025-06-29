using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PopupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text resultadosText;
    [SerializeField] private TMP_Text erroresText;
    [SerializeField] private Button closeButton;

    void Start()
    {
        popupPanel.SetActive(false);
        SimulationManager.Instance.OnSimulationFinished.AddListener(ShowPopup);

        closeButton.onClick.AddListener(() => {
            popupPanel.SetActive(false);
            SimulationManager.Instance.ResetSimulation();
        });
    }

    private void ShowPopup()
    {
        popupPanel.SetActive(true);

        
        bool isSuccessful = SimulationManager.Instance.CurrentGeneratorData.Voltage >=
                           SimulationManager.Instance.MinIdealVoltage &&
                           SimulationManager.Instance.CurrentGeneratorData.Voltage <=
                           SimulationManager.Instance.MaxIdealVoltage;

        string status = isSuccessful ?
            "<color=green>SIMULACIÓN EXITOSA</color>" :
            "<color=red>SIMULACIÓN FALLIDA</color>";

        resultadosText.text = $"{status}\n\n" +
                            $"VOLTAJE: {SimulationManager.Instance.CurrentGeneratorData.Voltage}V\n" +
                            $"CORRIENTE: {SimulationManager.Instance.CurrentGeneratorData.Current}A\n" +
                            $"FRECUENCIA: {SimulationManager.Instance.CurrentGeneratorData.Frequency}Hz\n\n" +
                            $"Rango Ideal: 10V - 12V";

        if (SimulationManager.Instance.ActiveErrors.Count == 0)
        {
            erroresText.text = "0 ERRORES DETECTADOS";
        }
        else
        {
            erroresText.text = "ERRORES:\n";
            foreach (string error in SimulationManager.Instance.ActiveErrors)
            {
                erroresText.text += $"- {error}\n";
            }
        }
    }
}
