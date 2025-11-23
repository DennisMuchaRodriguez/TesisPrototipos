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

    [Header("Límites Verticales")]
    public float limitArriba = 2.9f;
    public float limitBajo = -55.2f;

    private int currentPositionIndex = 1; 
    private Vector3 originalPosition;
    private float currentVerticalOffset = 0f;
    private Camera mainCamera;
    private bool isZooming = true;
    private bool canMoveVertically = false;

    private float currentMaxY;
    private float currentMinY;

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

        
        transform.position = StartCamera.transform.position;
        mainCamera.orthographicSize = startSize;


        originalPosition = FinishCamera.transform.position;

        UpdateLimitsForCurrentPosition();

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
            transform.position = Vector3.Lerp(transform.position, cameraPositions[1].position, Time.deltaTime * zoomSpeed);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);

            float distanceThreshold = 0.05f;
            float sizeThreshold = 0.05f;

            if (Vector3.Distance(transform.position, cameraPositions[1].position) < distanceThreshold &&
                Mathf.Abs(mainCamera.orthographicSize - targetSize) < sizeThreshold)
            {
             
                transform.position = cameraPositions[1].position;
                mainCamera.orthographicSize = targetSize;
                isZooming = false;
                canMoveVertically = true;

          
                if (Canva != null)
                    Canva.SetActive(true);

              
                originalPosition = cameraPositions[1].position;
                currentPositionIndex = 1; 
                UpdateLimitsForCurrentPosition();
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

        float newOffset = Mathf.Clamp(currentVerticalOffset + amount, currentMinY, currentMaxY);

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
            currentVerticalOffset = 0f;
            UpdateLimitsForCurrentPosition();
            UpdateCameraPosition();
        }
    }

    private void UpdateLimitsForCurrentPosition()
    {
        currentMaxY = limitArriba;
        currentMinY = limitBajo;
    }

    private void UpdateCameraPosition()
    {
        if (cameraPositions[currentPositionIndex] != null)
        {
            Vector3 targetPosition = originalPosition + Vector3.up * currentVerticalOffset;
            transform.position = targetPosition;
            transform.rotation = cameraPositions[currentPositionIndex].rotation;

          
            mainCamera.orthographicSize = targetSize;
        }
    }

    private void UpdateButtons()
    {
        if (leftButton != null)
            leftButton.interactable = (currentPositionIndex > 0) && !isZooming;

        if (rightButton != null)
            rightButton.interactable = (currentPositionIndex < cameraPositions.Length - 1) && !isZooming;
    }

    public void SkipZoom()
    {
        if (isZooming)
        {
            transform.position = cameraPositions[1].position;
            mainCamera.orthographicSize = targetSize;
            isZooming = false;
            canMoveVertically = true;
            currentPositionIndex = 1; 

            if (Canva != null)
                Canva.SetActive(true);

            UpdateLimitsForCurrentPosition();
            UpdateButtons();
        }
    }
}