using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
public class MoveCamera : MonoBehaviour
{
    [Header("Camera Positions")]
    public Transform[] cameraPositions;

    [Header("Navigation Buttons")]
    public Button leftButton;
    public Button rightButton;

    [Header("Movement Settings")]
    [Range(0.1f, 5f)] public float smoothTime = 1f; // Controla la suavidad del movimiento

    [Header("Cinemachine Virtual Camera")]
    public CinemachineCamera virtualCamera;

    private int currentPositionIndex = 0;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Assign a Cinemachine Virtual Camera in the inspector!");
            enabled = false;
            return;
        }

        if (cameraPositions == null || cameraPositions.Length == 0)
        {
            Debug.LogError("No camera positions defined!");
            enabled = false;
            return;
        }

        // Posicionar inmediatamente al inicio
        virtualCamera.transform.position = cameraPositions[currentPositionIndex].position;
        virtualCamera.transform.rotation = cameraPositions[currentPositionIndex].rotation;
        UpdateButtons();
    }

    void Update()
    {
        // Movimiento suave en Update
        if (virtualCamera != null && cameraPositions.Length > currentPositionIndex)
        {
            // Suavizado de posición
            virtualCamera.transform.position = Vector3.SmoothDamp(
                virtualCamera.transform.position,
                cameraPositions[currentPositionIndex].position,
                ref velocity,
                smoothTime);

            // Suavizado de rotación
            virtualCamera.transform.rotation = Quaternion.Slerp(
                virtualCamera.transform.rotation,
                cameraPositions[currentPositionIndex].rotation,
                smoothTime * Time.deltaTime);
        }
    }

    public void MoveLeft()
    {
        if (currentPositionIndex > 0)
        {
            currentPositionIndex--;
            UpdateButtons();
        }
    }

    public void MoveRight()
    {
        if (currentPositionIndex < cameraPositions.Length - 1)
        {
            currentPositionIndex++;
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        leftButton.interactable = currentPositionIndex > 0;
        rightButton.interactable = currentPositionIndex < cameraPositions.Length - 1;
    }
}
