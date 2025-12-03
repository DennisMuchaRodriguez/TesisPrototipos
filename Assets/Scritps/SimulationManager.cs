using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement; 

public class SimulationManager : MonoBehaviour
{
    public class GeneratorData
    {
        public float Voltage = 0f;
        public float Current = 0f;
        public float Frequency = 60f;
    }

    public GeneratorData CurrentGeneratorData = new GeneratorData();
    public List<string> ActiveErrors = new List<string>();
    public UnityEvent OnSimulationFinished = new UnityEvent();

    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text resultText;

    [Header("Configuración de Navegación")]
    public string menuSceneName = "Menu";

    private List<PanelButton> _toggleButtons = new List<PanelButton>();
    private bool _isSimulationFinishing = false;

    public float MinIdealVoltage = 200f;
    public float MaxIdealVoltage = 220f;

    public void ReturnToMenu()
    {
        ResetSimulation();

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            SceneManager.LoadScene(0);
        }

        Debug.Log("Regresando al menú principal");
    }


    public void ReturnToMenu(string sceneName)
    {
        ResetSimulation();

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            ReturnToMenu(); 
        }
    }

    public void RegisterToggleButton(PanelButton button)
    {
        if (!_toggleButtons.Contains(button))
        {
            _toggleButtons.Add(button);
        }
    }

    public void UnregisterToggleButton(PanelButton button)
    {
        if (_toggleButtons.Contains(button))
        {
            _toggleButtons.Remove(button);
        }
    }

    public void UpdateCurrentFromToggleButtons()
    {
        float currentToAdd = 0f;
        foreach (var button in _toggleButtons)
        {
            if (button.IsActive())
            {
                currentToAdd += 10f;
            }
        }
        CurrentGeneratorData.Current = currentToAdd;
    }

    public void FinishSimulation()
    {
        if (_isSimulationFinishing) return;
        _isSimulationFinishing = true;

        UpdateCurrentFromToggleButtons();

        bool isSuccessful = CurrentGeneratorData.Voltage >= MinIdealVoltage &&
                          CurrentGeneratorData.Voltage <= MaxIdealVoltage;

        OnSimulationFinished.Invoke();
        _isSimulationFinishing = false;
    }

    public void ResetSimulation()
    {
        CurrentGeneratorData = new GeneratorData();
        ActiveErrors.Clear();
        Time.timeScale = 1f;

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }
    }
}