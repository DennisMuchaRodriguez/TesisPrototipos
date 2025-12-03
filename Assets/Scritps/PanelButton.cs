using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

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

    [Header("Rotación de Objetos Controlados")]
    public bool controlaObjetos = false;

    [System.Serializable]
    public class ObjetoConfigurado
    {
        public GameObject objeto;
        public List<float> angulosRotacion = new List<float> { 0f, 90f, 180f };
    }

    public List<ObjetoConfigurado> objetosConfigurados = new List<ObjetoConfigurado>();

    [Header("Configuración de Animación")]
    [Tooltip("Duración de la animación de rotación en segundos")]
    public float duracionAnimacion = 0.5f;
    [Tooltip("Tipo de ease para la animación")]
    public Ease tipoEase = Ease.OutBack;

    [Header("Configuración Especial")]
    [Tooltip("Si está activado, solo funcionará la primera vez que se haga click")]
    public bool soloPrimeraVez = false;

    [Header("Activar Objetos Ocultos")]
    [Tooltip("Lista de objetos que se activarán/desactivarán al hacer click")]
    public List<GameObject> objetosOcultosAActivar;

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

    // Control de estados para objetos - ahora por cada objeto
    private Dictionary<GameObject, int> _estadosActuales = new Dictionary<GameObject, int>();
    private bool _estaAnimando = false;
    private bool _yaSeUso = false;

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

        // Inicializar estados para cada objeto
        foreach (var objConfig in objetosConfigurados)
        {
            if (objConfig.objeto != null)
            {
                _estadosActuales[objConfig.objeto] = 0;
                AplicarRotacionInmediata(objConfig.objeto, 0);
            }
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
        if (_estaAnimando) return;
        if (soloPrimeraVez && _yaSeUso) return;

        bool fueCorrecto = true;
        if (!string.IsNullOrEmpty(buttonId) && sequenceManager != null)
        {
            fueCorrecto = sequenceManager.RegisterButtonPress(buttonId);
        }

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

        if (controlaObjetos)
        {
            CambiarEstadoObjetos();
        }

        ActivarObjetosOcultos();

        if (soloPrimeraVez)
        {
            _yaSeUso = true;
            _image.color = Color.gray;
        }
    }

    private void ActivarObjetosOcultos()
    {
        if (objetosOcultosAActivar.Count == 0) return;

        foreach (GameObject objeto in objetosOcultosAActivar)
        {
            if (objeto != null)
            {
                objeto.SetActive(true);
            }
        }
    }

    private void CambiarEstadoObjetos()
    {
        if (!controlaObjetos || objetosConfigurados.Count == 0)
            return;

        _estaAnimando = true;
        int animacionesCompletadas = 0;
        int totalAnimaciones = objetosConfigurados.Count;

        foreach (var objConfig in objetosConfigurados)
        {
            if (objConfig.objeto != null && objConfig.angulosRotacion.Count > 0)
            {
                // Avanzar al siguiente estado para este objeto específico
                int estadoActual = _estadosActuales[objConfig.objeto];
                int nuevoEstado = (estadoActual + 1) % objConfig.angulosRotacion.Count;
                _estadosActuales[objConfig.objeto] = nuevoEstado;

                float angulo = objConfig.angulosRotacion[nuevoEstado];

                // Aplicar rotación animada
                objConfig.objeto.transform.DORotate(new Vector3(0f, 0f, angulo), duracionAnimacion)
                    .SetEase(tipoEase)
                    .OnComplete(() => {
                        animacionesCompletadas++;
                        if (animacionesCompletadas >= totalAnimaciones)
                        {
                            _estaAnimando = false;
                        }
                    });

                // Rotar aguja si existe
                Transform aguja = objConfig.objeto.transform.Find("Aguja");
                if (aguja != null)
                {
                    aguja.DOLocalRotate(new Vector3(0f, 0f, -angulo), duracionAnimacion)
                        .SetEase(tipoEase);
                }

                Debug.Log($"Objeto {objConfig.objeto.name} - Estado {nuevoEstado + 1} (Ángulo: {angulo}°)");
            }
            else
            {
                animacionesCompletadas++;
            }
        }

        if (totalAnimaciones == 0)
        {
            _estaAnimando = false;
        }
    }

    private void AplicarRotacionInmediata(GameObject objeto, int estadoIndex)
    {
        // Encontrar la configuración para este objeto
        foreach (var objConfig in objetosConfigurados)
        {
            if (objConfig.objeto == objeto && estadoIndex < objConfig.angulosRotacion.Count)
            {
                float angulo = objConfig.angulosRotacion[estadoIndex];
                objeto.transform.rotation = Quaternion.Euler(0f, 0f, angulo);

                Transform aguja = objeto.transform.Find("Aguja");
                if (aguja != null)
                {
                    aguja.localRotation = Quaternion.Euler(0f, 0f, -angulo);
                }
                break;
            }
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
        _estaAnimando = false;

        // Resetear estados de todos los objetos
        foreach (var objConfig in objetosConfigurados)
        {
            if (objConfig.objeto != null)
            {
                _estadosActuales[objConfig.objeto] = 0;
                AplicarRotacionInmediata(objConfig.objeto, 0);
            }
        }

        _yaSeUso = false;
        _image.color = Color.white;
    }

    public bool EstaAnimando()
    {
        return _estaAnimando;
    }

    public void ResetearUsoUnico()
    {
        _yaSeUso = false;
        _image.color = Color.white;
    }
}