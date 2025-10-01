using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Referencias de Botones")]
    public List<Button> menuButtons;

    [Header("Configuración de Animación")]
    public float delayBetweenButtons = 0.2f;
    public float animationDuration = 0.8f;
    public Ease easeType = Ease.OutBack;

    private List<Vector2> originalPositions = new List<Vector2>();
    private List<RectTransform> buttonTransforms = new List<RectTransform>();

    void Start()
    {
        
        InitializeButtons();

      
        AnimateButtonsIn();
    }

    void InitializeButtons()
    {
        foreach (Button button in menuButtons)
        {
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                buttonTransforms.Add(rectTransform);
                originalPositions.Add(rectTransform.anchoredPosition);

          
                Vector2 offScreenPosition = new Vector2(
                    Screen.width + rectTransform.rect.width,
                    rectTransform.anchoredPosition.y
                );
                rectTransform.anchoredPosition = offScreenPosition;
            }
        }
    }

    void AnimateButtonsIn()
    {
        for (int i = 0; i < buttonTransforms.Count; i++)
        {
            if (buttonTransforms[i] != null)
            {
              
                buttonTransforms[i].localScale = Vector3.zero;

            
                Sequence buttonSequence = DOTween.Sequence();

          
                buttonSequence
                    .Append(buttonTransforms[i].DOAnchorPos(originalPositions[i], animationDuration)
                        .SetEase(easeType))
                    .Join(buttonTransforms[i].DOScale(1f, animationDuration)
                        .SetEase(easeType))
                    .SetDelay(i * delayBetweenButtons);
            }
        }
    }

 
    public void AnimateButtonsOut()
    {
        for (int i = 0; i < buttonTransforms.Count; i++)
        {
            if (buttonTransforms[i] != null)
            {
                Vector2 offScreenPosition = new Vector2(
                    Screen.width + buttonTransforms[i].rect.width,
                    buttonTransforms[i].anchoredPosition.y
                );

                buttonTransforms[i]
                    .DOAnchorPos(offScreenPosition, animationDuration)
                    .SetEase(Ease.InBack)
                    .SetDelay(i * delayBetweenButtons);
            }
        }
    }

    
    public void ResetAnimations()
    {
        
        DOTween.KillAll();


        InitializeButtons();
        AnimateButtonsIn();
    }

    void OnDestroy()
    {
       
        DOTween.KillAll();
    }
}