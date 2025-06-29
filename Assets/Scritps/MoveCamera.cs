using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MoveCamera : MonoBehaviour
{
    [Header("Posiciones de la c�mara")]
    public Transform[] cameraPositions;

    [Header("Botones de navegaci�n")]
    public Button leftButton;
    public Button rightButton;


    [Header("Movimiento Vertical")]
   
    public float verticalMoveSpeed = 0.1f; 
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;

    private int currentPositionIndex = 0;
    private Vector3 originalPosition; 
    private float currentVerticalOffset = 0f;

    void Start()
    {
        if (cameraPositions == null || cameraPositions.Length == 0)
        {
            Debug.LogError("No hay posiciones de c�mara definidas!");
            return;
        }

        
        originalPosition = cameraPositions[currentPositionIndex].position;

        MoveCameraToPosition(currentPositionIndex);
        UpdateButtons();
    }

    void Update()
    {
       
        if (Input.GetKey(upKey))
        {
            MoveVertical(verticalMoveSpeed);
        }
        else if (Input.GetKey(downKey))
        {
            MoveVertical(-verticalMoveSpeed);
        }
    }

    public void MoveLeft()
    {
        if (currentPositionIndex > 0)
        {
            currentPositionIndex--;
            MoveCameraToPosition(currentPositionIndex);
            Debug.Log("Izquierda");
            UpdateButtons();
        }
    }

    public void MoveRight()
    {
        if (currentPositionIndex < cameraPositions.Length - 1)
        {
            currentPositionIndex++;
            MoveCameraToPosition(currentPositionIndex);
            Debug.Log("Derecha");
            UpdateButtons();
        }
    }



    private void MoveVertical(float amount)
    {
     
        float newOffset = Mathf.Clamp(currentVerticalOffset + amount, -53.8f , - 1.02f);

  
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
            Debug.Log("Se movi� la c�mara");
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
