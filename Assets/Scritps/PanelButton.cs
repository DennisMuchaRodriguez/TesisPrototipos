using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button), typeof(Image))]
public class PanelButton : MonoBehaviour, IPointerDownHandler
{
    public enum ButtonType
    {
        RotateAndAdd,
        ToggleAndAdd,
        FinishSimulation,
        SequenceStep // Nuevo tipo para pasos de secuencia
    }

    [Header("Configuración")]
    public ButtonType buttonType;
    public string buttonId; // ID único para identificar el botón en la secuencia

    [Header("Rotación Configurable")]
    public float rotationAmount = 45f;
    public int maxRotations = 2;
    public float rotationToggle = 90f;

    [Header("Sonidos")]
    public AudioClip rotateSound;
    public AudioClip toggleSound;
    public AudioClip finishSound;

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
        }

       
        if (!string.IsNullOrEmpty(buttonId) && sequenceManager != null)
        {
            sequenceManager.RegisterButtonPress(buttonId);
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
            simulationManager.CurrentGeneratorData.Voltage += 1;
    }

    private void HandleToggleAndAdd()
    {
        _isActive = !_isActive;
        transform.Rotate(0f, 0f, rotationToggle);
        _image.color = _isActive ? Color.white : Color.gray;

        if (simulationManager != null)
            simulationManager.UpdateCurrentFromToggleButtons();
    }

    public bool IsActive() => _isActive;

    public void ResetRotationState()
    {
        _currentRotationCount = 0;
        _reverseRotation = false;
        transform.rotation = Quaternion.identity;
    }
}