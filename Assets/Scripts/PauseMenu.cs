using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pausa con Esc + recordatorio de controles al empezar. Va en el objeto PauseCanvas del nivel.
public class PauseMenu : MonoBehaviour
{
    [Header("Panel de pausa")]
    public GameObject panel;
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton;
    public Button quitButton;

    [Header("Escenas")]
    public string menuSceneName = "MainMenu";

    [Header("Recordatorio de controles al empezar")]
    public float controlsHintSeconds = 12f;

    private bool paused;
    private bool terminalWasOpen;
    private float startTime;

    void Awake()
    {
        Time.timeScale = 1f;
        startTime = Time.realtimeSinceStartup;

        panel.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);
        menuButton.onClick.AddListener(GoToMenu);
        quitButton.onClick.AddListener(Quit);

        // Sonido de clic en todos los botones
        resumeButton.onClick.AddListener(AudioManager.Click);
        restartButton.onClick.AddListener(AudioManager.Click);
        menuButton.onClick.AddListener(AudioManager.Click);
        quitButton.onClick.AddListener(AudioManager.Click);
    }

    void Update()
    {
        bool terminalOpen = TerminalUI.Instance != null && TerminalUI.Instance.IsOpen;

        // Esc cierra la terminal si esta abierta; solo pausa si no lo estaba
        if (Input.GetKeyDown(KeyCode.Escape) && !terminalOpen && !terminalWasOpen)
        {
            if (paused) Resume();
            else Pause();
        }

        terminalWasOpen = terminalOpen;
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        panel.SetActive(true);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        panel.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnGUI()
    {
        if (paused) return;
        if (Time.realtimeSinceStartup - startTime > controlsHintSeconds) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 22;
        style.alignment = TextAnchor.UpperRight;
        style.normal.textColor = new Color(1f, 1f, 1f, 0.9f);

        GUI.Label(new Rect(Screen.width - 640, 20, 620, 160),
            "WASD: mover\nE: interactuar\nQ o 1-3: cambiar de caja\nEsc: pausa", style);
    }
}