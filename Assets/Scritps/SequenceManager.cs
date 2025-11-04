using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SequenceManager : MonoBehaviour
{
  

    [System.Serializable]
    public class SimulationStep
    {
        public string stepName;
        public string requiredButtonId;
        public bool isCompleted = false;
        public string errorMessage;
    }

    [Header("Referencias")]
    public SimulationManager simulationManager;


    [Header("Pasos de la Simulación")]
    public List<SimulationStep> startupSequence = new List<SimulationStep>();
    public List<SimulationStep> shutdownSequence = new List<SimulationStep>();

    [Header("Configuración")]
    public bool isStartupComplete = false;
    public bool isShutdownComplete = false;
    private int currentStartupStep = 0;
    private int currentShutdownStep = 0;
    private float startTime;
    private float completionTime;

    [Header("Eventos")]
    public UnityEvent OnStartupComplete = new UnityEvent();
    public UnityEvent OnShutdownComplete = new UnityEvent();
    public UnityEvent OnSequenceError = new UnityEvent();

    [Header("Feedback de Error")]
    public AudioClip errorSound;
    public GameObject errorMessagePanel;
    public TMP_Text errorText;
    public float errorDisplayTime = 2f;

    private AudioSource audioSource;

    void Start()
    {
        simulationManager = FindFirstObjectByType<SimulationManager>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        startTime = Time.time;
        ResetSequence();

        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(false);
    }

    public bool RegisterButtonPress(string buttonId)
    {
        if (!isStartupComplete)
        {
            return CheckStartupSequence(buttonId);
        }
        else if (!isShutdownComplete)
        {
            return CheckShutdownSequence(buttonId);
        }
        return true;
    }

    private bool CheckStartupSequence(string buttonId)
    {
        if (currentStartupStep < startupSequence.Count)
        {
            SimulationStep currentStep = startupSequence[currentStartupStep];

            if (buttonId == currentStep.requiredButtonId)
            {
                currentStep.isCompleted = true;
                currentStartupStep++;

                if (currentStartupStep >= startupSequence.Count)
                {
                    isStartupComplete = true;
                    completionTime = Time.time - startTime;
                    OnStartupComplete.Invoke();

                    simulationManager.FinishSimulation();
                    Debug.Log("Startup sequence completed!");
                }
                return true; 
            }
            else
            {
               
                ShowError(currentStep.errorMessage);
                if (simulationManager != null)
                    simulationManager.ActiveErrors.Add(currentStep.errorMessage);
                OnSequenceError.Invoke();
                return false; 
            }
        }
        return true;
    }

    private bool CheckShutdownSequence(string buttonId)
    {
        if (currentShutdownStep < shutdownSequence.Count)
        {
            SimulationStep currentStep = shutdownSequence[currentShutdownStep];

            if (buttonId == currentStep.requiredButtonId)
            {
                currentStep.isCompleted = true;
                currentShutdownStep++;

                if (currentShutdownStep >= shutdownSequence.Count)
                {
                    isShutdownComplete = true;
                    completionTime = Time.time - startTime;
                    OnShutdownComplete.Invoke();
                    if (simulationManager != null)
                        simulationManager.FinishSimulation();
                }
                return true; 
            }
            else
            {
                
                ShowError(currentStep.errorMessage);
                if (simulationManager != null)
                    simulationManager.ActiveErrors.Add(currentStep.errorMessage);
                OnSequenceError.Invoke();
                return false; 
            }
        }
        return true;
    }

    public void ResetSequence()
    {
        currentStartupStep = 0;
        currentShutdownStep = 0;
        isStartupComplete = false;
        isShutdownComplete = false;
        startTime = Time.time;

        foreach (var step in startupSequence)
        {
            step.isCompleted = false;
        }

        foreach (var step in shutdownSequence)
        {
            step.isCompleted = false;
        }
    }
    private void ShowError(string errorMessage)
    {
        if (errorSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(errorSound);
        }
            

       
        if (errorMessagePanel != null && errorText != null)
        {
            errorText.text = errorMessage;
            errorMessagePanel.SetActive(true);

      
            Invoke("HideError", errorDisplayTime);
        }
    }

    private void HideError()
    {
        if (errorMessagePanel != null)
        {
            errorMessagePanel.SetActive(false);
        }
            
    }
    public void ResetApagadoTime()
    {
        startTime = Time.time; 
    }
    public float GetCompletionTime()
    {
        return completionTime;
    }

    public string GetCurrentStartupStep()
    {
        if (currentStartupStep < startupSequence.Count)
            return startupSequence[currentStartupStep].stepName;
        return "Startup Complete";
    }
    public float GetStartupTime()
    {
        if (isStartupComplete)
            return completionTime;
        else
            return Time.time - startTime;
    }
    public string GetCurrentShutdownStep()
    {
        if (currentShutdownStep < shutdownSequence.Count)
            return shutdownSequence[currentShutdownStep].stepName;
        return "Shutdown Complete";
    }
}