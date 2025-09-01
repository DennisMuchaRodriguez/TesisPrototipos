using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MoveCamera : MonoBehaviour
{
    [Header("Posiciones de la cámara")]
    public Transform[] cameraPositions;

    [Header("Botones de navegación")]
    public Button leftButton;
    public Button rightButton;
    public GameObject Canva;

    [Header("Movimiento Vertical")]
    public float verticalMoveSpeed = 0.1f;
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;

    [Header("Zoom Inicial")]
    public float startSize = 41.0f;
    public float targetSize = 6.29f;
    public float zoomSpeed = 1f;

    [Header("Posiciones Camara")]
    public GameObject StartCamera;
    public GameObject FinishCamera;

    private int currentPositionIndex = 0;
    private Vector3 originalPosition;
    private float currentVerticalOffset = 0f;
    private Camera mainCamera;
    private bool isZooming = true;

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

        originalPosition = cameraPositions[currentPositionIndex].position;
        MoveCameraToPosition(currentPositionIndex);
        UpdateButtons();

      
        if (Canva != null)
            Canva.SetActive(false);
    }

    void Update()
    {
        if (isZooming)
        {
            
            transform.position = Vector3.Lerp(transform.position, FinishCamera.transform.position, Time.deltaTime * zoomSpeed);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);

            if (Vector3.Distance(transform.position, FinishCamera.transform.position) < 0.01f &&
                Mathf.Abs(mainCamera.orthographicSize - targetSize) < 0.01f)
            {
                transform.position = FinishCamera.transform.position;
                mainCamera.orthographicSize = targetSize;
                isZooming = false;

                cameraPositions[0] = FinishCamera.transform;
                if (Canva != null)
                    Canva.SetActive(true);
            }
        }
        else
        {
         
            if (Input.GetKey(upKey))
                MoveVertical(verticalMoveSpeed);
            else if (Input.GetKey(downKey))
                MoveVertical(-verticalMoveSpeed);
        }
    }

    public void MoveLeft()
    {
        if (currentPositionIndex > 0)
        {
            currentPositionIndex--;
            MoveCameraToPosition(currentPositionIndex);
            UpdateButtons();
        }
    }

    public void MoveRight()
    {
        if (currentPositionIndex < cameraPositions.Length - 1)
        {
            currentPositionIndex++;
            MoveCameraToPosition(currentPositionIndex);
            UpdateButtons();
        }
    }

    private void MoveVertical(float amount)
    {
        float newOffset = Mathf.Clamp(currentVerticalOffset + amount, -53.8f, -1.02f);

        if (newOffset != currentVerticalOffset)
        {
            currentVerticalOffset = newOffset;
            UpdateCameraPosition();
        }
    }

    private void MoveCameraToPosition(int index)
    {
        if (index >= 0 && index < cameraPositions.Length)
        {
            currentPositionIndex = index;
            originalPosition = cameraPositions[index].position;
            currentVerticalOffset = 0f;
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition()
    {
        transform.position = originalPosition + Vector3.up * currentVerticalOffset;
        transform.rotation = cameraPositions[currentPositionIndex].rotation;
    }

    private void UpdateButtons()
    {
        leftButton.interactable = currentPositionIndex > 0;
        rightButton.interactable = currentPositionIndex < cameraPositions.Length - 1;
    }

}
