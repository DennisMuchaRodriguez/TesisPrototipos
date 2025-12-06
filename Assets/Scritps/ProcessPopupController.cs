using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class ProcessPopupController : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject popupPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Button continueButton;
    public CanvasGroup blocker;

    [Header("Configuración Delays")]
    public float startupDelay = 2f;
    public float intermediateDelay = 1.5f;
    public float shutdownDelay = 1f;

    [Header("Mensajes por Proceso")]
    public string startupTitle = "PROCESO DE ENCENDIDO";
    [TextArea(2, 4)]
    public string startupDesc = "Prepara el sistema para su operación inicial...";

    public string intermediateTitle = "CAMBIO DE LÍNEA";
    [TextArea(2, 4)]
    public string intermediateDesc = "Procedimiento para cambiar la configuración...";

    public string shutdownTitle = "PROCESO DE APAGADO";
    [TextArea(2, 4)]
    public string shutdownDesc = "Apagando el sistema de manera segura...";

    [Header("Animación")]
    public float animationDuration = 0.5f;
    public Ease easeType = Ease.OutBack;

    private bool isPopupActive = false;
    private SequenceManager sequenceManager;

    void Start()
    {
        // Ocultar panel al inicio
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            popupPanel.transform.localScale = Vector3.zero;
        }

        // Configurar bloqueador
        if (blocker != null)
        {
            blocker.gameObject.SetActive(false);
            blocker.alpha = 0;
            blocker.blocksRaycasts = false;
        }

        // Configurar botón
        if (continueButton != null)
            continueButton.onClick.AddListener(ClosePopup);

        // Buscar sequence manager
        sequenceManager = FindObjectOfType<SequenceManager>();

        // Mostrar popup de encendido con delay
        StartCoroutine(ShowPopupWithDelay(startupTitle, startupDesc, startupDelay));
    }

    IEnumerator ShowPopupWithDelay(string title, string description, float delay)
    {
        // Asegurarse de que el tiempo esté normal antes del delay
        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(delay); // Usar tiempo real, no afectado por Time.timeScale
        ShowPopup(title, description);
    }

    void ShowPopup(string title, string description)
    {
        if (isPopupActive) return;

        Debug.Log($"Mostrando popup: {title}");

        // Asegurarse de que el tiempo esté normal
        Time.timeScale = 1f;

        // Actualizar textos
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        // Activar bloqueador
        if (blocker != null)
        {
            blocker.gameObject.SetActive(true);
            blocker.DOFade(1, 0.3f).SetUpdate(true);
            blocker.blocksRaycasts = true;
        }

        // Mostrar panel con animación (igual que PopupController)
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            popupPanel.transform.localScale = Vector3.zero;
            popupPanel.transform.DOScale(Vector3.one, animationDuration)
                .SetEase(easeType)
                .SetUpdate(true) // Ignora Time.timeScale
                .OnComplete(() => {
                    isPopupActive = true;
                });
        }

        // Pausar el tiempo SOLO cuando el popup está completamente mostrado
        StartCoroutine(PauseTimeAfterAnimation());
    }

    IEnumerator PauseTimeAfterAnimation()
    {
        yield return new WaitForSecondsRealtime(animationDuration);
        Time.timeScale = 0f;
    }

    void ClosePopup()
    {
        if (!isPopupActive) return;

        Debug.Log("Cerrando popup");

        // Restaurar tiempo inmediatamente
        Time.timeScale = 1f;

        // Animación de salida (igual que PopupController)
        if (popupPanel != null)
        {
            popupPanel.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => {
                    popupPanel.SetActive(false);
                    isPopupActive = false;

                    // Desactivar bloqueador
                    if (blocker != null)
                    {
                        blocker.blocksRaycasts = false;
                        blocker.DOFade(0, 0.3f)
                            .OnComplete(() => blocker.gameObject.SetActive(false));
                    }
                });
        }
    }

    // Métodos públicos optimizados - VERSIÓN CORREGIDA
    public void ShowStartupPopup()
    {
        if (gameObject.activeInHierarchy && !isPopupActive)
            StartCoroutine(ShowPopupWithDelay(startupTitle, startupDesc, startupDelay));
    }

    public void ShowIntermediatePopup()
    {
        if (gameObject.activeInHierarchy && !isPopupActive)
            StartCoroutine(ShowPopupWithDelay(intermediateTitle, intermediateDesc, intermediateDelay));
    }

    public void ShowShutdownPopup()
    {
        // Asegurar que cualquier otro popup esté cerrado primero
        ForceClosePopup();

        if (gameObject.activeInHierarchy)
            StartCoroutine(ShowPopupWithDelay(shutdownTitle, shutdownDesc, shutdownDelay));
    }

    // Método para forzar cierre si es necesario
    public void ForceClosePopup()
    {
        if (isPopupActive)
        {
            Time.timeScale = 1f;
            isPopupActive = false;
        }

        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (blocker != null)
        {
            blocker.blocksRaycasts = false;
            blocker.gameObject.SetActive(false);
        }
    }

    // Método para verificar si hay popup activo
    public bool IsPopupActive()
    {
        return isPopupActive;
    }
}