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

    void Start()
    {
        popupPanel.SetActive(false);
        SimulationManager.Instance.OnSimulationFinished.AddListener(ShowPopup);

        closeButton.onClick.AddListener(() => { ReturnToMenu();});
    }

    private void ShowPopup()
    {
        popupPanel.SetActive(true);

        resultadosText.text = $"VOLTAJE: {SimulationManager.Instance.CurrentGeneratorData.Voltage}V\n" +
                             $"CORRIENTE: {SimulationManager.Instance.CurrentGeneratorData.Current}A\n" +
                             $"FRECUENCIA: {SimulationManager.Instance.CurrentGeneratorData.Frequency}Hz";

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

    private void ReturnToMenu()
    {
   
        SimulationManager.Instance.ResetSimulation();
        SceneManager.LoadScene(menuSceneName);
        Time.timeScale = 1f;
    }
}
