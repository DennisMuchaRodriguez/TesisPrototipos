using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Configuración de escena")]

    public string sceneName;

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
}
