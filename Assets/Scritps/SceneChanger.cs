using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Configuración de escena")]
    public string sceneName;

    [Header("Popups")]
    public GameObject popupPanel1;
    public GameObject popupPanel2;

    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("No se ha especificado un nombre de escena!");
        }
    }

    public void ChangeScene(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            SceneManager.LoadScene(name);
        }
    }

    public void OpenPopup1()
    {
        if (popupPanel1 != null)
        {
            popupPanel1.SetActive(true);
            popupPanel1.transform.localScale = Vector3.zero;
            popupPanel1.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }
    }

    public void OpenPopup2()
    {
        if (popupPanel2 != null)
        {
            popupPanel2.SetActive(true);
            popupPanel2.transform.localScale = Vector3.zero;
            popupPanel2.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }
    }


    public void ClosePopup1()
    {
        if (popupPanel1 != null)
        {
            popupPanel1.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => popupPanel1.SetActive(false));
        }
    }

    public void ClosePopup2()
    {
        if (popupPanel2 != null)
        {
            popupPanel2.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => popupPanel2.SetActive(false));
        }
    }

 
    public void QuitGame()
    {
        Application.Quit();


    }
}
