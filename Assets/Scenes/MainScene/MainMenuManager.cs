using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
  public void LoadScene(string MainScene)
    {
        SceneManager.LoadScene(MainScene);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
