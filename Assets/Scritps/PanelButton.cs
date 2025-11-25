using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
[RequireComponent(typeof(Button), typeof(Image))]
public class PanelButton : MonoBehaviour, IPointerDownHandler
{
    public enum ButtonType
    {
        RotateAndAdd,
        ToggleAndAdd,
        FinishSimulation,
        SequenceStep,
        WarningOnly
    }
    [Header("Configuración para WarningOnly")]
    public string warningMessage = "¡Advertencia!";
    public float warningDisplayTime = 3f;
    [Header("Configuración")]
    public ButtonType buttonType;
    public string buttonId; 

    [Header("Rotación Configurable")]
    public float rotationAmount = 45f;
    public int maxRotations = 2;
    public float rotationToggle = 90f;

    [Header("Sonidos")]
    public AudioClip rotateSound;
    public AudioClip toggleSound;
    public AudioClip finishSound;

    [Header("Configuración de Medidores")]
    public bool controlaMedidores = false;
    public List<GameObject> medidoresAControlar; // Arrastra los medidores aquí
    public float valorMinimo = 0f;
    public float valorMaximo = 100f;
    public float incrementoPorClick = 10f;

    private bool _isActive = true;
    private Image _image;
    private int _currentRotationCount = 0;
    private bool _reverseRotation = false;
    private AudioSource _audioSource;
    private SequenceManager sequenceManager;
    private SimulationManager simulationManager;
    void Start()
    {
        _image = GetComponent<Image>();

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

     
        sequenceManager = FindFirstObjectByType<SequenceManager>();
        simulationManager = FindFirstObjectByType<SimulationManager>();
        if (buttonType == ButtonType.ToggleAndAdd)
        {
            simulationManager.RegisterToggleButton(this);
        }
    }

    void OnDestroy()
    {
        if (buttonType == ButtonType.ToggleAndAdd && simulationManager != null)
        {
            simulationManager.UnregisterToggleButton(this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        bool fueCorrecto = true;
        if (!string.IsNullOrEmpty(buttonId) && sequenceManager != null)
        {
            fueCorrecto = sequenceManager.RegisterButtonPress(buttonId);
        }

        // Solo rotar si fue correcto o si no es un botón de secuencia
        if (fueCorrecto || string.IsNullOrEmpty(buttonId))
        {
            switch (buttonType)
            {
                case ButtonType.RotateAndAdd:
                    PlaySound(rotateSound);
                    HandleRotateAndAdd();
                    break;

                case ButtonType.ToggleAndAdd:
                    PlaySound(toggleSound);
                    HandleToggleAndAdd();
                    break;

                case ButtonType.FinishSimulation:
                    PlaySound(finishSound);
                    if (simulationManager != null)
                        simulationManager.FinishSimulation();
                    break;

                case ButtonType.SequenceStep:
                    PlaySound(rotateSound);
                    HandleSequenceStep();
                    break;

               
                case ButtonType.WarningOnly:
                    PlaySound(rotateSound); 
                    ShowWarningMessage();
                    break;
            }
        }

        if (controlaMedidores)
        {
            HandleMedidores();
        }
    }

    private void HandleSequenceStep()
    {
        float direction = _reverseRotation ? -1f : 1f;
        transform.Rotate(0f, 0f, rotationAmount * direction);
        _currentRotationCount++;

        if (_currentRotationCount >= maxRotations)
        {
            _reverseRotation = !_reverseRotation;
            _currentRotationCount = 0;
        }

    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
    private void ShowWarningMessage()
    {
        if (sequenceManager != null)
        {
           
            sequenceManager.ShowTemporaryWarning(warningMessage, warningDisplayTime);
        }


    }
    private void HandleRotateAndAdd()
    {
        float direction = _reverseRotation ? -1f : 1f;
        transform.Rotate(0f, 0f, rotationAmount * direction);
        _currentRotationCount++;

        if (_currentRotationCount >= maxRotations)
        {
            _reverseRotation = !_reverseRotation;
            _currentRotationCount = 0;
        }


        if (simulationManager != null)
        {
            simulationManager.CurrentGeneratorData.Voltage += 1;
        }
            
    }

    private void HandleToggleAndAdd()
    {
        _isActive = !_isActive;
        transform.Rotate(0f, 0f, rotationToggle);
        _image.color = _isActive ? Color.white : Color.gray;

        if (simulationManager != null)
        {
            simulationManager.UpdateCurrentFromToggleButtons();
        }
           
    }

    public bool IsActive() => _isActive;

    public void ResetRotationState()
    {
        _currentRotationCount = 0;
        _reverseRotation = false;
        transform.rotation = Quaternion.identity;
    }

    private void HandleMedidores()
    {
        if (!controlaMedidores || medidoresAControlar.Count == 0) return;

        foreach (GameObject medidor in medidoresAControlar)
        {
            // Aquí implementas la rotación del medidor según tu sistema
            RotateMeter(medidor, incrementoPorClick);
        }
    }

    private void RotateMeter(GameObject medidor, float incremento)
    {
        // Ejemplo básico - ajusta según cómo tengas implementados tus medidores
        Transform needle = medidor.transform.Find("Aguja"); // o el nombre que uses
        if (needle != null)
        {
            // Calcula rotación basada en el rango
            float porcentaje = Mathf.Clamp01((incremento - valorMinimo) / (valorMaximo - valorMinimo));
            float rotacion = Mathf.Lerp(0f, 270f, porcentaje); // Ejemplo: 0° a 270°

            needle.localRotation = Quaternion.Euler(0f, 0f, -rotacion);
        }
    }
}