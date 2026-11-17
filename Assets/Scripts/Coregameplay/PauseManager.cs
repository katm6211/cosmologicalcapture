using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public GameObject pausePanel;          // optional UI panel (Resume / Quit buttons)

    public bool IsPaused { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetPaused(!IsPaused);
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;

        if (pausePanel != null) pausePanel.SetActive(paused);

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    public void Resume() => SetPaused(false);

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    void OnDestroy()
    {
        if (Instance == this) Time.timeScale = 1f;
    }
}