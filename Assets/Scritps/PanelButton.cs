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
        FinishSimulation
    }

    [Header("Configuración")]
    public ButtonType buttonType;
    public float rotationAmount = 45f;
    public float rotationToggle = 90f;
    private bool _isActive = true;
    private Image _image;

    void Start()
    {
        _image = GetComponent<Image>();

       
        if (buttonType == ButtonType.ToggleAndAdd)
        {
            SimulationManager.Instance.RegisterToggleButton(this);
        }
    }

    void OnDestroy()
    {
        
        if (buttonType == ButtonType.ToggleAndAdd && SimulationManager.Instance != null)
        {
            SimulationManager.Instance.UnregisterToggleButton(this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (buttonType)
        {
            case ButtonType.RotateAndAdd:
                transform.Rotate(0f, 0f, rotationAmount);
                SimulationManager.Instance.CurrentGeneratorData.Voltage += 1;
                break;

            case ButtonType.ToggleAndAdd:
               
                _isActive = !_isActive;
                transform.Rotate(0f, 0f, rotationToggle);
                _image.color = _isActive ? Color.white : Color.gray;
                SimulationManager.Instance.UpdateCurrentFromToggleButtons();
                break;

            case ButtonType.FinishSimulation:
                SimulationManager.Instance.FinishSimulation();
                break;
        }
    }

    public bool IsActive() => _isActive;
}