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

    private int currentPositionIndex = 0;

    void Start()
    {
        
        if (cameraPositions == null || cameraPositions.Length == 0)
        {
            Debug.LogError("No hay posiciones de cámara definidas!");
            return;
        }

    
        MoveCameraToPosition(currentPositionIndex);

      
        UpdateButtons();
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

    private void MoveCameraToPosition(int index)
    {
        if (index >= 0 && index < cameraPositions.Length)
        {
            transform.position = cameraPositions[index].position;
            transform.rotation = cameraPositions[index].rotation;
        }
    }

    private void UpdateButtons()
    {

        leftButton.interactable = currentPositionIndex > 0;


        rightButton.interactable = currentPositionIndex < cameraPositions.Length - 1;
    }
}
