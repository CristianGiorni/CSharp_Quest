using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu principal: Jugar, Controles, Salir. Va en el objeto MenuCanvas de la escena MainMenu.
public class MainMenu : MonoBehaviour
{
    [Header("Escena del juego")]
    public string gameSceneName = "VerticalSlice";

    [Header("Paneles")]
    public GameObject mainPanel;
    public GameObject controlsPanel;

    [Header("Botones")]
    public Button playButton;
    public Button controlsButton;
    public Button quitButton;
    public Button backButton;

    void Awake()
    {
        Time.timeScale = 1f;

        playButton.onClick.AddListener(Play);
        controlsButton.onClick.AddListener(ShowControls);
        quitButton.onClick.AddListener(Quit);
        backButton.onClick.AddListener(ShowMain);

        // Sonido de clic en todos los botones
        playButton.onClick.AddListener(AudioManager.Click);
        controlsButton.onClick.AddListener(AudioManager.Click);
        quitButton.onClick.AddListener(AudioManager.Click);
        backButton.onClick.AddListener(AudioManager.Click);

        ShowMain();
    }

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowControls()
    {
        mainPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void ShowMain()
    {
        controlsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}