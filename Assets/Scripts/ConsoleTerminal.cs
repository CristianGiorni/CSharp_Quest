using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TerminalOptionData
{
    public DataType type;
    public string value;
    [TextArea(1, 3)] public string feedback;

    public TerminalOptionData() { }

    public TerminalOptionData(DataType type, string value, string feedback = "")
    {
        this.type = type;
        this.value = value;
        this.feedback = feedback;
    }
}

// Consola del mundo. Al acercarse y apretar E abre la terminal flotante.
public class ConsoleTerminal : MonoBehaviour
{
    [Header("Codigo que se muestra (los textos vacios se ocultan)")]
    [TextArea(2, 6)] public string codeTop = "// Falta declarar la variable de energia\n// Elegi su tipo de dato";
    public string codeBeforeSlot = "";
    public string codeAfterSlot = " energia = 100;";
    [TextArea(2, 6)] public string codeBottom = "";

    [Header("Respuesta correcta")]
    [Tooltip("Tildado: el hueco solo acepta piezas del tipo indicado (puzles de valores). Destildado: acepta cualquiera y se valida al ejecutar (puzles de elegir el tipo).")]
    public bool checkTypeOnDrop = false;
    public DataType expectedType = DataType.Int;
    public string expectedValue = "int";

    [Header("Piezas disponibles en esta terminal")]
    public TerminalOptionData[] options = new TerminalOptionData[]
    {
        new TerminalOptionData(DataType.Int, "int"),
        new TerminalOptionData(DataType.Bool, "bool", "bool solo guarda true o false. 100 es un numero entero."),
        new TerminalOptionData(DataType.Float, "float", "float sirve para decimales (como 2.5f). 100 es un entero: para eso se usa int."),
        new TerminalOptionData(DataType.String, "string", "string guarda texto, como \"hola\". 100 es un numero, no un texto."),
        new TerminalOptionData(DataType.Float, "double", "double guarda decimales con mas precision. 100 es un entero: para eso se usa int.")
    };

    [Header("Mensaje por defecto cuando la respuesta es incorrecta")]
    public string wrongLogicMessage = "El codigo no funciona asi. Revisalo e intenta con otra pieza.";

    [Header("Interaccion")]
    public float interactRadius = 2f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Eventos")]
    public UnityEvent onSolved;
    public UnityEvent onWrongLogic;

    [Header("Corazones (intentos por terminal)")]
    public int maxHearts = 3;
    [Tooltip("Segundos de espera cuando se acaban los corazones")]
    public float lockSeconds = 10f;
    [TextArea(1, 3)] public string hint = "Pista: 100 es un numero entero, sin decimales ni texto.";
    [Tooltip("Tildado: muestra la pista mientras la terminal se reinicia. Destildado: solo la cuenta regresiva, porque cada error ya muestra su explicacion.")]
    public bool showHintOnLock = false;

    public int HeartsLeft { get; set; }
    public float LockedUntil { get; set; }
    public bool IsLocked { get { return LockedUntil > 0f && Time.time < LockedUntil; } }

    private Transform player;
    private bool solved;
    private bool near;

    void Awake()
    {
        HeartsLeft = maxHearts;
    }

    void Start()
    {
        PlayerMovementGrid p = FindFirstObjectByType<PlayerMovementGrid>();
        if (p != null) player = p.transform;
    }

    void Update()
    {
        near = false;

        if (solved || player == null) return;
        if (TerminalUI.Instance == null || TerminalUI.Instance.IsOpen) return;

        Vector3 diff = player.position - transform.position;
        diff.y = 0f;
        near = diff.magnitude <= interactRadius;

        if (near && Input.GetKeyDown(interactKey))
        {
            TerminalUI.Instance.Open(this);
        }
    }

    // Lo llama la terminal cuando el jugador resuelve el codigo
    public void Solve()
    {
        solved = true;
        onSolved.Invoke();
    }

    void OnGUI()
    {
        if (!near) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(20, Screen.height - 100, 1400, 40), interactKey + ": usar la consola", style);
    }
}