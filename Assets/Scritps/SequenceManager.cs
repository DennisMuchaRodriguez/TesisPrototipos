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
        public List<string> validButtonIds;
        public bool isCompleted = false;
        public string errorMessage;
        public UnityEvent OnStepCompleted;
    }

    [Header("Referencias")]
    public SimulationManager simulationManager;

    [Header("Pasos de la Simulación")]
    public List<SimulationStep> startupSequence = new List<SimulationStep>();
    public List<SimulationStep> intermediateSequence = new List<SimulationStep>(); // NUEVO: Proceso intermedio
    public List<SimulationStep> shutdownSequence = new List<SimulationStep>();

    [Header("Configuración")]
    public bool isStartupComplete = false;
    public bool isIntermediateComplete = false; // NUEVO
    public bool isShutdownComplete = false;
    private int currentStartupStep = 0;
    private int currentIntermediateStep = 0; // NUEVO
    private int currentShutdownStep = 0;
    private float startTime;
    private float intermediateStartTime; // NUEVO
    private float completionTime;

    [Header("Eventos")]
    public UnityEvent OnStartupComplete = new UnityEvent();
    public UnityEvent OnIntermediateComplete = new UnityEvent(); // NUEVO
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
        else if (!isIntermediateComplete) // NUEVO: Verificar proceso intermedio
        {
            return CheckIntermediateSequence(buttonId);
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

            if (currentStep.validButtonIds.Contains(buttonId))
            {
                currentStep.isCompleted = true;
                currentStep.OnStepCompleted?.Invoke();
                currentStartupStep++;

                if (currentStartupStep >= startupSequence.Count)
                {
                    isStartupComplete = true;
                    completionTime = Time.time - startTime;
                    intermediateStartTime = Time.time; // NUEVO: Iniciar tiempo del proceso intermedio
                    OnStartupComplete.Invoke();
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

    // NUEVO: Proceso intermedio
    private bool CheckIntermediateSequence(string buttonId)
    {
        if (currentIntermediateStep < intermediateSequence.Count)
        {
            SimulationStep currentStep = intermediateSequence[currentIntermediateStep];

            if (currentStep.validButtonIds.Contains(buttonId))
            {
                currentStep.isCompleted = true;
                currentStep.OnStepCompleted?.Invoke();
                currentIntermediateStep++;

                if (currentIntermediateStep >= intermediateSequence.Count)
                {
                    isIntermediateComplete = true;
                    completionTime = Time.time - intermediateStartTime;
                    OnIntermediateComplete.Invoke();
                    Debug.Log("Intermediate sequence completed!");
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

            if (currentStep.validButtonIds.Contains(buttonId))
            {
                currentStep.isCompleted = true;
                currentStep.OnStepCompleted?.Invoke();
                currentShutdownStep++;

                if (currentShutdownStep >= shutdownSequence.Count)
                {
                    isShutdownComplete = true;
                    completionTime = Time.time - startTime;
                    OnShutdownComplete.Invoke();
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
        currentIntermediateStep = 0; // NUEVO
        currentShutdownStep = 0;
        isStartupComplete = false;
        isIntermediateComplete = false; // NUEVO
        isShutdownComplete = false;
        startTime = Time.time;

        foreach (var step in startupSequence)
        {
            step.isCompleted = false;
        }

        foreach (var step in intermediateSequence) // NUEVO
        {
            step.isCompleted = false;
        }

        foreach (var step in shutdownSequence)
        {
            step.isCompleted = false;
        }
    }

    // NUEVO: Método para iniciar el proceso intermedio
    public void StartIntermediateProcess()
    {
        isIntermediateComplete = false;
        currentIntermediateStep = 0;
        intermediateStartTime = Time.time;
        Debug.Log("Iniciando proceso intermedio...");
    }

    // NUEVO: Método para iniciar el proceso de apagado
    public void StartShutdownProcess()
    {
        isShutdownComplete = false;
        currentShutdownStep = 0;
        startTime = Time.time;
        Debug.Log("Iniciando proceso de apagado...");
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

    public void ShowTemporaryWarning(string warningMessage, float displayTime)
    {
        ShowError(warningMessage);
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

    // NUEVO: Obtener tiempo del proceso intermedio
    public float GetIntermediateTime()
    {
        if (isIntermediateComplete)
            return completionTime;
        else
            return Time.time - intermediateStartTime;
    }

    public string GetCurrentStartupStep()
    {
        if (currentStartupStep < startupSequence.Count)
            return startupSequence[currentStartupStep].stepName;
        return "Startup Complete";
    }

    public string GetCurrentIntermediateStep() // NUEVO
    {
        if (currentIntermediateStep < intermediateSequence.Count)
            return intermediateSequence[currentIntermediateStep].stepName;
        return "Intermediate Complete";
    }

    public string GetCurrentShutdownStep()
    {
        if (currentShutdownStep < shutdownSequence.Count)
            return shutdownSequence[currentShutdownStep].stepName;
        return "Shutdown Complete";
    }

    public float GetStartupTime()
    {
        if (isStartupComplete)
            return completionTime;
        else
            return Time.time - startTime;
    }

    public SimulationStep GetStepByName(string stepName)
    {
        foreach (var step in startupSequence)
        {
            if (step.stepName == stepName) return step;
        }
        foreach (var step in intermediateSequence) // NUEVO
        {
            if (step.stepName == stepName) return step;
        }
        foreach (var step in shutdownSequence)
        {
            if (step.stepName == stepName) return step;
        }
        return null;
    }
}