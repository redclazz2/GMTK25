using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    [SerializeField] private string sceneName = "GameplayScene";

    public void LoadNext()
    {
        SceneManager.LoadScene(sceneName);
    }
}
