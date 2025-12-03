using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;

public class StepActionExecutor : MonoBehaviour
{
    [System.Serializable]
    public class ObjetoConfigurado
    {
        public GameObject objeto;
        public List<float> angulosRotacion = new List<float> { 0f, 90f, 180f };
    }

    [Header("Configuración del Paso")]
    public SequenceManager sequenceManager;
    public string nombrePasoRequerido; // Nombre del paso que debe completarse

    [Header("Rotación de Objetos")]
    public List<ObjetoConfigurado> objetosConfigurados = new List<ObjetoConfigurado>();

    [Header("Configuración de Animación")]
    public float duracionAnimacion = 0.5f;
    public Ease tipoEase = Ease.OutBack;

    [Header("Objetos Ocultos a Activar")]
    public List<GameObject> objetosOcultosAActivar;

    [Header("Eventos")]
    public UnityEvent OnActionsExecuted;

    private Dictionary<GameObject, int> _estadosActuales = new Dictionary<GameObject, int>();
    private bool _accionesEjecutadas = false;
    private bool _estaAnimando = false;

    void Start()
    {
        // Inicializar estados para cada objeto
        foreach (var objConfig in objetosConfigurados)
        {
            if (objConfig.objeto != null)
            {
                _estadosActuales[objConfig.objeto] = 0;
            }
        }

        // Suscribirse al evento del paso requerido
        if (sequenceManager != null && !string.IsNullOrEmpty(nombrePasoRequerido))
        {
            var step = sequenceManager.GetStepByName(nombrePasoRequerido);
            if (step != null)
            {
                step.OnStepCompleted.AddListener(EjecutarAcciones);
            }
            else
            {
                Debug.LogError($"No se encontró el paso: {nombrePasoRequerido}");
            }
        }
    }

    private void EjecutarAcciones()
    {
        // Solo ejecutar una vez
        if (_accionesEjecutadas || _estaAnimando) return;

        _accionesEjecutadas = true;
        _estaAnimando = true;

        Debug.Log($"Ejecutando acciones para el paso: {nombrePasoRequerido}");

        // Activar objetos ocultos
        foreach (GameObject objeto in objetosOcultosAActivar)
        {
            if (objeto != null)
            {
                objeto.SetActive(true);
            }
        }

        // Rotar objetos configurados
        int animacionesCompletadas = 0;
        int totalAnimaciones = objetosConfigurados.Count;

        foreach (var objConfig in objetosConfigurados)
        {
            if (objConfig.objeto != null && objConfig.angulosRotacion.Count > 0)
            {
                // Avanzar al siguiente estado para este objeto
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
                            OnActionsExecuted?.Invoke();
                        }
                    });

                // Rotar aguja si existe
                Transform aguja = objConfig.objeto.transform.Find("Aguja");
                if (aguja != null)
                {
                    aguja.DOLocalRotate(new Vector3(0f, 0f, -angulo), duracionAnimacion)
                        .SetEase(tipoEase);
                }

                Debug.Log($"Objeto {objConfig.objeto.name} rotado a {angulo}°");
            }
            else
            {
                animacionesCompletadas++;
            }
        }

        if (totalAnimaciones == 0)
        {
            _estaAnimando = false;
            OnActionsExecuted?.Invoke();
        }
    }

    // Método para resetear (útil cuando se reinicia la simulación)
    public void ResetActions()
    {
        _accionesEjecutadas = false;
        _estaAnimando = false;

        // Resetear estados de objetos
        foreach (var objConfig in objetosConfigurados)
        {
            if (objConfig.objeto != null)
            {
                _estadosActuales[objConfig.objeto] = 0;
                objConfig.objeto.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                Transform aguja = objConfig.objeto.transform.Find("Aguja");
                if (aguja != null)
                {
                    aguja.localRotation = Quaternion.Euler(0f, 0f, 0f);
                }
            }
        }

        // Desactivar objetos ocultos
        foreach (GameObject objeto in objetosOcultosAActivar)
        {
            if (objeto != null)
            {
                objeto.SetActive(false);
            }
        }
    }

    // Método para forzar la ejecución (útil para debugging)
    public void ForceExecute()
    {
        EjecutarAcciones();
    }
}