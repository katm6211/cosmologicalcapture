using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
  public void LoadScene(string CoreGameplay)
    {
        SceneManager.LoadScene(CoreGameplay);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
