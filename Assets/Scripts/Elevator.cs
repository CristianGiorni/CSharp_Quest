using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Ascensor de salida: al activarse enciende las luces en verde, abre las puertas
// y, cuando el jugador entra, termina el nivel.
public class Elevator : MonoBehaviour
{
    [Header("Puertas (se deslizan hacia los costados)")]
    public Transform leftDoor;
    public Transform rightDoor;
    public float openDistance = 1.4f;
    public float openDuration = 1.2f;

    [Header("Luces indicadoras")]
    public Renderer[] lightRenderers;
    public Light[] realLights;
    public Color offColor = new Color(0.85f, 0.15f, 0.15f);
    public Color onColor = new Color(0.2f, 0.9f, 0.3f);

    [Header("Fin de nivel")]
    [Tooltip("Nombre de la escena del siguiente nivel (debe estar en Build Settings). Vacio = volver a jugar este nivel.")]
    public string nextSceneName = "";

    [Header("Eventos")]
    public UnityEvent onOpened;
    public UnityEvent onLevelCompleted;

    private float levelStartTime;
    private float levelSeconds;
    private bool opened;
    private bool completed;
    private Vector3 leftClosed;
    private Vector3 rightClosed;
    private Material[] lightMaterials;

    void Awake()
    {
        LevelStats.Reset();
        levelStartTime = Time.time;

        if (leftDoor != null) leftClosed = leftDoor.localPosition;
        if (rightDoor != null) rightClosed = rightDoor.localPosition;

        // .material crea una copia propia para cada luz
        lightMaterials = new Material[lightRenderers.Length];
        for (int i = 0; i < lightRenderers.Length; i++)
        {
            lightMaterials[i] = lightRenderers[i].material;
        }

        SetLights(offColor);
    }

    void Update()
    {
        if (completed && Input.GetKeyDown(KeyCode.Return))
        {
            string scene = string.IsNullOrEmpty(nextSceneName) ? SceneManager.GetActiveScene().name : nextSceneName;
            SceneManager.LoadScene(scene);
        }
    }

    // Se conecta al evento On Solved de la consola
    public void Activate()
    {
        if (opened) return;
        opened = true;
        AudioManager.Elevator();
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        SetLights(onColor);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            float k = Mathf.SmoothStep(0f, 1f, t);

            if (leftDoor != null) leftDoor.localPosition = leftClosed + Vector3.left * (openDistance * k);
            if (rightDoor != null) rightDoor.localPosition = rightClosed + Vector3.right * (openDistance * k);

            yield return null;
        }

        onOpened.Invoke();
    }

    private void SetLights(Color color)
    {
        foreach (Material m in lightMaterials)
        {
            if (m == null) continue;

            m.color = color;

            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * 2f);
            }
        }

        foreach (Light l in realLights)
        {
            if (l != null) l.color = color;
        }
    }

    // El trigger del ascensor esta dentro de la cabina
    void OnTriggerEnter(Collider other)
    {
        if (!opened || completed) return;
        if (other.GetComponentInParent<PlayerMovementGrid>() == null) return;

        CompleteLevel(other);
    }

    private void CompleteLevel(Collider playerCollider)
    {
        completed = true;
        levelSeconds = Time.time - levelStartTime;
        AudioManager.LevelComplete();

        PlayerMovementGrid movement = playerCollider.GetComponentInParent<PlayerMovementGrid>();
        PlayerInteraction interaction = movement.GetComponent<PlayerInteraction>();
        Rigidbody rb = movement.GetComponent<Rigidbody>();

        if (rb != null) rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        movement.enabled = false;
        if (interaction != null) interaction.enabled = false;

        onLevelCompleted.Invoke();
    }

    void OnGUI()
    {
        if (!completed) return;

        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float scale = Screen.height / 1080f;

        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.fontSize = Mathf.RoundToInt(64 * scale);
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.normal.textColor = new Color(0.4f, 1f, 0.5f);

        GUIStyle sub = new GUIStyle(GUI.skin.label);
        sub.fontSize = Mathf.RoundToInt(34 * scale);
        sub.alignment = TextAnchor.MiddleCenter;
        sub.normal.textColor = Color.white;

        int minutes = Mathf.FloorToInt(levelSeconds / 60f);
        int seconds = Mathf.FloorToInt(levelSeconds % 60f);
        int total = LevelStats.TotalMistakes;

        float y = Screen.height * 0.25f;
        GUI.Label(new Rect(0, y, Screen.width, 90 * scale), "NIVEL COMPLETADO", title);
        y += 130 * scale;

        GUI.Label(new Rect(0, y, Screen.width, 50 * scale), "Tiempo: " + minutes + ":" + seconds.ToString("00"), sub);
        y += 60 * scale;
        GUI.Label(new Rect(0, y, Screen.width, 50 * scale), "Errores: " + total, sub);
        y += 90 * scale;

        GUIStyle legend = new GUIStyle(sub);
        legend.fontStyle = FontStyle.Italic;
        legend.normal.textColor = new Color(1f, 0.9f, 0.5f);
        GUI.Label(new Rect(0, y, Screen.width, 50 * scale), "Equivocarse es parte de aprender. Pasa los puzles otra vez si queres practicar.", legend);
        y += 100 * scale;

        string hint = string.IsNullOrEmpty(nextSceneName) ? "Enter: jugar de nuevo" : "Enter: siguiente nivel";
        sub.fontSize = Mathf.RoundToInt(28 * scale);
        sub.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        GUI.Label(new Rect(0, y, Screen.width, 44 * scale), hint, sub);
    }
}