using UnityEngine;
using UnityEngine.UI;

public class MoveCamera : MonoBehaviour
{
    [Header("Posiciones de la cámara")]
    public Transform[] cameraPositions;

    [Header("Botones de navegación")]
    public Button leftButton;
    public Button rightButton;
    public GameObject Canva;

    [Header("Movimiento Vertical")]
    public float verticalMoveSpeed = 2f;
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;

    [Header("Zoom Inicial")]
    public float startSize = 41.0f;
    public float targetSize = 6.29f;
    public float zoomSpeed = 3f;

    [Header("Posiciones Camara")]
    public GameObject StartCamera;
    public GameObject FinishCamera;

    private int currentPositionIndex = 0;
    private Vector3 originalPosition;
    private float currentVerticalOffset = 0f;
    private Camera mainCamera;
    private bool isZooming = true;
    private bool canMoveVertically = false;

    void Start()
    {
        mainCamera = GetComponent<Camera>();

        if (mainCamera == null)
        {
            Debug.LogError("Este script debe ir en un objeto con un componente Camera");
            return;
        }

        if (cameraPositions == null || cameraPositions.Length == 0)
        {
            Debug.LogError("No hay posiciones de cámara definidas!");
            return;
        }

        // Inicializar en posición de inicio
        transform.position = StartCamera.transform.position;
        mainCamera.orthographicSize = startSize;

        // Configurar posición actual
        originalPosition = FinishCamera.transform.position;
        cameraPositions[0] = FinishCamera.transform;

        if (Canva != null)
            Canva.SetActive(false);

        UpdateButtons();
    }

    void Update()
    {
        HandleZoom();
        HandleVerticalMovement();
    }

    private void HandleZoom()
    {
        if (isZooming)
        {
            // Aplicar zoom suavemente
            transform.position = Vector3.Lerp(transform.position, FinishCamera.transform.position, Time.deltaTime * zoomSpeed);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);

            // Verificar si el zoom ha terminado
            float distanceThreshold = 0.05f;
            float sizeThreshold = 0.05f;

            if (Vector3.Distance(transform.position, FinishCamera.transform.position) < distanceThreshold &&
                Mathf.Abs(mainCamera.orthographicSize - targetSize) < sizeThreshold)
            {
                // Finalizar zoom
                transform.position = FinishCamera.transform.position;
                mainCamera.orthographicSize = targetSize;
                isZooming = false;
                canMoveVertically = true;

                // Activar canvas
                if (Canva != null)
                    Canva.SetActive(true);

                // Actualizar posición actual
                originalPosition = FinishCamera.transform.position;
                UpdateButtons();
            }
        }
    }

    private void HandleVerticalMovement()
    {
        if (!canMoveVertically || isZooming) return;

        float verticalInput = 0f;

        if (Input.GetKey(upKey))
            verticalInput = 1f;
        else if (Input.GetKey(downKey))
            verticalInput = -1f;

        if (verticalInput != 0f)
        {
            MoveVertical(verticalInput * verticalMoveSpeed * Time.deltaTime);
        }
    }

    public void MoveLeft()
    {
        if (isZooming) return;

        if (currentPositionIndex > 0)
        {
            currentPositionIndex--;
            MoveCameraToPosition(currentPositionIndex);
            UpdateButtons();
        }
    }

    public void MoveRight()
    {
        if (isZooming) return;

        if (currentPositionIndex < cameraPositions.Length - 1)
        {
            currentPositionIndex++;
            MoveCameraToPosition(currentPositionIndex);
            UpdateButtons();
        }
    }

    private void MoveVertical(float amount)
    {
        if (!canMoveVertically || isZooming) return;

        float newOffset = Mathf.Clamp(currentVerticalOffset + amount, -53.8f, -1.02f);

        if (newOffset != currentVerticalOffset)
        {
            currentVerticalOffset = newOffset;
            UpdateCameraPosition();
        }
    }

    private void MoveCameraToPosition(int index)
    {
        if (index >= 0 && index < cameraPositions.Length && cameraPositions[index] != null)
        {
            currentPositionIndex = index;
            originalPosition = cameraPositions[index].position;
            currentVerticalOffset = 0f; // Resetear offset vertical al cambiar de posición
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        if (cameraPositions[currentPositionIndex] != null)
        {
            Vector3 targetPosition = originalPosition + Vector3.up * currentVerticalOffset;
            transform.position = targetPosition;
            transform.rotation = cameraPositions[currentPositionIndex].rotation;
        }
    }

    private void UpdateButtons()
    {
        if (leftButton != null)
            leftButton.interactable = (currentPositionIndex > 0) && !isZooming;

        if (rightButton != null)
            rightButton.interactable = (currentPositionIndex < cameraPositions.Length - 1) && !isZooming;
    }

    // Método público para forzar el fin del zoom (por si necesitas debug)
    public void SkipZoom()
    {
        if (isZooming)
        {
            transform.position = FinishCamera.transform.position;
            mainCamera.orthographicSize = targetSize;
            isZooming = false;
            canMoveVertically = true;

            if (Canva != null)
                Canva.SetActive(true);

            UpdateButtons();
        }
    }
}